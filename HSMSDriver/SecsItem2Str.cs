using System;
using System.Text;

namespace HSMSDriver
{
	internal class SecsItem2Str
	{
		internal static string GetBinaryStr(byte[] data)
		{
			return ByteStringBuilder.ToLogString(data);
		}

		internal static string GetBooleanStr(bool[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i])
				{
					builder.Append("T ");
				}
				else
				{
					builder.Append("F ");
				}
			}
			return builder.ToString().Trim();
		}

		internal static string GetF4Str(float[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetF8Str(double[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetI1Str(sbyte[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetI2Str(short[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetI4Str(int[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetI8Str(long[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetSecsItemStr(int level, SECSItem data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < level; i++)
			{
				builder.Append("    ");
			}
			eSECS_FORMAT format = data.Format;
			if (format == eSECS_FORMAT.LIST)
			{
				builder.Append("<L [");
				builder.Append(data.ItemCount);
				builder.Append("]\n");
				int itemCount = data.ItemCount;
				if (itemCount > 0)
				{
					for (int j = 0; j < itemCount; j++)
					{
						builder.Append(SecsItem2Str.GetSecsItemStr(level + 1, data.Item(j + 1)));
					}
				}
				for (int k = 0; k < level; k++)
				{
					builder.Append("    ");
				}
				builder.Append(">\n");
			}
			else
			{
				eSECS_FORMAT eSECSFORMAT = format;
				if (eSECSFORMAT <= eSECS_FORMAT.ASCII)
				{
					switch (eSECSFORMAT)
					{
					case eSECS_FORMAT.BINARY:
						if (data.IsEmpty)
						{
							builder.Append("<B [0] ''>\n");
						}
						else
						{
							byte[] buffer;
							if (!data.IsArray)
							{
								buffer = new byte[]
								{
									(byte)data.Value
								};
							}
							else
							{
								buffer = (byte[])data.Value;
							}
							string binaryStr = SecsItem2Str.GetBinaryStr(buffer);
							builder.Append("<B [");
							builder.Append(buffer.Length);
							builder.Append("] '");
							builder.Append(binaryStr);
							builder.Append("' >\n");
						}
						break;
					case eSECS_FORMAT.BOOLEAN:
						if (!data.IsEmpty)
						{
							bool[] flagArray;
							if (!data.IsArray)
							{
								flagArray = new bool[]
								{
									(bool)data.Value
								};
							}
							else
							{
								flagArray = (bool[])data.Value;
							}
							string booleanStr = SecsItem2Str.GetBooleanStr(flagArray);
							builder.Append("<BOOLEAN [");
							builder.Append(flagArray.Length);
							builder.Append("] ");
							builder.Append(booleanStr);
							builder.Append(" >\n");
						}
						break;
					default:
						if (eSECSFORMAT == eSECS_FORMAT.ASCII)
						{
							string str = "";
							if (!data.IsEmpty)
							{
								str = (string)data.Value;
							}
							builder.Append("<A [");
							builder.Append(str.Length);
							builder.Append("] '");
							builder.Append(str);
							builder.Append("' >\n");
						}
						break;
					}
				}
				else
				{
					switch (eSECSFORMAT)
					{
					case eSECS_FORMAT.I8:
						if (!data.IsEmpty)
						{
							long[] numArray8;
							if (!data.IsArray)
							{
								numArray8 = new long[]
								{
									(long)data.Value
								};
							}
							else
							{
								numArray8 = (long[])data.Value;
							}
							string str2 = SecsItem2Str.GetI8Str(numArray8);
							builder.Append("<I8 [");
							builder.Append(numArray8.Length);
							builder.Append("] ");
							builder.Append(str2);
							builder.Append(" >\n");
						}
						break;
					case eSECS_FORMAT.I1:
						if (!data.IsEmpty)
						{
							sbyte[] numArray9;
							if (!data.IsArray)
							{
								numArray9 = new sbyte[]
								{
									(sbyte)data.Value
								};
							}
							else
							{
								numArray9 = (sbyte[])data.Value;
							}
							string str3 = SecsItem2Str.GetI1Str(numArray9);
							builder.Append("<I1 [");
							builder.Append(numArray9.Length);
							builder.Append("] ");
							builder.Append(str3);
							builder.Append(" >\n");
						}
						break;
					case eSECS_FORMAT.I2:
						if (!data.IsEmpty)
						{
							short[] numArray10;
							if (!data.IsArray)
							{
								numArray10 = new short[]
								{
									(short)data.Value
								};
							}
							else
							{
								numArray10 = (short[])data.Value;
							}
							string str4 = SecsItem2Str.GetI2Str(numArray10);
							builder.Append("<I2 [");
							builder.Append(numArray10.Length);
							builder.Append("] ");
							builder.Append(str4);
							builder.Append(" >\n");
						}
						break;
					case (eSECS_FORMAT)27:
					case (eSECS_FORMAT)29:
					case (eSECS_FORMAT)30:
					case (eSECS_FORMAT)31:
						break;
					case eSECS_FORMAT.I4:
						if (!data.IsEmpty)
						{
							int[] numArray11;
							if (!data.IsArray)
							{
								numArray11 = new int[]
								{
									(int)data.Value
								};
							}
							else
							{
								numArray11 = (int[])data.Value;
							}
							string str5 = SecsItem2Str.GetI4Str(numArray11);
							builder.Append("<I4 [");
							builder.Append(numArray11.Length);
							builder.Append("] ");
							builder.Append(str5);
							builder.Append(" >\n");
						}
						break;
					case eSECS_FORMAT.F8:
						if (!data.IsEmpty)
						{
							double[] numArray12;
							if (data.IsArray)
							{
								numArray12 = (double[])data.Value;
							}
							else
							{
								numArray12 = new double[]
								{
									(double)data.Value
								};
							}
							string str6 = SecsItem2Str.GetF8Str(numArray12);
							builder.Append("<F8 [");
							builder.Append(numArray12.Length);
							builder.Append("] ");
							builder.Append(str6);
							builder.Append(" >\n");
						}
						break;
					default:
						switch (eSECSFORMAT)
						{
						case eSECS_FORMAT.F4:
							if (!data.IsEmpty)
							{
								float[] numArray13;
								if (!data.IsArray)
								{
									numArray13 = new float[]
									{
										(float)data.Value
									};
								}
								else
								{
									numArray13 = (float[])data.Value;
								}
								string str7 = SecsItem2Str.GetF4Str(numArray13);
								builder.Append("<F4 [");
								builder.Append(numArray13.Length);
								builder.Append("] ");
								builder.Append(str7);
								builder.Append(" >\n");
							}
							break;
						case eSECS_FORMAT.U8:
							if (!data.IsEmpty)
							{
								ulong[] numArray14;
								if (!data.IsArray)
								{
									numArray14 = new ulong[]
									{
										(ulong)data.Value
									};
								}
								else
								{
									numArray14 = (ulong[])data.Value;
								}
								string str8 = SecsItem2Str.GetU8Str(numArray14);
								builder.Append("<U8 [");
								builder.Append(numArray14.Length);
								builder.Append("] ");
								builder.Append(str8);
								builder.Append(" >\n");
							}
							break;
						case eSECS_FORMAT.U1:
							if (!data.IsEmpty)
							{
								byte[] buffer2;
								if (!data.IsArray)
								{
									buffer2 = new byte[]
									{
										(byte)data.Value
									};
								}
								else
								{
									buffer2 = (byte[])data.Value;
								}
								string str9 = SecsItem2Str.GetU1Str(buffer2);
								builder.Append("<U1 [");
								builder.Append(buffer2.Length);
								builder.Append("] ");
								builder.Append(str9);
								builder.Append(" >\n");
							}
							break;
						case eSECS_FORMAT.U2:
							if (!data.IsEmpty)
							{
								ushort[] numArray15;
								if (!data.IsArray)
								{
									numArray15 = new ushort[]
									{
										(ushort)data.Value
									};
								}
								else
								{
									numArray15 = (ushort[])data.Value;
								}
								string str10 = SecsItem2Str.GetU2Str(numArray15);
								builder.Append("<U2 [");
								builder.Append(numArray15.Length);
								builder.Append("] ");
								builder.Append(str10);
								builder.Append(" >\n");
							}
							break;
						case eSECS_FORMAT.U4:
							if (!data.IsEmpty)
							{
								uint[] numArray16;
								if (!data.IsArray)
								{
									numArray16 = new uint[]
									{
										(uint)data.Value
									};
								}
								else
								{
									numArray16 = (uint[])data.Value;
								}
								string str11 = SecsItem2Str.GetU4Str(numArray16);
								builder.Append("<U4 [");
								builder.Append(numArray16.Length);
								builder.Append("] ");
								builder.Append(str11);
								builder.Append(" >\n");
							}
							break;
						}
						break;
					}
				}
			}
			return builder.ToString();
		}

		internal static string GetSecsMessageStr(SECSMessage msg)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("S{0}F{1}", msg.Stream, msg.Function);
			if (msg.WBit)
			{
				builder.Append(" W");
			}
			builder.Append(" System Bytes=");
			builder.Append(msg.SystemBytes);
			builder.Append("\n");
			SECSItem root = msg.Root;
			if (root != null)
			{
				builder.Append(SecsItem2Str.GetSecsItemStr(0, root));
				builder.Append(".\n");
			}
			return builder.ToString();
		}

		internal static string GetU1Str(byte[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetU2Str(ushort[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetU4Str(uint[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}

		internal static string GetU8Str(ulong[] data)
		{
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				builder.Append(data[i]);
				builder.Append(" ");
			}
			return builder.ToString().Trim();
		}
	}
}
