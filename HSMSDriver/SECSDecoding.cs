using System;
using System.Text;

namespace HSMSDriver
{
	internal class SECSDecoding
	{
		private byte[] mDataItem;

		private int mReadIndex;

		public SECSItem Byte_TO_SecsItem(byte[] aDataByte)
		{
			this.mReadIndex = 0;
			this.mDataItem = aDataByte;
			if (this.mDataItem.Length > 0)
			{
				byte num = this.mDataItem[this.mReadIndex];
				this.mReadIndex++;
				int format = num >> 2;
				int lengthBytes = (int)(num & 3);
				return this.DecodeDataItem((eSECS_FORMAT)format, lengthBytes);
			}
			return null;
		}

		public SECSMessage Byte_TO_SecsMessage(byte[] aHeader)
		{
			SECSMessage message = new SECSMessage("", "");
			byte[] aBytes = new byte[]
			{
				aHeader[0],
				aHeader[1]
			};
			byte[] buffer2 = new byte[]
			{
				aHeader[6],
				aHeader[7],
				aHeader[8],
				aHeader[9]
			};
			message.SystemBytes = Byte2SecsValue.GetLong(buffer2);
			message.DeviceIdID = Byte2SecsValue.AdjustDeviceID((short)Byte2SecsValue.GetInt(aBytes));
			message.Stream = (int)(aHeader[2] & 255);
			int stream = message.Stream;
			if (stream > 128)
			{
				message.Stream = stream - 128;
				message.WBit = true;
			}
			message.Function = (int)(aHeader[3] & 255);
			return message;
		}

		private SECSItem DecodeDataItem(eSECS_FORMAT format, int lengthBytes)
		{
			int aLength = this.getBinaryLength(lengthBytes);
			SECSItem item = new SECSItem(format);
			if (format <= eSECS_FORMAT.BOOLEAN)
			{
				if (format == eSECS_FORMAT.LIST)
				{
					item.Name = "L";
					for (int i = 0; i < aLength; i++)
					{
						byte num3 = this.mDataItem[this.mReadIndex];
						this.mReadIndex++;
						int num4 = num3 >> 2;
						int num5 = (int)(num3 & 3);
						item.Add(this.DecodeDataItem((eSECS_FORMAT)num4, num5));
					}
					return item;
				}
				switch (format)
				{
				case eSECS_FORMAT.BINARY:
				{
					byte[] buffer2 = this.ParseBinary(aLength);
					item.Value = buffer2;
					return item;
				}
				case eSECS_FORMAT.BOOLEAN:
				{
					bool[] flagArray = this.ParseBoolean(aLength);
					if (flagArray.Length <= 1)
					{
						if (flagArray.Length == 1)
						{
							item.Value = flagArray[0];
						}
						return item;
					}
					item.Value = flagArray;
					return item;
				}
				}
			}
			else
			{
				switch (format)
				{
				case eSECS_FORMAT.ASCII:
				{
					string str = this.ParseAscii(aLength);
					item.Length = aLength;
					item.Value = str;
					return item;
				}
				case eSECS_FORMAT.JIS8:
					item.Value = this.ParseJIS8(aLength);
					return item;
				case eSECS_FORMAT.CHAR2:
					item.Value = this.ParseChar2(aLength);
					return item;
				case (eSECS_FORMAT)19:
				case (eSECS_FORMAT)20:
				case (eSECS_FORMAT)21:
				case (eSECS_FORMAT)22:
				case (eSECS_FORMAT)23:
				case (eSECS_FORMAT)27:
					break;
				case eSECS_FORMAT.I8:
				{
					long[] numArray4 = this.ParseInt8(aLength);
					if (numArray4.Length <= 1)
					{
						if (numArray4.Length == 1)
						{
							item.Value = numArray4[0];
						}
						return item;
					}
					item.Value = numArray4;
					return item;
				}
				case eSECS_FORMAT.I1:
				{
					sbyte[] numArray5 = this.ParseInt1(aLength);
					if (numArray5.Length <= 1)
					{
						if (numArray5.Length == 1)
						{
							item.Value = numArray5[0];
						}
						return item;
					}
					item.Value = numArray5;
					return item;
				}
				case eSECS_FORMAT.I2:
				{
					short[] numArray6 = this.ParseInt2(aLength);
					if (numArray6.Length <= 1)
					{
						if (numArray6.Length == 1)
						{
							item.Value = numArray6[0];
						}
						return item;
					}
					item.Value = numArray6;
					return item;
				}
				case eSECS_FORMAT.I4:
				{
					int[] numArray7 = this.ParseInt4(aLength);
					if (numArray7.Length <= 1)
					{
						if (numArray7.Length == 1)
						{
							item.Value = numArray7[0];
						}
						return item;
					}
					item.Value = numArray7;
					return item;
				}
				default:
					if (format != eSECS_FORMAT.F8)
					{
						switch (format)
						{
						case eSECS_FORMAT.F4:
						{
							float[] numArray8 = this.ParseFloat(aLength);
							if (numArray8.Length <= 1)
							{
								if (numArray8.Length == 1)
								{
									item.Value = numArray8[0];
								}
								return item;
							}
							item.Value = numArray8;
							return item;
						}
						case eSECS_FORMAT.U8:
						{
							ulong[] numArray9 = this.ParseUInt8(aLength);
							if (numArray9.Length <= 1)
							{
								if (numArray9.Length == 1)
								{
									item.Value = numArray9[0];
								}
								return item;
							}
							item.Value = numArray9;
							return item;
						}
						case eSECS_FORMAT.U1:
						{
							byte[] buffer3 = this.ParseUInt1(aLength);
							if (buffer3.Length <= 1)
							{
								if (buffer3.Length == 1)
								{
									item.Value = buffer3[0];
								}
								return item;
							}
							item.Value = buffer3;
							return item;
						}
						case eSECS_FORMAT.U2:
						{
							ushort[] numArray10 = this.ParseUInt2(aLength);
							if (numArray10.Length <= 1)
							{
								if (numArray10.Length == 1)
								{
									item.Value = numArray10[0];
								}
								return item;
							}
							item.Value = numArray10;
							return item;
						}
						case eSECS_FORMAT.U4:
						{
							uint[] numArray11 = this.ParseUInt4(aLength);
							if (numArray11.Length <= 1)
							{
								if (numArray11.Length == 1)
								{
									item.Value = numArray11[0];
								}
								return item;
							}
							item.Value = numArray11;
							return item;
						}
						}
					}
					else
					{
						double[] numArray12 = this.ParseDouble(aLength);
						if (numArray12.Length > 1)
						{
							item.Value = numArray12;
							return item;
						}
						if (numArray12.Length == 1)
						{
							item.Value = numArray12[0];
						}
						return item;
					}
					break;
				}
			}
			throw new Exception("Format Not meet the SECS standard");
		}

		private int getBinaryLength(int aLength)
		{
			byte[] aBytes = new byte[aLength];
			Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
			this.mReadIndex += aLength;
			return (int)Byte2SecsValue.GetInt(aBytes);
		}

		private string ParseAscii(int aLength)
		{
			string result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetAscii(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private byte[] ParseBinary(int aLength)
		{
			byte[] binary;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				binary = Byte2SecsValue.GetBinary(aBytes);
			}
			catch (Exception)
			{
				throw;
			}
			return binary;
		}

		private bool[] ParseBoolean(int aLength)
		{
			bool[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetBoolean(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private string ParseChar2(int aLength)
		{
			string str2;
			try
			{
				byte[] aBytes = new byte[2];
				byte[] buffer2 = new byte[aLength - 2];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, 2);
				this.mReadIndex += 2;
				Array.Copy(this.mDataItem, this.mReadIndex, buffer2, 0, buffer2.Length);
				this.mReadIndex += buffer2.Length;
				aBytes[0] = buffer2[0];
				aBytes[1] = buffer2[1];
				int @int = (int)Byte2SecsValue.GetInt(aBytes);
				string str = null;
				switch (@int)
				{
				case 1:
					str = Encoding.GetEncoding(1200).GetString(buffer2);
					break;
				case 2:
					str = Encoding.GetEncoding(65001).GetString(buffer2);
					break;
				case 3:
					str = Encoding.GetEncoding(20127).GetString(buffer2);
					break;
				case 4:
					str = Encoding.GetEncoding(28591).GetString(buffer2);
					break;
				case 5:
				case 6:
					str = Encoding.GetEncoding(874).GetString(buffer2);
					break;
				case 7:
					str = Encoding.GetEncoding(57002).GetString(buffer2);
					break;
				case 8:
					str = Encoding.GetEncoding(932).GetString(buffer2);
					break;
				case 9:
					str = Encoding.GetEncoding(20932).GetString(buffer2);
					break;
				case 10:
					str = Encoding.GetEncoding(51949).GetString(buffer2);
					break;
				case 11:
					str = Encoding.GetEncoding(936).GetString(buffer2);
					break;
				case 12:
					str = Encoding.GetEncoding(51936).GetString(buffer2);
					break;
				case 13:
					str = Encoding.GetEncoding(950).GetString(buffer2);
					break;
				}
				str2 = str;
			}
			catch (Exception)
			{
				throw;
			}
			return str2;
		}

		private double[] ParseDouble(int aLength)
		{
			double[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetF8(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private float[] ParseFloat(int aLength)
		{
			float[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetF4(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private sbyte[] ParseInt1(int aLength)
		{
			sbyte[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetI1(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private short[] ParseInt2(int aLength)
		{
			short[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetI2(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private int[] ParseInt4(int aLength)
		{
			int[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetI4(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private long[] ParseInt8(int aLength)
		{
			long[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetI8(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private string ParseJIS8(int aLength)
		{
			string str;
			try
			{
				byte[] buffer = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, buffer, 0, buffer.Length);
				this.mReadIndex += aLength;
				str = Encoding.Unicode.GetString(buffer);
			}
			catch (Exception)
			{
				throw;
			}
			return str;
		}

		private byte[] ParseUInt1(int aLength)
		{
			byte[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetU1(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private ushort[] ParseUInt2(int aLength)
		{
			ushort[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetU2(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private uint[] ParseUInt4(int aLength)
		{
			uint[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetU4(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}

		private ulong[] ParseUInt8(int aLength)
		{
			ulong[] result;
			try
			{
				byte[] aBytes = new byte[aLength];
				Array.Copy(this.mDataItem, this.mReadIndex, aBytes, 0, aBytes.Length);
				this.mReadIndex += aLength;
				result = Byte2SecsValue.GetU8(aBytes);
			}
			catch (Exception)
			{
				result = null;
			}
			return result;
		}
	}
}
