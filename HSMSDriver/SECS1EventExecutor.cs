using System;
using System.Collections.Generic;
using System.Threading;

namespace HSMSDriver
{
	internal class SECS1EventExecutor : AbstractThread
	{
		private Queue<SECS1EventArgs> eventQueue = new Queue<SECS1EventArgs>();

		private Dictionary<long, SECSMessage> msgWaitingReply = new Dictionary<long, SECSMessage>();

		private string name;

		private SECS1Port secs1;

		private object syncObject = new object();

		public event SECS1Port.SECS1EventHandler OnSECS1Event;

		public override string Name
		{
			get
			{
				return this.name + "#SECS1EventHandler";
			}
		}

		internal SECS1EventExecutor(string name, SECS1Port secs1)
		{
			this.name = name;
			this.secs1 = secs1;
		}

		private void EventInternalHandler(SECS1EventArgs args)
		{
			switch (args.EventType)
			{
			case SECSEventType.PrimarySent:
				//this.logger.Warn(string.Format("[SEND] {0}", SecsItem2Str.GetSecsMessageStr(args.Trans.Primary)));
				break;
			case SECSEventType.PrimaryRcvd:
				//this.logger.Warn(string.Format("[RECV] {0}", SecsItem2Str.GetSecsMessageStr(args.Trans.Primary)));
				break;
			case SECSEventType.SecondarySent:
				//this.logger.Warn(string.Format("[SEND] {0}", SecsItem2Str.GetSecsMessageStr(args.Trans.Secondary)));
				break;
			case SECSEventType.SecondaryRcvd:
				//this.logger.Warn(string.Format("[RECV] {0}", SecsItem2Str.GetSecsMessageStr(args.Trans.Secondary)));
				break;
			default:
				//this.logger.Warn(string.Format("Error happened, code={0}, description={1}", args.ErrorCode, args.ErrorMsg));
				if (args.Trans != null && args.Trans.Primary != null)
				{
					//this.logger.Debug(string.Format("SECS Message Happened Error: {0}", SecsItem2Str.GetSecsMessageStr(args.Trans.Primary)));
				}
				if (args.Trans != null && args.Trans.Secondary != null)
				{
					//this.logger.Debug(string.Format("SECS Message Happened Error: {0}", SecsItem2Str.GetSecsMessageStr(args.Trans.Secondary)));
				}
				break;
			}
			if (this.OnSECS1Event != null)
			{
				this.OnSECS1Event(args.EventType, args.Trans, args.ErrorCode, args.ErrorMsg);
			}
		}

		internal void NofityRECV(SECSMessage msg)
		{
			lock (this.syncObject)
			{
				SECS1EventArgs args = new SECS1EventArgs
				{
					ErrorCode = SECSErrors.None,
					ErrorMsg = ""
				};
				if (msg.Function % 2 == 0)
				{
					if (this.msgWaitingReply.ContainsKey(msg.SystemBytes))
					{
						SECSMessage message = this.msgWaitingReply[msg.SystemBytes];
						this.secs1.StopTimer(message);
						this.msgWaitingReply.Remove(msg.SystemBytes);
						if (msg.Stream < 1 || msg.Function < 1)
						{
							SECSTransaction transaction = message.Transaction;
							transaction.Secondary = msg;
							args.EventType = SECSEventType.Error;
							args.ErrorCode = SECSErrors.RcvdAbortMessage;
							args.ErrorMsg = SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.RcvdAbortMessage);
							args.Trans = transaction;
						}
						else
						{
							SECSTransaction transaction = message.Transaction;
							transaction.Secondary = msg;
							args.EventType = SECSEventType.SecondaryRcvd;
							args.Trans = transaction;
						}
					}
					else
					{
						SECSTransaction transaction = new SECSTransaction
						{
							Primary = null,
							Secondary = msg
						};
						args.EventType = SECSEventType.Error;
						args.ErrorCode = SECSErrors.RcvdUnknownMessage;
						args.ErrorMsg = SECSErrorsMessage.GetSECSErrorMessage(args.ErrorCode);
						args.Trans = transaction;
					}
				}
				else
				{
					SECSTransaction transaction = new SECSTransaction
					{
						Primary = msg,
						SystemBytes = msg.SystemBytes,
						Secondary = null,
						ExpectReply = msg.WBit,
						DeviceID = msg.DeviceIdID
					};
					args.EventType = SECSEventType.PrimaryRcvd;
					args.Trans = transaction;
				}
				this.eventQueue.Enqueue(args);
			}
		}

		internal void NotifyERROR(SECSErrors err, SECSMessage msg)
		{
			lock (this.syncObject)
			{
				SECS1EventArgs args = new SECS1EventArgs
				{
					EventType = SECSEventType.Error,
					ErrorCode = err,
					ErrorMsg = SECSErrorsMessage.GetSECSErrorMessage(err)
				};
				if (msg != null)
				{
					args.Trans = msg.Transaction;
				}
				else
				{
					args.Trans = null;
				}
				this.eventQueue.Enqueue(args);
			}
		}

		internal void NotifyEVENT(SECSEventType type, SECSTransaction trans, SECSErrors err)
		{
			lock (this.syncObject)
			{
				SECS1EventArgs args = new SECS1EventArgs
				{
					EventType = type,
					ErrorCode = err,
					ErrorMsg = SECSErrorsMessage.GetSECSErrorMessage(err),
					Trans = trans
				};
				this.eventQueue.Enqueue(args);
			}
		}

		internal void NotifySENT(bool isPrimarySent, SECSMessage msg)
		{
			lock (this.syncObject)
			{
				SECS1EventArgs args = new SECS1EventArgs
				{
					ErrorCode = SECSErrors.None,
					ErrorMsg = "",
					Trans = msg.Transaction
				};
				if (isPrimarySent)
				{
					args.Trans.DeviceID = msg.DeviceIdID;
					args.Trans.SystemBytes = msg.SystemBytes;
					args.Trans.ExpectReply = msg.WBit;
					args.EventType = SECSEventType.PrimarySent;
				}
				else
				{
					args.EventType = SECSEventType.SecondarySent;
				}
				this.eventQueue.Enqueue(args);
			}
		}

		protected override void Run()
		{
			while (this.running)
			{
				try
				{
					if (this.eventQueue.Count > 0)
					{
						lock (this.syncObject)
						{
							SECS1EventArgs args = this.eventQueue.Dequeue();
							this.EventInternalHandler(args);
							continue;
						}
					}
					Thread.Sleep(50);
				}
				catch (Exception exception)
				{
					this.logger.Error("SECS1EventHandler#Run", exception);
				}
			}
		}
	}
}
