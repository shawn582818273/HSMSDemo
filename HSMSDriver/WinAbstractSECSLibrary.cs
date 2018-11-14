using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Linq;

namespace HSMSDriver
{
	internal class WinAbstractSECSLibrary : AbstractSECSLibrary
	{
		private readonly Dictionary<string, SECSTransaction> transdic;

		private XDocument xmlDoc;

		public IDictionary<string, SECSTransaction> TransList
		{
			get
			{
				return this.transdic;
			}
		}

		public WinAbstractSECSLibrary() : this("TTSKY AbstractSECSLibrary", "TTSKY SECS Default Libray")
		{
		}

		public WinAbstractSECSLibrary(string name, string desc) : base(name, desc)
		{
			this.transdic = new Dictionary<string, SECSTransaction>();
			this.xmlDoc = new XDocument();
		}

		public override void AddTransaction(SECSTransaction trans)
		{
			if (trans != null)
			{
				if (this.transdic.ContainsKey(trans.Name))
				{
					trans.Name += "New";
					if (this.transdic.ContainsKey(trans.Name))
					{
						int num = 1;
						while (this.transdic.ContainsKey(trans.Name + num))
						{
							num++;
						}
						trans.Name += num;
						this.transdic.Add(trans.Name, trans);
						return;
					}
					this.transdic.Add(trans.Name, trans);
					return;
				}
				else
				{
					this.transdic.Add(trans.Name, trans);
				}
			}
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
					if (rcvd.ItemCount < format.ItemCount)
					{
						for (int i = 0; i < rcvd.ItemCount; i++)
						{
							SECSItem item = format.Item(i + 1);
							SECSItem item2 = rcvd.Item(i + 1);
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
					else
					{
						for (int j = 0; j < format.ItemCount; j++)
						{
							SECSItem item3 = format.Item(j + 1);
							SECSItem item4 = rcvd.Item(j + 1);
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
				else
				{
					rcvd.Name = format.Name;
					rcvd.Description = format.Description;
					result = true;
				}
			}
			catch (Exception)
			{
				result = false;
			}
			return result;
		}

		private static SECSTransaction DeepCopy(SECSTransaction trans)
		{
			object obj2;
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, trans);
				stream.Seek(0L, SeekOrigin.Begin);
				obj2 = formatter.Deserialize(stream);
			}
			return (SECSTransaction)obj2;
		}

		internal override bool FindFunction(int stream, int function)
		{
			foreach (KeyValuePair<string, SECSTransaction> pair in this.transdic)
			{
				if (pair.Value.Primary != null && pair.Value.Primary.Stream == stream && pair.Value.Primary.Function == function)
				{
					bool result = true;
					return result;
				}
				if (pair.Value.Secondary != null && pair.Value.Secondary.Stream == stream && pair.Value.Secondary.Function == function)
				{
					bool result = true;
					return result;
				}
			}
			return false;
		}

		public override SECSMessage FindMessage(string name)
		{
			throw new NotSupportedException("This Format SECS Library Not Support FindMessage Method.");
		}

		internal override List<SECSMessage> FindMessage(int stream, int function)
		{
			List<SECSMessage> list = new List<SECSMessage>();
			foreach (KeyValuePair<string, SECSTransaction> pair in this.transdic)
			{
				if (pair.Value.Primary != null && pair.Value.Primary.Stream == stream && pair.Value.Primary.Function == function)
				{
					SECSMessage message = pair.Value.Primary.Clone() as SECSMessage;
					list.Add(message);
				}
				else if (pair.Value.Secondary != null && pair.Value.Secondary.Stream == stream && pair.Value.Secondary.Function == function)
				{
					SECSMessage message2 = pair.Value.Secondary.Clone() as SECSMessage;
					list.Add(message2);
				}
			}
			return list;
		}

		internal override bool FindStream(int stream)
		{
			return this.transdic.Any((KeyValuePair<string, SECSTransaction> item) => item.Value.Primary != null && item.Value.Primary.Stream == stream);
		}

		public override SECSTransaction FindTransaction(string name)
		{
			if (this.transdic.ContainsKey(name))
			{
				return WinAbstractSECSLibrary.DeepCopy(this.transdic[name]);
			}
			return null;
		}

		public override bool Load(string filename)
		{
			this.xmlDoc = null;
			try
			{
				this.xmlDoc = XDocument.Load(filename);
				XElement element = this.xmlDoc.Element("Library");
				if (element == null)
				{
					bool result = false;
					return result;
				}
				this.transdic.Clear();
				base.Name = (string)element.Element("Name");
				base.Description = (string)element.Element("Description");
				foreach (XElement element2 in element.Elements("Transaction"))
				{
					SECSTransaction trans = this.XElementToTrans(element2);
					this.AddTransaction(trans);
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
			if (this.transdic.ContainsKey(name))
			{
				this.transdic.Remove(name);
			}
		}

		public override void Save(string filename)
		{
			this.xmlDoc = null;
			this.xmlDoc = new XDocument();
			XElement element = new XElement("Library");
			this.xmlDoc.AddFirst(element);
			element.Add(new XElement("Name", new XText(base.Name)));
			element.Add(new XElement("Description", new XText(base.Description)));
			foreach (KeyValuePair<string, SECSTransaction> pair in this.transdic)
			{
				SECSTransaction trans = pair.Value;
				XElement element2 = this.TransToXElement(trans);
				if (element2 != null)
				{
					element.Add(element2);
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
			XElement element = new XElement("Item");
			element.Add(new XElement("Name", new XText(root.Name)));
			element.Add(new XElement("Description", new XText(root.Description)));
			if (root.Format == eSECS_FORMAT.LIST)
			{
				XElement element2 = new XElement("Format", new XText("List"));
				element.Add(element2);
				for (int i = 0; i < root.ItemCount; i++)
				{
					XElement element3 = this.SECSItemToXElement(root.Item(i + 1));
					element.Add(element3);
				}
				return element;
			}
			XElement element4 = new XElement("Format");
			XText text = null;
			eSECS_FORMAT format = root.Format;
			if (format <= eSECS_FORMAT.I4)
			{
				switch (format)
				{
				case eSECS_FORMAT.BINARY:
					text = new XText("Binary");
					break;
				case eSECS_FORMAT.BOOLEAN:
					text = new XText("Boolean");
					break;
				default:
					switch (format)
					{
					case eSECS_FORMAT.ASCII:
						text = new XText("ASCII");
						break;
					case eSECS_FORMAT.JIS8:
						text = new XText("JIS8");
						break;
					case eSECS_FORMAT.CHAR2:
						text = new XText("CHAR2");
						break;
					case eSECS_FORMAT.I8:
						text = new XText("I8");
						break;
					case eSECS_FORMAT.I1:
						text = new XText("I1");
						break;
					case eSECS_FORMAT.I2:
						text = new XText("I2");
						break;
					case eSECS_FORMAT.I4:
						text = new XText("I4");
						break;
					}
					break;
				}
			}
			else if (format != eSECS_FORMAT.F8)
			{
				switch (format)
				{
				case eSECS_FORMAT.F4:
					text = new XText("F4");
					break;
				case eSECS_FORMAT.U8:
					text = new XText("U8");
					break;
				case eSECS_FORMAT.U1:
					text = new XText("U1");
					break;
				case eSECS_FORMAT.U2:
					text = new XText("U2");
					break;
				case eSECS_FORMAT.U4:
					text = new XText("U4");
					break;
				}
			}
			else
			{
				text = new XText("F8");
			}
			element4.Add(text);
			element.Add(element4);
			XElement element5 = new XElement("Value");
			XText text2 = null;
			if (root.IsEmpty)
			{
				text2 = new XText("");
			}
			else
			{
				eSECS_FORMAT format2 = root.Format;
				if (format2 <= eSECS_FORMAT.I4)
				{
					switch (format2)
					{
					case eSECS_FORMAT.BINARY:
						if (!root.IsArray)
						{
							text2 = new XText(root.Value.ToString().Trim());
						}
						else
						{
							text2 = new XText(SecsItem2Str.GetBinaryStr((byte[])root.Value).Trim());
						}
						break;
					case eSECS_FORMAT.BOOLEAN:
						if (!root.IsArray)
						{
							text2 = new XText(SecsItem2Str.GetBooleanStr(new bool[]
							{
								(bool)root.Value
							}));
						}
						else
						{
							text2 = new XText(SecsItem2Str.GetBooleanStr((bool[])root.Value));
						}
						break;
					default:
						switch (format2)
						{
						case eSECS_FORMAT.ASCII:
							text2 = new XText((string)root.Value);
							break;
						case eSECS_FORMAT.JIS8:
							text2 = new XText((string)root.Value);
							break;
						case eSECS_FORMAT.CHAR2:
							text2 = new XText((string)root.Value);
							break;
						case eSECS_FORMAT.I8:
							if (!root.IsArray)
							{
								text2 = new XText(SecsItem2Str.GetI8Str(new long[]
								{
									(long)root.Value
								}));
							}
							else
							{
								text2 = new XText(SecsItem2Str.GetI8Str((long[])root.Value));
							}
							break;
						case eSECS_FORMAT.I1:
							if (!root.IsArray)
							{
								text2 = new XText(SecsItem2Str.GetI1Str(new sbyte[]
								{
									(sbyte)root.Value
								}));
							}
							else
							{
								text2 = new XText(SecsItem2Str.GetI1Str((sbyte[])root.Value));
							}
							break;
						case eSECS_FORMAT.I2:
							if (!root.IsArray)
							{
								text2 = new XText(SecsItem2Str.GetI2Str(new short[]
								{
									(short)root.Value
								}));
							}
							else
							{
								text2 = new XText(SecsItem2Str.GetI2Str((short[])root.Value));
							}
							break;
						case eSECS_FORMAT.I4:
							if (!root.IsArray)
							{
								text2 = new XText(SecsItem2Str.GetI4Str(new int[]
								{
									(int)root.Value
								}));
							}
							else
							{
								text2 = new XText(SecsItem2Str.GetI4Str((int[])root.Value));
							}
							break;
						}
						break;
					}
				}
				else if (format2 != eSECS_FORMAT.F8)
				{
					switch (format2)
					{
					case eSECS_FORMAT.F4:
						if (!root.IsArray)
						{
							text2 = new XText(SecsItem2Str.GetF4Str(new float[]
							{
								(float)root.Value
							}));
						}
						else
						{
							text2 = new XText(SecsItem2Str.GetF4Str((float[])root.Value));
						}
						break;
					case eSECS_FORMAT.U8:
						if (!root.IsArray)
						{
							text2 = new XText(SecsItem2Str.GetU8Str(new ulong[]
							{
								(ulong)root.Value
							}));
						}
						else
						{
							text2 = new XText(SecsItem2Str.GetU8Str((ulong[])root.Value));
						}
						break;
					case eSECS_FORMAT.U1:
						if (!root.IsArray)
						{
							text2 = new XText(SecsItem2Str.GetU1Str(new byte[]
							{
								(byte)root.Value
							}));
						}
						else
						{
							text2 = new XText(SecsItem2Str.GetU1Str((byte[])root.Value));
						}
						break;
					case eSECS_FORMAT.U2:
						if (!root.IsArray)
						{
							text2 = new XText(SecsItem2Str.GetU2Str(new ushort[]
							{
								(ushort)root.Value
							}));
						}
						else
						{
							text2 = new XText(SecsItem2Str.GetU2Str((ushort[])root.Value));
						}
						break;
					case eSECS_FORMAT.U4:
						if (!root.IsArray)
						{
							text2 = new XText(SecsItem2Str.GetU4Str(new uint[]
							{
								(uint)root.Value
							}));
						}
						else
						{
							text2 = new XText(SecsItem2Str.GetU4Str((uint[])root.Value));
						}
						break;
					}
				}
				else if (root.IsArray)
				{
					text2 = new XText(SecsItem2Str.GetF8Str((double[])root.Value));
				}
				else
				{
					text2 = new XText(SecsItem2Str.GetF8Str(new double[]
					{
						(double)root.Value
					}));
				}
			}
			element5.Add(text2);
			element.Add(element5);
			return element;
		}

		public override XElement TransToXElement(SECSTransaction trans)
		{
			if (trans == null)
			{
				return null;
			}
			XElement element = new XElement("Transaction");
			element.Add(new XElement("Name", new XText(trans.Name)));
			element.Add(new XElement("Description", new XText(trans.Description)));
			element.Add(new XElement("ReplyExpected", new XText(trans.ExpectReply.ToString())));
			if (trans.Primary != null)
			{
				element.Add(new XElement("Stream", new XText(trans.Primary.Stream.ToString(CultureInfo.InvariantCulture))));
				element.Add(new XElement("Function", new XText(trans.Primary.Function.ToString(CultureInfo.InvariantCulture))));
			}
			else if (trans.Secondary != null)
			{
				element.Add(new XElement("Stream", new XText(trans.Secondary.Stream.ToString(CultureInfo.InvariantCulture))));
				element.Add(new XElement("Function", new XText(trans.Secondary.Function.ToString(CultureInfo.InvariantCulture))));
			}
			if (trans.Primary != null)
			{
				XElement element2 = new XElement("Primary");
				element.Add(element2);
				if (trans.Primary.Root != null)
				{
					XElement element3 = this.SECSItemToXElement(trans.Primary.Root);
					element2.Add(element3);
				}
			}
			if (trans.Secondary != null)
			{
				XElement element4 = new XElement("Secondary");
				element.Add(element4);
				if (trans.Primary != null && (trans.Primary == null || trans.Primary.Function == 0))
				{
					return element;
				}
				if (trans.Secondary.Root != null)
				{
					XElement element5 = this.SECSItemToXElement(trans.Secondary.Root);
					element4.Add(element5);
				}
			}
			return element;
		}

		public override string TransToXml(SECSTransaction trans)
		{
			return this.TransToXElement(trans).ToString();
		}

		public override SECSItem XElementToSECSItem(XElement element)
		{
			if (element == null)
			{
				return null;
			}
			string name = ((string)element.Element("Name")).Trim();
			string desc = ((string)element.Element("Description")).Trim();
			string str3 = ((string)element.Element("Format")).Trim().ToUpper(CultureInfo.InvariantCulture);
			if (str3 == "LIST")
			{
				SECSItem item = new SECSItem(eSECS_FORMAT.LIST, name, desc);
				foreach (XElement element2 in element.Elements("Item"))
				{
					SECSItem child = this.XElementToSECSItem(element2);
					item.Add(child);
				}
				return item;
			}
			SECSItem item2 = null;
			string data = ((string)element.Element("Value")).Trim();
			string key;
			switch (key = str3)
			{
			case "ASCII":
				item2 = new SECSItem(eSECS_FORMAT.ASCII, name, desc);
				break;
			case "CHAR2":
				item2 = new SECSItem(eSECS_FORMAT.CHAR2, name, desc);
				break;
			case "BINARY":
				item2 = new SECSItem(eSECS_FORMAT.BINARY, name, desc);
				break;
			case "BOOLEAN":
				item2 = new SECSItem(eSECS_FORMAT.BOOLEAN, name, desc);
				break;
			case "I1":
				item2 = new SECSItem(eSECS_FORMAT.I1, name, desc);
				break;
			case "I2":
				item2 = new SECSItem(eSECS_FORMAT.I2, name, desc);
				break;
			case "I4":
				item2 = new SECSItem(eSECS_FORMAT.I4, name, desc);
				break;
			case "I8":
				item2 = new SECSItem(eSECS_FORMAT.I8, name, desc);
				break;
			case "U1":
				item2 = new SECSItem(eSECS_FORMAT.U1, name, desc);
				break;
			case "U2":
				item2 = new SECSItem(eSECS_FORMAT.U2, name, desc);
				break;
			case "U4":
				item2 = new SECSItem(eSECS_FORMAT.U4, name, desc);
				break;
			case "U8":
				item2 = new SECSItem(eSECS_FORMAT.U8, name, desc);
				break;
			case "F4":
				item2 = new SECSItem(eSECS_FORMAT.F4, name, desc);
				break;
			case "F8":
				item2 = new SECSItem(eSECS_FORMAT.F8, name, desc);
				break;
			case "JIS-8":
			case "JIS8":
				item2 = new SECSItem(eSECS_FORMAT.JIS8, name, desc);
				break;
			}
			if (string.IsNullOrEmpty(data))
			{
				if (item2 != null)
				{
					item2.Value = null;
				}
				return item2;
			}
			if (item2 != null && (item2.Format == eSECS_FORMAT.ASCII || item2.Format == eSECS_FORMAT.JIS8 || item2.Format == eSECS_FORMAT.CHAR2))
			{
				item2.Value = data;
				return item2;
			}
			if (item2 != null)
			{
				switch (item2.Format)
				{
				case eSECS_FORMAT.BINARY:
				{
					byte[] binary = Str2SecsItem.GetBinary(data);
					if (binary.Length <= 1)
					{
						item2.Value = binary[0];
						return item2;
					}
					item2.Value = binary;
					return item2;
				}
				case eSECS_FORMAT.BOOLEAN:
				{
					bool[] boolean = Str2SecsItem.GetBoolean(data);
					if (boolean.Length != 1)
					{
						item2.Value = boolean;
						return item2;
					}
					item2.Value = boolean[0];
					return item2;
				}
				case eSECS_FORMAT.I8:
				{
					long[] numArray7 = Str2SecsItem.GetI8(data);
					if (numArray7.Length != 1)
					{
						item2.Value = numArray7;
						return item2;
					}
					item2.Value = numArray7[0];
					return item2;
				}
				case eSECS_FORMAT.I1:
				{
					sbyte[] numArray8 = Str2SecsItem.GetI1(data);
					if (numArray8.Length != 1)
					{
						item2.Value = numArray8;
						return item2;
					}
					item2.Value = numArray8[0];
					return item2;
				}
				case eSECS_FORMAT.I2:
				{
					short[] numArray9 = Str2SecsItem.GetI2(data);
					if (numArray9.Length != 1)
					{
						item2.Value = numArray9;
						return item2;
					}
					item2.Value = numArray9[0];
					return item2;
				}
				case (eSECS_FORMAT)27:
				case (eSECS_FORMAT)29:
				case (eSECS_FORMAT)30:
				case (eSECS_FORMAT)31:
					return item2;
				case eSECS_FORMAT.I4:
				{
					int[] numArray10 = Str2SecsItem.GetI4(data);
					if (numArray10.Length != 1)
					{
						item2.Value = numArray10;
						return item2;
					}
					item2.Value = numArray10[0];
					return item2;
				}
				case eSECS_FORMAT.F8:
				{
					double[] numArray11 = Str2SecsItem.GetF8(data);
					if (numArray11.Length != 1)
					{
						item2.Value = numArray11;
						return item2;
					}
					item2.Value = numArray11[0];
					return item2;
				}
				case eSECS_FORMAT.F4:
				{
					float[] numArray12 = Str2SecsItem.GetF4(data);
					if (numArray12.Length != 1)
					{
						item2.Value = numArray12;
						return item2;
					}
					item2.Value = numArray12[0];
					return item2;
				}
				case (eSECS_FORMAT)37:
				case (eSECS_FORMAT)38:
				case (eSECS_FORMAT)39:
				case (eSECS_FORMAT)43:
					return item2;
				case eSECS_FORMAT.U8:
				{
					ulong[] numArray13 = Str2SecsItem.GetU8(data);
					if (numArray13.Length != 1)
					{
						item2.Value = numArray13;
						return item2;
					}
					item2.Value = numArray13[0];
					return item2;
				}
				case eSECS_FORMAT.U1:
				{
					byte[] buffer2 = Str2SecsItem.GetU1(data);
					if (buffer2.Length != 1)
					{
						item2.Value = buffer2;
						return item2;
					}
					item2.Value = buffer2[0];
					return item2;
				}
				case eSECS_FORMAT.U2:
				{
					ushort[] numArray14 = Str2SecsItem.GetU2(data);
					if (numArray14.Length != 1)
					{
						item2.Value = numArray14;
						return item2;
					}
					item2.Value = numArray14[0];
					return item2;
				}
				case eSECS_FORMAT.U4:
				{
					uint[] numArray15 = Str2SecsItem.GetU4(data);
					if (numArray15.Length != 1)
					{
						item2.Value = numArray15;
						return item2;
					}
					item2.Value = numArray15[0];
					return item2;
				}
				}
			}
			return item2;
		}

		public override SECSTransaction XElementToTrans(XElement element)
		{
			SECSTransaction result;
			try
			{
				if (element == null)
				{
					result = null;
				}
				else
				{
					SECSTransaction transaction = new SECSTransaction
					{
						Name = (string)element.Element("Name"),
						Description = (string)element.Element("Description")
					};
					string str = (string)element.Element("ReplyExpected");
					if (str != null && str.Trim().ToUpper(CultureInfo.InvariantCulture) == "TRUE")
					{
						transaction.ExpectReply = true;
					}
					else
					{
						transaction.ExpectReply = false;
					}
					XElement element2 = element.Element("Primary");
					if (element2 != null)
					{
						transaction.Primary = new SECSMessage();
						transaction.Primary.Name = transaction.Name;
						transaction.Primary.Description = transaction.Description;
						transaction.Primary.WBit = transaction.ExpectReply;
						transaction.Primary.Stream = (int)element.Element("Stream");
						transaction.Primary.Function = (int)element.Element("Function");
						XElement element3 = element2.Element("Item");
						transaction.Primary.Root = this.XElementToSECSItem(element3);
					}
					XElement element4 = element.Element("Secondary");
					if (element4 != null)
					{
						if (transaction.Primary != null)
						{
							if (transaction.Primary.Function != 0)
							{
								SECSMessage message = new SECSMessage
								{
									Stream = transaction.Primary.Stream,
									Function = transaction.Primary.Function + 1
								};
								transaction.Secondary = message;
								XElement element5 = element4.Element("Item");
								transaction.Secondary.Root = this.XElementToSECSItem(element5);
							}
						}
						else
						{
							SECSMessage message2 = new SECSMessage
							{
								Stream = (int)element.Element("Stream"),
								Function = (int)element.Element("Function")
							};
							transaction.Secondary = message2;
							XElement element6 = element4.Element("Item");
							transaction.Secondary.Root = this.XElementToSECSItem(element6);
						}
					}
					result = transaction;
				}
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		public override SECSTransaction XmlToTrans(string xmlnode)
		{
			SECSTransaction result;
			try
			{
				this.xmlDoc = XDocument.Parse(xmlnode);
				result = this.XElementToTrans(this.xmlDoc.Element("Transaction"));
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}
	}
}
