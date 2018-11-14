using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;

namespace HSMSDriver
{
	internal class MessageAbstractSECSLibrary : AbstractSECSLibrary
	{
		private readonly List<SECSMessage> list;

		private XDocument xmlDoc;

		public IList<SECSMessage> SECSMessageList
		{
			get
			{
				return this.list;
			}
		}

		public MessageAbstractSECSLibrary() : this("TTSKY AbstractSECSLibrary", "TTSKY SECS Default Libray")
		{
		}

		public MessageAbstractSECSLibrary(string name, string desc) : base(name, desc)
		{
			this.list = new List<SECSMessage>();
			this.xmlDoc = new XDocument();
		}

		public void AddMessage(SECSMessage msg)
		{
			this.list.Add(msg);
		}

		public override void AddTransaction(SECSTransaction trans)
		{
			throw new NotSupportedException("This SECS Library's Format Not Support AddTransaction Function.");
		}

		internal override bool CheckSecsItemFormat(ref SECSItem rcvd, ref SECSItem format)
		{
			bool result;
			try
			{
				if (rcvd.Format != format.Format)
				{
					result = false;
				}
				else if (format.Format == eSECS_FORMAT.LIST)
				{
					if (format.Fixed)
					{
						if (rcvd.ItemCount != format.Length)
						{
							result = false;
						}
						else
						{
							for (int i = 1; i <= format.ItemCount; i++)
							{
								SECSItem item = format.Item(i);
								SECSItem item2 = rcvd.Item(i);
								if (!this.CheckSecsItemFormat(ref item2, ref item))
								{
									result = false;
									return result;
								}
							}
							rcvd.Name = format.Name;
							rcvd.Description = format.Description;
							result = true;
						}
					}
					else if (rcvd.ItemCount > format.Length)
					{
						result = false;
					}
					else
					{
						for (int j = 1; j <= rcvd.ItemCount; j++)
						{
							SECSItem item3 = format.Item(1);
							SECSItem item4 = rcvd.Item(j);
							if (!this.CheckSecsItemFormat(ref item4, ref item3))
							{
								result = false;
								return result;
							}
						}
						rcvd.Name = format.Name;
						rcvd.Description = format.Description;
						result = true;
					}
				}
				else if (format.Format == eSECS_FORMAT.ASCII)
				{
					string str = (rcvd.Value as string) ?? "";
					if (format.Fixed)
					{
						if (str.Length > format.Length)
						{
							result = false;
						}
						else
						{
							result = true;
						}
					}
					else if (str.Length > format.Length)
					{
						result = false;
					}
					else
					{
						result = true;
					}
				}
				else
				{
					result = true;
				}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		public static SECSMessage DeepCopy(SECSMessage msg)
		{
			object obj2;
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, msg);
				stream.Seek(0L, SeekOrigin.Begin);
				obj2 = formatter.Deserialize(stream);
				stream.Close();
			}
			return (SECSMessage)obj2;
		}

		internal override bool FindFunction(int stream, int function)
		{
			return this.list.Any((SECSMessage t) => t.Stream == stream && t.Function == function);
		}

		public override SECSMessage FindMessage(string name)
		{
			return (from msg in this.list
			where msg.Name == name
			select MessageAbstractSECSLibrary.DeepCopy(msg)).FirstOrDefault<SECSMessage>();
		}

		internal override List<SECSMessage> FindMessage(int stream, int function)
		{
			return (from t in this.list
			where t.Stream == stream && t.Function == function
			select MessageAbstractSECSLibrary.DeepCopy(t)).ToList<SECSMessage>();
		}

		public SECSMessage FindMessage(int stream, int function, string direction)
		{
			foreach (SECSMessage message in from t in this.list
			where t.Stream == stream && t.Function == function
			select t)
			{
				if (message.IsHost && direction == "H->E")
				{
					SECSMessage result = MessageAbstractSECSLibrary.DeepCopy(message);
					return result;
				}
				if (!message.IsHost && direction == "E->H")
				{
					SECSMessage result = MessageAbstractSECSLibrary.DeepCopy(message);
					return result;
				}
			}
			return null;
		}

		internal override bool FindStream(int stream)
		{
			return this.list.Any((SECSMessage t) => t.Stream == stream);
		}

		public override SECSTransaction FindTransaction(string name)
		{
			string str = name.Substring(1, name.IndexOf("F") - 1);
			string str2 = name.Substring(name.IndexOf("F") + 1);
			int num = int.Parse(str);
			int num2 = int.Parse(str2);
			SECSTransaction transaction = new SECSTransaction();
			foreach (SECSMessage message in this.list)
			{
				if (message.Stream == num && message.Function == num2)
				{
					transaction.Primary = (message.Clone() as SECSMessage);
					break;
				}
			}
			foreach (SECSMessage message2 in this.list)
			{
				if (message2.Stream == num && message2.Function == num2 + 1)
				{
					transaction.Secondary = (message2.Clone() as SECSMessage);
					return transaction;
				}
			}
			return transaction;
		}

		public override bool Load(string filename)
		{
			this.xmlDoc = null;
			try
			{
				this.xmlDoc = XDocument.Load(filename);
				XElement element = this.xmlDoc.Element("SECSLibrary");
				if (element == null)
				{
					bool result = false;
					return result;
				}
				this.list.Clear();
				if (element.Attribute("Name") != null)
				{
					base.Name = element.Attribute("Name").Value.Trim();
				}
				if (element.Attribute("Description") != null)
				{
					base.Description = element.Attribute("Description").Value.Trim();
				}
				foreach (XElement element2 in element.Elements("SECSMessage"))
				{
					SECSMessage msg = this.XElementToSECSMessage(element2);
					this.AddMessage(msg);
				}
			}
			catch (Exception)
			{
				bool result = false;
				return result;
			}
			return true;
		}

		public override void RemoveTransaction(string name)
		{
			throw new NotSupportedException("This SECS Library's Format Not Support RemoveTransaction Function.");
		}

		public override void Save(string filename)
		{
			this.xmlDoc = null;
			this.xmlDoc = new XDocument();
			XElement element = new XElement("Library");
			element.SetAttributeValue("Format", "BySECSMessage");
			element.SetAttributeValue("Name", base.Name);
			element.SetAttributeValue("Description", base.Description);
			this.xmlDoc.AddFirst(element);
			foreach (SECSMessage message in this.list)
			{
				if (message != null)
				{
					element.Add(this.SECSMessageToXElement(message));
				}
			}
			this.xmlDoc.Save(filename);
		}

		public override XElement SECSItemToXElement(SECSItem root)
		{
			if (root == null)
			{
				return null;
			}
			if (root.Format == eSECS_FORMAT.LIST)
			{
				XElement element = new XElement("L");
				element.SetAttributeValue("Count", root.ItemCount.ToString(CultureInfo.InvariantCulture));
				element.SetAttributeValue("Fixed", root.Fixed ? "True" : "False");
				element.SetAttributeValue("ItemName", root.Name);
				for (int i = 1; i <= root.ItemCount; i++)
				{
					element.Add(this.SECSItemToXElement(root.Item(i)));
				}
				return element;
			}
			XElement element2 = new XElement(SECSFormat2.Format2Str((int)root.Format));
			element2.SetAttributeValue("Count", root.ItemCount.ToString(CultureInfo.InvariantCulture));
			element2.SetAttributeValue("Fixed", root.Fixed ? "True" : "False");
			element2.SetAttributeValue("ItemName", root.Name);
			if (root.IsEmpty)
			{
				element2.Value = "";
				return element2;
			}
			eSECS_FORMAT format = root.Format;
			if (format <= eSECS_FORMAT.I4)
			{
				switch (format)
				{
				case eSECS_FORMAT.BINARY:
					element2.Value = SecsItem2Str.GetBinaryStr(root.Value as byte[]);
					return element2;
				case eSECS_FORMAT.BOOLEAN:
					if (!root.IsArray)
					{
						bool[] data = new bool[]
						{
							(bool)root.Value
						};
						element2.Value = SecsItem2Str.GetBooleanStr(data);
						return element2;
					}
					element2.Value = SecsItem2Str.GetBooleanStr(root.Value as bool[]);
					return element2;
				case eSECS_FORMAT.ASCII:
					element2.Value = (string)root.Value;
					return element2;
				case eSECS_FORMAT.JIS8:
					element2.Value = (string)root.Value;
					return element2;
				case eSECS_FORMAT.CHAR2:
					element2.Value = (string)root.Value;
					return element2;
				case (eSECS_FORMAT)19:
				case (eSECS_FORMAT)20:
				case (eSECS_FORMAT)21:
				case (eSECS_FORMAT)22:
				case (eSECS_FORMAT)23:
				case (eSECS_FORMAT)27:
					return element2;
				case eSECS_FORMAT.I8:
					if (!root.IsArray)
					{
						long[] numArray9 = new long[]
						{
							(long)root.Value
						};
						element2.Value = SecsItem2Str.GetI8Str(numArray9);
						return element2;
					}
					element2.Value = SecsItem2Str.GetI8Str(root.Value as long[]);
					return element2;
				case eSECS_FORMAT.I1:
					if (!root.IsArray)
					{
						sbyte[] numArray10 = new sbyte[]
						{
							(sbyte)root.Value
						};
						element2.Value = SecsItem2Str.GetI1Str(numArray10);
						return element2;
					}
					element2.Value = SecsItem2Str.GetI1Str(root.Value as sbyte[]);
					return element2;
				case eSECS_FORMAT.I2:
					if (!root.IsArray)
					{
						short[] numArray11 = new short[]
						{
							(short)root.Value
						};
						element2.Value = SecsItem2Str.GetI2Str(numArray11);
						return element2;
					}
					element2.Value = SecsItem2Str.GetI2Str(root.Value as short[]);
					return element2;
				case eSECS_FORMAT.I4:
					if (!root.IsArray)
					{
						int[] numArray12 = new int[]
						{
							(int)root.Value
						};
						element2.Value = SecsItem2Str.GetI4Str(numArray12);
						return element2;
					}
					element2.Value = SecsItem2Str.GetI4Str(root.Value as int[]);
					return element2;
				}
				return element2;
			}
			switch (format)
			{
			case eSECS_FORMAT.F8:
			{
				if (root.IsArray)
				{
					element2.Value = SecsItem2Str.GetF8Str(root.Value as double[]);
					return element2;
				}
				double[] numArray13 = new double[]
				{
					(double)root.Value
				};
				element2.Value = SecsItem2Str.GetF8Str(numArray13);
				return element2;
			}
			case eSECS_FORMAT.F4:
				if (!root.IsArray)
				{
					float[] numArray14 = new float[]
					{
						(float)root.Value
					};
					element2.Value = SecsItem2Str.GetF4Str(numArray14);
					return element2;
				}
				element2.Value = SecsItem2Str.GetF4Str(root.Value as float[]);
				return element2;
			case (eSECS_FORMAT)37:
			case (eSECS_FORMAT)38:
			case (eSECS_FORMAT)39:
			case (eSECS_FORMAT)43:
				return element2;
			case eSECS_FORMAT.U8:
				if (!root.IsArray)
				{
					ulong[] numArray15 = new ulong[]
					{
						(ulong)root.Value
					};
					element2.Value = SecsItem2Str.GetU8Str(numArray15);
					return element2;
				}
				element2.Value = SecsItem2Str.GetU8Str(root.Value as ulong[]);
				return element2;
			case eSECS_FORMAT.U1:
				if (!root.IsArray)
				{
					byte[] buffer = new byte[]
					{
						(byte)root.Value
					};
					element2.Value = SecsItem2Str.GetU1Str(buffer);
					return element2;
				}
				element2.Value = SecsItem2Str.GetU1Str(root.Value as byte[]);
				return element2;
			case eSECS_FORMAT.U2:
				if (!root.IsArray)
				{
					ushort[] numArray16 = new ushort[]
					{
						(ushort)root.Value
					};
					element2.Value = SecsItem2Str.GetU2Str(numArray16);
					return element2;
				}
				element2.Value = SecsItem2Str.GetU2Str(root.Value as ushort[]);
				return element2;
			case eSECS_FORMAT.U4:
				if (!root.IsArray)
				{
					uint[] numArray17 = new uint[]
					{
						(uint)root.Value
					};
					element2.Value = SecsItem2Str.GetU4Str(numArray17);
					return element2;
				}
				element2.Value = SecsItem2Str.GetU4Str(root.Value as uint[]);
				return element2;
			}
			return element2;
		}

		internal XElement SECSMessageToXElement(SECSMessage msg)
		{
			XElement element = new XElement("SECSMessage");
			element.SetAttributeValue("Name", msg.Name);
			element.Add(new XElement("MessageName", msg.Name));
			element.Add(new XElement("Description", msg.Description));
			element.Add(new XElement("Stream", msg.Stream.ToString(CultureInfo.InvariantCulture)));
			element.Add(new XElement("Function", msg.Function.ToString(CultureInfo.InvariantCulture)));
			element.Add(new XElement("Direction", msg.IsHost ? "H->E" : "E->H"));
			element.Add(new XElement("Wait", msg.WBit ? "True" : "False"));
			element.Add(new XElement("DataItem", this.SECSItemToXElement(msg.Root)));
			return element;
		}

		public override XElement TransToXElement(SECSTransaction trans)
		{
			throw new NotSupportedException("This SECS Library's Format Not Support TransToXElement Function.");
		}

		public override string TransToXml(SECSTransaction trans)
		{
			throw new NotSupportedException("This SECS Library's Format Not Support TransToXml Function.");
		}

		public override SECSItem XElementToSECSItem(XElement element)
		{
			if (element == null)
			{
				return null;
			}
			string str = element.Name.ToString().Trim().ToUpper();
			SECSItem item = null;
			if (str == "L")
			{
				item = new SECSItem(eSECS_FORMAT.LIST);
				using (IEnumerator<XElement> enumerator = element.Elements().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						XElement element2 = enumerator.Current;
						SECSItem child = this.XElementToSECSItem(element2);
						if (child != null)
						{
							item.Add(child);
						}
					}
					goto IL_3F2;
				}
			}
			string data = element.Value.Trim();
			string key;
			switch (key = str)
			{
			case "A":
				item = new SECSItem(eSECS_FORMAT.ASCII)
				{
					Value = data
				};
				break;
			case "B":
				item = new SECSItem(eSECS_FORMAT.BINARY)
				{
					Value = Str2SecsItem.GetBinary(data)
				};
				break;
			case "BOOLEAN":
			{
				item = new SECSItem(eSECS_FORMAT.BOOLEAN);
				bool[] boolean = Str2SecsItem.GetBoolean(data);
				if (boolean.Length == 1)
				{
					item.Value = boolean[0];
				}
				else
				{
					item.Value = boolean;
				}
				break;
			}
			case "U1":
			{
				item = new SECSItem(eSECS_FORMAT.U1);
				byte[] buffer = Str2SecsItem.GetU1(data);
				if (buffer.Length == 1)
				{
					item.Value = buffer[0];
				}
				else
				{
					item.Value = buffer;
				}
				break;
			}
			case "U2":
			{
				item = new SECSItem(eSECS_FORMAT.U2);
				ushort[] numArray = Str2SecsItem.GetU2(data);
				if (numArray.Length == 1)
				{
					item.Value = numArray[0];
				}
				else
				{
					item.Value = numArray;
				}
				break;
			}
			case "U4":
			{
				item = new SECSItem(eSECS_FORMAT.U4);
				uint[] numArray2 = Str2SecsItem.GetU4(data);
				if (numArray2.Length == 1)
				{
					item.Value = numArray2[0];
				}
				else
				{
					item.Value = numArray2;
				}
				break;
			}
			case "U8":
			{
				item = new SECSItem(eSECS_FORMAT.U8);
				ulong[] numArray3 = Str2SecsItem.GetU8(data);
				if (numArray3.Length == 1)
				{
					item.Value = numArray3[0];
				}
				else
				{
					item.Value = numArray3;
				}
				break;
			}
			case "I1":
			{
				item = new SECSItem(eSECS_FORMAT.I1);
				sbyte[] numArray4 = Str2SecsItem.GetI1(data);
				if (numArray4.Length == 1)
				{
					item.Value = numArray4[0];
				}
				else
				{
					item.Value = numArray4;
				}
				break;
			}
			case "I2":
			{
				item = new SECSItem(eSECS_FORMAT.I2);
				short[] numArray5 = Str2SecsItem.GetI2(data);
				if (numArray5.Length == 1)
				{
					item.Value = numArray5[0];
				}
				else
				{
					item.Value = numArray5;
				}
				break;
			}
			case "I4":
			{
				item = new SECSItem(eSECS_FORMAT.I4);
				int[] numArray6 = Str2SecsItem.GetI4(data);
				if (numArray6.Length == 1)
				{
					item.Value = numArray6[0];
				}
				else
				{
					item.Value = numArray6;
				}
				break;
			}
			case "I8":
			{
				item = new SECSItem(eSECS_FORMAT.I8);
				long[] numArray7 = Str2SecsItem.GetI8(data);
				if (numArray7.Length == 1)
				{
					item.Value = numArray7[0];
				}
				else
				{
					item.Value = numArray7;
				}
				break;
			}
			case "CHAR2":
				item = new SECSItem(eSECS_FORMAT.CHAR2)
				{
					Value = data
				};
				break;
			case "JIS8":
				item = new SECSItem(eSECS_FORMAT.JIS8)
				{
					Value = data
				};
				break;
			}
			IL_3F2:
			if (item != null)
			{
				if (element.Attribute("ItemName") != null)
				{
					item.Name = element.Attribute("ItemName").Value;
				}
				if (element.Attribute("Count") != null)
				{
					item.Length = int.Parse(element.Attribute("Count").Value);
				}
				if (element.Attribute("Fixed") != null)
				{
					item.Fixed = (element.Attribute("Fixed").Value.Trim().ToUpper() == "TRUE");
				}
			}
			return item;
		}

		internal SECSMessage XElementToSECSMessage(XElement element)
		{
			SECSMessage message = new SECSMessage("", "");
			foreach (XElement element2 in element.Elements())
			{
				string str = element2.Name.ToString().Trim();
				if (str == "MessageName")
				{
					message.Name = element2.Value.Trim();
				}
				else if (str == "Description")
				{
					message.Description = element2.Value.Trim();
				}
				else if (str == "Stream")
				{
					message.Stream = int.Parse(element2.Value.Trim());
				}
				else if (str == "Function")
				{
					message.Function = int.Parse(element2.Value.Trim());
				}
				else if (str == "Wait")
				{
					message.WBit = (element2.Value.Trim().ToUpper() == "TRUE");
				}
				else if (str == "Direction")
				{
					message.IsHost = (element2.Value.Trim().ToUpper() == "H->E");
				}
				else if (str == "DataItem" && element2.HasElements)
				{
					SECSItem item = this.XElementToSECSItem(element2.Elements().First<XElement>());
					if (item != null)
					{
						message.Root = item;
					}
				}
			}
			return message;
		}

		public override SECSTransaction XElementToTrans(XElement element)
		{
			throw new NotSupportedException("This SECS Library's Format Not Support XElementToTrans Function.");
		}

		public override SECSTransaction XmlToTrans(string xmlnode)
		{
			throw new NotSupportedException("This SECS Library's Format Not Support XmlToTrans Function.");
		}
	}
}
