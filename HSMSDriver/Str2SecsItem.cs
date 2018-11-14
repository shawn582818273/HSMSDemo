using System;
using System.Globalization;

namespace HSMSDriver
{
	internal class Str2SecsItem
	{
		internal static byte[] GetBinary(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			byte[] buffer = new byte[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				buffer[i] = byte.Parse(strArray[i], NumberStyles.HexNumber);
			}
			return buffer;
		}

		internal static bool[] GetBoolean(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			bool[] flagArray = new bool[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				if (strArray[i] == "T" || strArray[i] == "True")
				{
					flagArray[i] = true;
				}
				else
				{
					if (strArray[i] != "F" && strArray[i] != "False")
					{
						throw new Exception("Not Support translate " + strArray[i] + " to boolean");
					}
					flagArray[i] = false;
				}
			}
			return flagArray;
		}

		internal static float[] GetF4(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			float[] numArray = new float[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = float.Parse(strArray[i]);
			}
			return numArray;
		}

		internal static double[] GetF8(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			double[] numArray = new double[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = double.Parse(strArray[i]);
			}
			return numArray;
		}

		internal static sbyte[] GetI1(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			sbyte[] numArray = new sbyte[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = sbyte.Parse(strArray[i]);
			}
			return numArray;
		}

		internal static short[] GetI2(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			short[] numArray = new short[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = short.Parse(strArray[i]);
			}
			return numArray;
		}

		internal static int[] GetI4(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			int[] numArray = new int[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = int.Parse(strArray[i]);
			}
			return numArray;
		}

		internal static long[] GetI8(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			long[] numArray = new long[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = long.Parse(strArray[i]);
			}
			return numArray;
		}

		internal static byte[] GetU1(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			byte[] buffer = new byte[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				buffer[i] = byte.Parse(strArray[i]);
			}
			return buffer;
		}

		internal static ushort[] GetU2(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			ushort[] numArray = new ushort[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = ushort.Parse(strArray[i]);
			}
			return numArray;
		}

		internal static uint[] GetU4(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			uint[] numArray = new uint[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = uint.Parse(strArray[i]);
			}
			return numArray;
		}

		internal static ulong[] GetU8(string data)
		{
			string[] strArray = data.Split(new char[]
			{
				' '
			}, StringSplitOptions.RemoveEmptyEntries);
			ulong[] numArray = new ulong[strArray.Length];
			for (int i = 0; i < strArray.Length; i++)
			{
				numArray[i] = ulong.Parse(strArray[i]);
			}
			return numArray;
		}
	}
}
