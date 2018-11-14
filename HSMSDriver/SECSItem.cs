using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace HSMSDriver
{
	[Serializable]
	public class SECSItem : ICloneable
	{
		private byte[] binaryValue;

		private string desc;

		private eSECS_FORMAT format;

		private bool isArray;

		private bool isFixed;

		private int itemLength;

		private object itemValue;

		private LCEncodingCode lcEncodingCode;

		private string name;

		private SECSItem parent;

		public readonly List<SECSItem> secslist;

		public string Description
		{
			get
			{
				return this.desc;
			}
			set
			{
				this.desc = value;
			}
		}

		public LCEncodingCode EncodingCode
		{
			get
			{
				return this.lcEncodingCode;
			}
			set
			{
				this.lcEncodingCode = value;
			}
		}

		internal bool Fixed
		{
			get
			{
				return this.isFixed;
			}
			set
			{
				this.isFixed = value;
			}
		}

		public eSECS_FORMAT Format
		{
			get
			{
				return this.format;
			}
		}

		internal bool IsArray
		{
			get
			{
				return this.isArray;
			}
		}

		internal bool IsEmpty
		{
			get
			{
				if (this.Format == eSECS_FORMAT.LIST)
				{
					return this.secslist.Count == 0;
				}
				return this.Value == null;
			}
		}

		public int ItemCount
		{
			get
			{
				if (this.Format == eSECS_FORMAT.LIST)
				{
					return this.secslist.Count;
				}
				if (this.Value == null)
				{
					return 0;
				}
				if (this.Format == eSECS_FORMAT.ASCII || this.Format == eSECS_FORMAT.JIS8 || this.Format == eSECS_FORMAT.CHAR2)
				{
					return ((string)this.Value).Length;
				}
				if (this.IsArray)
				{
					Array array = (Array)this.Value;
					return array.Length;
				}
				return 1;
			}
		}

		internal int Length
		{
			get
			{
				return this.itemLength;
			}
			set
			{
				this.itemLength = value;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		public SECSItem Parent
		{
			get
			{
				return this.parent;
			}
			set
			{
				if (value == null)
				{
					if (this.parent != null)
					{
						this.parent.secslist.Remove(this);
					}
					this.parent = null;
					return;
				}
				if (this.ContainsChild(value))
				{
					throw new Exception("Cannot set to Parent because it's child item");
				}
				if (this.parent != null)
				{
					this.parent.secslist.Remove(this);
					this.parent = null;
				}
				value.secslist.Add(this);
				this.parent = value;
			}
		}

		public object Value
		{
			get
			{
				return this.itemValue;
			}
			set
			{
				if (value == null)
				{
					this.itemValue = null;
					this.binaryValue = null;
					return;
				}
				bool isArray = this.isArray;
				object itemValue = this.itemValue;
				byte[] binaryValue = this.binaryValue;
				try
				{
					switch (this.Format)
					{
					case eSECS_FORMAT.LIST:
						this.SetToList(value);
						break;
					case eSECS_FORMAT.BINARY:
						this.SetToBinary(value);
						break;
					case eSECS_FORMAT.BOOLEAN:
						this.SetToBoolean(value);
						break;
					case eSECS_FORMAT.ASCII:
						this.SetToASCII(value);
						break;
					case eSECS_FORMAT.JIS8:
						this.SetToJIS8(value);
						break;
					case eSECS_FORMAT.CHAR2:
						this.SetToCHAR2(value);
						break;
					case eSECS_FORMAT.I8:
						this.SetToI8(value);
						break;
					case eSECS_FORMAT.I1:
						this.SetToI1(value);
						break;
					case eSECS_FORMAT.I2:
						this.SetToI2(value);
						break;
					case eSECS_FORMAT.I4:
						this.SetToI4(value);
						break;
					case eSECS_FORMAT.F8:
						this.SetToF8(value);
						break;
					case eSECS_FORMAT.F4:
						this.SetToF4(value);
						break;
					case eSECS_FORMAT.U8:
						this.SetToU8(value);
						break;
					case eSECS_FORMAT.U1:
						this.SetToU1(value);
						break;
					case eSECS_FORMAT.U2:
						this.SetToU2(value);
						break;
					case eSECS_FORMAT.U4:
						this.SetToU4(value);
						break;
					}
				}
				catch (Exception)
				{
					this.isArray = isArray;
					this.itemValue = itemValue;
					this.binaryValue = binaryValue;
					throw;
				}
			}
		}

		public SECSItem() : this(eSECS_FORMAT.LIST, "", "")
		{
		}

		public SECSItem(eSECS_FORMAT type) : this(type, "", "")
		{
		}

		public SECSItem(eSECS_FORMAT type, string name) : this(type, name, "")
		{
		}

		public SECSItem(eSECS_FORMAT type, string name, string desc)
		{
			this.name = "";
			this.desc = "";
			this.secslist = new List<SECSItem>();
			this.name = name;
			this.format = type;
			this.desc = desc;
			this.isFixed = false;
		}

		public void Add(SECSItem child)
		{
			if (child != null)
			{
				if (this.Format != eSECS_FORMAT.LIST)
				{
					throw new Exception("Can't add child to a non-list SECSItem");
				}
				if (!this.ContainsChild(child))
				{
					child.Parent = this;
				}
			}
		}

		public object Clone()
		{
			object obj2;
			using (MemoryStream stream = new MemoryStream())
			{
				BinaryFormatter formatter = new BinaryFormatter();
				formatter.Serialize(stream, this);
				stream.Seek(0L, SeekOrigin.Begin);
				obj2 = formatter.Deserialize(stream);
				stream.Close();
			}
			SECSItem item = (SECSItem)obj2;
			item.parent = null;
			return item;
		}

		public bool ContainsChild(SECSItem child)
		{
			if (this.Format == eSECS_FORMAT.LIST)
			{
				foreach (SECSItem item in this.secslist)
				{
					if (item == child)
					{
						bool result = true;
						return result;
					}
					if (item.ContainsChild(child))
					{
						bool result = true;
						return result;
					}
				}
				return false;
			}
			return false;
		}

		public void Delete()
		{
			if (this.Parent != null)
			{
				this.Parent.Remove(this);
			}
		}

		public SECSItem Duplicate()
		{
			if (this.parent == null || this.parent.format != eSECS_FORMAT.LIST)
			{
				throw new Exception("Can not duplicate. Parent is not a list");
			}
			SECSItem child = (SECSItem)this.Clone();
			this.parent.Add(child);
			return child;
		}

		public SECSItem Item(int nth)
		{
			if (this.Format != eSECS_FORMAT.LIST)
			{
				throw new Exception("Can't Get child from a non-list SECSItem");
			}
			if (nth < 1 || nth > this.secslist.Count)
			{
				throw new ArgumentOutOfRangeException("nth", "Index out of Range!");
			}
			return this.secslist[nth - 1];
		}

		public SECSItem Item(string itemName)
		{
			if (this.Format != eSECS_FORMAT.LIST)
			{
				throw new Exception("Can't Get child from a non-list SECSItem");
			}
			foreach (SECSItem item2 in this.secslist)
			{
				if (item2.Name == itemName)
				{
					return item2;
				}
			}
			return (from item in this.secslist
			where item.format == eSECS_FORMAT.LIST
			select item.Item(itemName)).FirstOrDefault<SECSItem>();
		}

		internal byte[] Raw()
		{
			Queue<byte> source = new Queue<byte>();
			if (this.Format == eSECS_FORMAT.LIST)
			{
				int length = this.secslist.Count;
				if (length < 256)
				{
					byte num = 1;
					source.Enqueue(num);
					byte num2 = (byte)(length & 255);
					source.Enqueue(num2);
				}
				else if (length < 65536)
				{
					byte num = 16;
					source.Enqueue(num);
					ushort num3 = (ushort)(length & 65535);
					source.Enqueue((byte)(num3 >> 8));
					source.Enqueue((byte)(num3 & 255));
				}
				else
				{
					byte num = 17;
					source.Enqueue(num);
					source.Enqueue((byte)(length >> 16));
					source.Enqueue((byte)((length & 65535) >> 8));
					source.Enqueue((byte)(length & 255));
				}
				for (int i = 0; i < length; i++)
				{
					byte[] buffer2 = this.secslist[i].Raw();
					for (int j = 0; j < buffer2.Length; j++)
					{
						byte num4 = buffer2[j];
						source.Enqueue(num4);
					}
				}
			}
			else if (this.Value != null)
			{
				int length = this.binaryValue.Length;
				if (length < 256)
				{
					byte num = (byte)((int)this.Format << 2 |1);
					source.Enqueue(num);
					byte num5 = (byte)(length & 255);
					source.Enqueue(num5);
				}
				else if (length < 65536)
				{
					byte num = (byte)((int)this.Format << 2 |2);
					source.Enqueue(num);
					ushort num6 = (ushort)(length & 65535);
					source.Enqueue((byte)(num6 >> 8));
					source.Enqueue((byte)(num6 & 255));
				}
				else
				{
					byte num = (byte)((int)this.Format << 2 |3);
					source.Enqueue(num);
					source.Enqueue((byte)(length >> 16));
					source.Enqueue((byte)((length & 65535) >> 8));
					source.Enqueue((byte)(length & 255));
				}
				byte[] binaryValue = this.binaryValue;
				for (int k = 0; k < binaryValue.Length; k++)
				{
					byte num7 = binaryValue[k];
					source.Enqueue(num7);
				}
			}
			else
			{
				byte num = (byte)((int)this.Format << 2 |1);
				source.Enqueue(num);
				source.Enqueue(0);
			}
			return source.ToArray<byte>();
		}

		public void Remove(SECSItem item)
		{
			if (this.Format != eSECS_FORMAT.LIST)
			{
				throw new Exception("Can't Remove from a non-list SecsItem");
			}
			if (!this.secslist.Any((SECSItem t) => t == item))
			{
				throw new Exception("SECSItem.Remove: Item not found");
			}
			item.Parent = null;
		}

		private void SetToASCII(object obj)
		{
			this.isArray = false;
			string str = Convert.ToString(obj);
			this.itemValue = str;
			this.binaryValue = new ASCIIEncoding().GetBytes(str);
		}

		private void SetToBinary(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				byte[] buffer = new byte[num];
				for (int i = 0; i < num; i++)
				{
					buffer[i] = Convert.ToByte(array.GetValue(i));
				}
				this.itemValue = buffer;
				this.binaryValue = buffer;
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToByte(obj);
			this.binaryValue = new byte[]
			{
				(byte)this.itemValue
			};
		}

		private void SetToBoolean(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				bool[] data = new bool[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToBoolean(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetBooleanBytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToBoolean(obj);
			bool[] flagArray2 = new bool[]
			{
				(bool)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetBooleanBytes(flagArray2);
		}

		private void SetToCHAR2(object obj)
		{
			if (!(obj is string))
			{
				throw new ArgumentException("Only support convert string to CHAR2");
			}
			this.isArray = false;
			this.itemValue = obj;
			List<byte> list = new List<byte>();
			byte[] bytes;
			switch (this.EncodingCode)
			{
			case LCEncodingCode.UTF8:
				list.AddRange(SecsValue2Byte.GetIntBytes(2, 2));
				bytes = Encoding.GetEncoding(65001).GetBytes((string)obj);
				break;
			case LCEncodingCode.ISO646:
				list.AddRange(SecsValue2Byte.GetIntBytes(3, 2));
				bytes = Encoding.GetEncoding(20127).GetBytes((string)obj);
				break;
			case LCEncodingCode.ISO88591:
				list.AddRange(SecsValue2Byte.GetIntBytes(4, 2));
				bytes = Encoding.GetEncoding(28591).GetBytes((string)obj);
				break;
			case LCEncodingCode.ISO885911:
				list.AddRange(SecsValue2Byte.GetIntBytes(5, 2));
				bytes = Encoding.GetEncoding(874).GetBytes((string)obj);
				break;
			case LCEncodingCode.TIS620:
				list.AddRange(SecsValue2Byte.GetIntBytes(6, 2));
				bytes = Encoding.GetEncoding(874).GetBytes((string)obj);
				break;
			case LCEncodingCode.IS13194:
				list.AddRange(SecsValue2Byte.GetIntBytes(7, 2));
				bytes = Encoding.GetEncoding(57002).GetBytes((string)obj);
				break;
			case LCEncodingCode.SHIFT_JIS:
				list.AddRange(SecsValue2Byte.GetIntBytes(8, 2));
				bytes = Encoding.GetEncoding(932).GetBytes((string)obj);
				break;
			case LCEncodingCode.JAPANESE:
				list.AddRange(SecsValue2Byte.GetIntBytes(9, 2));
				bytes = Encoding.GetEncoding(20932).GetBytes((string)obj);
				break;
			case LCEncodingCode.KOREAN:
				list.AddRange(SecsValue2Byte.GetIntBytes(10, 2));
				bytes = Encoding.GetEncoding(51949).GetBytes((string)obj);
				break;
			case LCEncodingCode.CHINESE_GB2312:
				list.AddRange(SecsValue2Byte.GetIntBytes(11, 2));
				bytes = Encoding.GetEncoding(936).GetBytes((string)obj);
				break;
			case LCEncodingCode.CHINESE_EUC_CN:
				list.AddRange(SecsValue2Byte.GetIntBytes(12, 2));
				bytes = Encoding.GetEncoding(51936).GetBytes((string)obj);
				break;
			case LCEncodingCode.CHINESE_BIG5:
				list.AddRange(SecsValue2Byte.GetIntBytes(13, 2));
				bytes = Encoding.GetEncoding(950).GetBytes((string)obj);
				break;
			default:
				list.AddRange(SecsValue2Byte.GetIntBytes(1, 2));
				bytes = Encoding.GetEncoding(1200).GetBytes((string)obj);
				break;
			}
			list.AddRange(bytes);
			this.binaryValue = list.ToArray();
		}

		private void SetToF4(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				float[] data = new float[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToSingle(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetF4Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToSingle(obj);
			float[] numArray2 = new float[]
			{
				(float)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetF4Bytes(numArray2);
		}

		private void SetToF8(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				double[] data = new double[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToDouble(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetF8Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToDouble(obj);
			double[] numArray2 = new double[]
			{
				(double)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetF8Bytes(numArray2);
		}

		private void SetToI1(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				sbyte[] data = new sbyte[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToSByte(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetI1Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToSByte(obj);
			sbyte[] numArray2 = new sbyte[]
			{
				(sbyte)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetI1Bytes(numArray2);
		}

		private void SetToI2(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				short[] data = new short[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToInt16(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetI2Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToInt16(obj);
			short[] numArray2 = new short[]
			{
				(short)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetI2Bytes(numArray2);
		}

		private void SetToI4(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				int[] data = new int[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToInt32(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetI4Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToInt32(obj);
			int[] numArray2 = new int[]
			{
				(int)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetI4Bytes(numArray2);
		}

		private void SetToI8(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				long[] data = new long[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToInt64(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetI8Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToInt64(obj);
			long[] numArray2 = new long[]
			{
				(long)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetI8Bytes(numArray2);
		}

		private void SetToJIS8(object obj)
		{
			if (!(obj is string))
			{
				throw new InvalidOperationException("Can Only Support Convert string to JIS8");
			}
			this.isArray = false;
			string str = (string)obj;
			this.itemValue = str;
			this.binaryValue = Encoding.Unicode.GetBytes(str);
		}

		private void SetToList(object obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException("obj");
			}
			throw new InvalidOperationException("Cannot Set Value to List");
		}

		private void SetToU1(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				byte[] buffer = new byte[num];
				for (int i = 0; i < num; i++)
				{
					buffer[i] = Convert.ToByte(array.GetValue(i));
				}
				this.itemValue = buffer;
				this.binaryValue = buffer;
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToByte(obj);
			this.binaryValue = new byte[]
			{
				(byte)this.itemValue
			};
		}

		private void SetToU2(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				ushort[] data = new ushort[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToUInt16(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetU2Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToUInt16(obj);
			ushort[] numArray2 = new ushort[]
			{
				(ushort)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetU2Bytes(numArray2);
		}

		private void SetToU4(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				uint[] data = new uint[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToUInt32(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetU4Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToUInt32(obj);
			uint[] numArray2 = new uint[]
			{
				(uint)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetU4Bytes(numArray2);
		}

		private void SetToU8(object obj)
		{
			if (obj.GetType().IsArray)
			{
				this.isArray = true;
				Array array = (Array)obj;
				int num = array.Length;
				ulong[] data = new ulong[num];
				for (int i = 0; i < num; i++)
				{
					data[i] = Convert.ToUInt64(array.GetValue(i));
				}
				this.itemValue = data;
				this.binaryValue = SecsValue2Byte.GetU8Bytes(data);
				return;
			}
			this.isArray = false;
			this.itemValue = Convert.ToUInt64(obj);
			ulong[] numArray2 = new ulong[]
			{
				(ulong)this.itemValue
			};
			this.binaryValue = SecsValue2Byte.GetU8Bytes(numArray2);
		}

		public void SetValue(eSECS_FORMAT fmt, object obj)
		{
			eSECS_FORMAT format = this.format;
			try
			{
				this.format = fmt;
				this.Value = obj;
			}
			catch (Exception)
			{
				this.format = format;
				throw;
			}
		}
	}
}
