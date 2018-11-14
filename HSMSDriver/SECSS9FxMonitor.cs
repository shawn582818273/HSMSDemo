using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace HSMSDriver
{
	internal class SECSS9FxMonitor : AbstractThread
	{
		private class ConversactionItem
		{
			public DateTime DeadLine
			{
				get;
				set;
			}

			public string EDID
			{
				get;
				set;
			}

			public SECSTransaction Transaction
			{
				get;
				set;
			}

			public string Transaction1
			{
				get;
				set;
			}

			public List<string> Transaction2
			{
				get;
				set;
			}

			public string Transaction2Str
			{
				get
				{
					if (this.Transaction2 == null)
					{
						return "null";
					}
					StringBuilder builder = new StringBuilder();
					foreach (string str in this.Transaction2)
					{
						builder.Append(str);
						builder.Append(",");
					}
					return builder.ToString().TrimEnd(new char[]
					{
						','
					});
				}
			}

			public ConversactionItem()
			{
			}

			public ConversactionItem(string trans1, List<string> trans2, string edid)
			{
				this.Transaction1 = trans1;
				this.Transaction2 = trans2;
				this.EDID = edid;
			}
		}

		private readonly Dictionary<string, SECSS9FxMonitor.ConversactionItem> conversactionConfigure = new Dictionary<string, SECSS9FxMonitor.ConversactionItem>();

		private readonly Dictionary<string, string> conversactionReflect = new Dictionary<string, string>();

		private readonly List<SECSS9FxMonitor.ConversactionItem> conversationList = new List<SECSS9FxMonitor.ConversactionItem>();

		private readonly SECSPort secsPort;

		private readonly object syncConversation = new object();

		private bool IsHost
		{
			get
			{
				if (this.secsPort.PortType != eSECS_PORT_TYPE.HSMS)
				{
					return this.secsPort.Secs1Parameters.IsHost;
				}
				return this.secsPort.HsmsParameters.IsHost;
			}
		}

		public override string Name
		{
			get
			{
				return "S9FxMonitor";
			}
		}

		public SECSS9FxMonitor(SECSPort port)
		{
			if (port == null)
			{
				throw new ArgumentNullException("port", "SECSPort Parameter is Null.");
			}
			this.secsPort = port;
			this.InitliazeConversationConfigure();
		}

		private void AddConversationTimeout(SECSEventType eventtype, SECSTransaction trans, SECSErrors err)
		{
			if (eventtype == SECSEventType.SecondarySent && err == SECSErrors.None && trans != null && trans.Secondary != null)
			{
				string str = string.Format("S{0}F{1}", trans.Secondary.Stream, trans.Secondary.Function);
				if (this.conversactionConfigure.ContainsKey(str))
				{
					if (str == "S3F12")
					{
						if (trans.Secondary.Root.Item(2).IsEmpty)
						{
							return;
						}
						if ((int)trans.Secondary.Root.Item(2).Value != 2)
						{
							return;
						}
					}
					else
					{
						if (trans.Secondary.Root.IsEmpty)
						{
							return;
						}
						if ((int)trans.Secondary.Root.Value != 0)
						{
							return;
						}
					}
					SECSS9FxMonitor.ConversactionItem item = new SECSS9FxMonitor.ConversactionItem
					{
						Transaction1 = this.conversactionConfigure[str].Transaction1,
						Transaction2 = this.conversactionConfigure[str].Transaction2,
						Transaction = trans
					};
					try
					{
						item.EDID = trans.Primary.Root.Item(1).Value.ToString();
						item.EDID = this.conversactionConfigure[str].EDID + ": " + item.EDID;
					}
					catch (Exception)
					{
						item.EDID = this.conversactionConfigure[str].EDID;
					}
					lock (this.syncConversation)
					{
						item.DeadLine = DateTime.Now.AddSeconds(45.0);
						this.conversationList.Add(item);
					}
				}
			}
		}

		private void CheckConversationTimeout()
		{
			List<int> list = new List<int>();
			for (int i = 0; i < this.conversationList.Count; i++)
			{
				if (this.conversationList[i].DeadLine < DateTime.Now)
				{
					list.Add(i);
				}
			}
			foreach (int num2 in list)
			{
				SECSS9FxMonitor.ConversactionItem item = this.conversationList[num2];
				this.conversationList.RemoveAt(num2);
				SECSTransaction trans = this.secsPort.Library.FindTransaction("S9F13");
				trans.Primary.Root.Item(1).Value = item.Transaction2Str;
				trans.Primary.Root.Item(2).Value = item.EDID;
				this.secsPort.Send(trans);
				this.secsPort.CallSECSEvent(SECSEventType.Error, item.Transaction, SECSErrors.ConversationTimeout, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ConversationTimeout));
			}
		}

		private bool CheckMsgFormat(SECSMessage msg, IEnumerable<SECSMessage> list)
		{
			foreach (SECSMessage message in list)
			{
				if (msg.Root == null && message.Root == null)
				{
					msg.Name = message.Name;
					msg.Description = message.Description;
					bool result = true;
					return result;
				}
				if (msg.Root == null || message.Root == null)
				{
					bool result = false;
					return result;
				}
				SECSItem root = msg.Root;
				SECSItem format = message.Root;
				if (this.secsPort.Library.CheckSecsItemFormat(ref root, ref format))
				{
					msg.Name = message.Name;
					msg.Description = message.Description;
					bool result = true;
					return result;
				}
			}
			return false;
		}

		private bool CheckS9F1Exception(SECSEventType eventtype, SECSTransaction trans)
		{
			if (eventtype == SECSEventType.PrimaryRcvd && trans != null && trans.Primary != null && trans.Primary.DeviceIdID != this.secsPort.DeviceID)
			{
				this.SendS9Fx(1, trans.Primary.Header);
				this.secsPort.CallSECSEvent(SECSEventType.Error, trans, SECSErrors.UnrecognizedDeviceID, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.UnrecognizedDeviceID));
				return false;
			}
			if (eventtype == SECSEventType.SecondaryRcvd && trans != null && trans.Secondary != null && trans.Secondary.DeviceIdID != this.secsPort.DeviceID)
			{
				this.SendS9Fx(1, trans.Secondary.Header);
				this.secsPort.CallSECSEvent(SECSEventType.Error, trans, SECSErrors.UnrecognizedDeviceID, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.UnrecognizedDeviceID));
				return false;
			}
			return true;
		}

		private bool CheckS9F357Exception(SECSEventType eventtype, SECSTransaction trans)
		{
			SECSMessage msg = null;
			if (eventtype == SECSEventType.PrimaryRcvd)
			{
				msg = trans.Primary;
			}
			else if (eventtype == SECSEventType.SecondaryRcvd)
			{
				msg = trans.Secondary;
			}
			if (this.secsPort.Library != null && msg != null)
			{
				if (!this.secsPort.Library.FindStream(msg.Stream))
				{
					this.SendS9Fx(3, msg.Header);
					this.secsPort.CallSECSEvent(SECSEventType.Error, trans, SECSErrors.UnrecognizedStreamType, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.UnrecognizedStreamType));
					return false;
				}
				if (!this.secsPort.Library.FindFunction(msg.Stream, msg.Function))
				{
					this.SendS9Fx(5, msg.Header);
					this.secsPort.CallSECSEvent(SECSEventType.Error, trans, SECSErrors.UnrecognizedFunctionType, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.UnrecognizedFunctionType));
					return false;
				}
				List<SECSMessage> list = this.secsPort.Library.FindMessage(msg.Stream, msg.Function);
				if (!this.CheckMsgFormat(msg, list))
				{
					this.SendS9Fx(7, msg.Header);
					this.secsPort.CallSECSEvent(SECSEventType.Error, trans, SECSErrors.IllegalData, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.IllegalData));
					return false;
				}
			}
			return true;
		}

		private void InitliazeConversationConfigure()
		{
			this.conversactionConfigure.Add("S2F2", new SECSS9FxMonitor.ConversactionItem("S2F2", new List<string>
			{
				"S2F3"
			}, "SPID"));
			this.conversactionReflect.Add("S2F3", "S2F2");
			this.conversactionConfigure.Add("S2F40", new SECSS9FxMonitor.ConversactionItem("S2F40", new List<string>
			{
				"S2F23",
				"S2F33",
				"S2F35",
				"S2F49"
			}, "DATAID"));
			this.conversactionReflect.Add("S2F23", "S2F40");
			this.conversactionReflect.Add("S2F33", "S2F40");
			this.conversactionReflect.Add("S2F35", "S2F40");
			this.conversactionReflect.Add("S2F49", "S2F40");
			this.conversactionConfigure.Add("S3F12", new SECSS9FxMonitor.ConversactionItem("S3F12", new List<string>
			{
				"S3F13",
				"S3F17"
			}, "PTN"));
			this.conversactionReflect.Add("S3F13", "S3F12");
			this.conversactionReflect.Add("S3F17", "S3F12");
			this.conversactionConfigure.Add("S4F26", new SECSS9FxMonitor.ConversactionItem("S4F26", new List<string>
			{
				"S4F19"
			}, "DATAID"));
			this.conversactionReflect.Add("S4F19", "S4F26");
			this.conversactionConfigure.Add("S6F6", new SECSS9FxMonitor.ConversactionItem("S6F6", new List<string>
			{
				"S6F25"
			}, "DATAID"));
			this.conversactionReflect.Add("S6F25", "S6F6");
			this.conversactionConfigure.Add("S7F2", new SECSS9FxMonitor.ConversactionItem("S7F2", new List<string>
			{
				"S7F3",
				"S7F31"
			}, "PPID"));
			this.conversactionReflect.Add("S7F3", "S7F2");
			this.conversactionReflect.Add("S7F31", "S7F2");
			this.conversactionConfigure.Add("S13F12", new SECSS9FxMonitor.ConversactionItem("S13F12", new List<string>
			{
				"S13F13",
				"S13F15"
			}, "DATAID"));
			this.conversactionReflect.Add("S13F13", "S13F12");
			this.conversactionReflect.Add("S13F15", "S13F12");
			this.conversactionConfigure.Add("S14F24", new SECSS9FxMonitor.ConversactionItem("S14F24", new List<string>
			{
				"S14F19"
			}, "DATAID"));
			this.conversactionReflect.Add("S14F19", "S14F24");
			this.conversactionConfigure.Add("S15F2", new SECSS9FxMonitor.ConversactionItem("S15F2", new List<string>
			{
				"S15F13",
				"S15F15",
				"S15F23",
				"S15F25",
				"S15F27",
				"S15F29",
				"S15F33",
				"S15F35",
				"S15F37",
				"S15F39",
				"S15F41",
				"S15F43",
				"S15F45",
				"S15F47"
			}, "DATAID"));
			this.conversactionReflect.Add("S15F13", "S15F2");
			this.conversactionReflect.Add("S15F15", "S15F2");
			this.conversactionReflect.Add("S15F23", "S15F2");
			this.conversactionReflect.Add("S15F25", "S15F2");
			this.conversactionReflect.Add("S15F27", "S15F2");
			this.conversactionReflect.Add("S15F29", "S15F2");
			this.conversactionReflect.Add("S15F33", "S15F2");
			this.conversactionReflect.Add("S15F35", "S15F2");
			this.conversactionReflect.Add("S15F37", "S15F2");
			this.conversactionReflect.Add("S15F39", "S15F2");
			this.conversactionReflect.Add("S15F41", "S15F2");
			this.conversactionReflect.Add("S15F43", "S15F2");
			this.conversactionReflect.Add("S15F45", "S15F2");
			this.conversactionReflect.Add("S15F47", "S15F2");
			this.conversactionConfigure.Add("S16F2", new SECSS9FxMonitor.ConversactionItem("S16F2", new List<string>
			{
				"S16F11",
				"S16F13",
				"S16F15"
			}, "DATAID"));
			this.conversactionReflect.Add("S16F11", "S16F2");
			this.conversactionReflect.Add("S16F13", "S16F2");
			this.conversactionReflect.Add("S16F15", "S16F2");
		}

		public bool PreHandleSECSEvent(SECSEventType eventtype, SECSTransaction trans, SECSErrors err)
		{
			this.UpdateMatchedSecondary(eventtype, trans);
			if (!this.IsHost)
			{
				if (!this.CheckS9F1Exception(eventtype, trans))
				{
					return false;
				}
				if (!this.CheckS9F357Exception(eventtype, trans))
				{
					return false;
				}
				if (eventtype == SECSEventType.Error && err == SECSErrors.T3TimeOut)
				{
					this.SendS9Fx(9, trans.Primary.Header);
					return true;
				}
				this.AddConversationTimeout(eventtype, trans, err);
				this.RemoveConversationTimeout(eventtype, trans, err);
			}
			return true;
		}

		private void RemoveConversationTimeout(SECSEventType eventtype, SECSTransaction trans, SECSErrors err)
		{
			if (eventtype == SECSEventType.PrimaryRcvd && trans != null && trans.Primary != null && err == SECSErrors.None)
			{
				string str = string.Format("S{0}F{1}", trans.Primary.Stream, trans.Primary.Function);
				if (this.conversactionReflect.ContainsKey(str))
				{
					lock (this.syncConversation)
					{
						int num = -1;
						for (int i = 0; i < this.conversationList.Count; i++)
						{
							if (this.conversationList[i].Transaction1 == this.conversactionReflect[str])
							{
								num = i;
								break;
							}
						}
						if (num != -1)
						{
							this.conversationList.RemoveAt(num);
						}
					}
				}
			}
		}

		protected override void Run()
		{
			while (this.running)
			{
				try
				{
					if (this.conversationList.Count > 0)
					{
						lock (this.syncConversation)
						{
							this.CheckConversationTimeout();
							continue;
						}
					}
					Thread.Sleep(100);
				}
				catch (Exception)
				{
					Thread.Sleep(100);
				}
			}
		}

		private void SendS9Fx(int aFx, byte[] hdr)
		{
			SECSTransaction trans = new SECSTransaction();
			SECSMessage message = new SECSMessage();
			trans.Primary = message;
			message.Transaction = trans;
			message.DeviceIdID = this.secsPort.DeviceID;
			message.Stream = 9;
			message.Function = aFx;
			message.WBit = false;
			trans.DeviceID = message.DeviceIdID;
			trans.ExpectReply = message.WBit;
			SECSItem item = (aFx == 9) ? new SECSItem(eSECS_FORMAT.BINARY, "SHEAD") : new SECSItem(eSECS_FORMAT.BINARY, "MHEAD");
			item.Value = hdr;
			message.Root = item;
			this.secsPort.Send(trans);
		}

		private void UpdateMatchedSecondary(SECSEventType eventtype, SECSTransaction trans)
		{
			if (eventtype == SECSEventType.PrimaryRcvd && trans != null && trans.Primary != null && this.secsPort.Library != null && trans.Primary.WBit)
			{
				SECSTransaction transaction = this.secsPort.Library.FindTransaction(string.Format("S{0}F{1}", trans.Primary.Stream, trans.Primary.Function));
				if (transaction != null)
				{
					trans.Secondary = transaction.Secondary;
				}
			}
		}
	}
}
