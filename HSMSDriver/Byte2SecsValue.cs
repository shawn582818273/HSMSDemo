using System;
using System.Text;

namespace HSMSDriver
{
	internal class Byte2SecsValue
	{
		internal static short AdjustDeviceID(short aDeviceID)
		{
			if (aDeviceID >= 0)
			{
				return aDeviceID;
			}
			byte[] intBytes = SecsValue2Byte.GetIntBytes((int)aDeviceID, 2);
			int num = (int)(intBytes[0] & 255);
			num -= 128;
			intBytes[0] = (byte)num;
			return (short)Byte2SecsValue.GetInt(intBytes);
		}

		internal static string GetAscii(byte[] aBytes)
		{
			char ch = '\0';
			string str = Encoding.Default.GetString(aBytes);
			int index = str.IndexOf(ch);
			if (index >= 0)
			{
				str = str.Substring(0, index);
			}
			return str;
		}

		internal static int GetBinary(byte aByte)
		{
			return (int)(aByte & 255);
		}

		internal static byte[] GetBinary(byte[] aBytes)
		{
			return (byte[])aBytes.Clone();
		}

		internal static bool GetBoolean(byte aByte)
		{
			return (aByte & 255) != 0;
		}

		internal static bool[] GetBoolean(byte[] aBytes)
		{
			bool[] flagArray = new bool[aBytes.Length];
			for (int i = 0; i < aBytes.Length; i++)
			{
				flagArray[i] = ((aBytes[i] & 255) != 0);
			}
			return flagArray;
		}

		internal static float[] GetF4(byte[] aBytes)
		{
			byte[] buffer = new byte[4];
			float[] numArray = new float[aBytes.Length / 4];
			for (int i = 0; i < aBytes.Length; i += 4)
			{
				Array.Copy(aBytes, i, buffer, 0, buffer.Length);
				buffer = Byte2SecsValue.Swap(buffer);
				float num2 = BitConverter.ToSingle(buffer, 0);
				numArray[i / 4] = num2;
			}
			return numArray;
		}

		internal static double[] GetF8(byte[] aBytes)
		{
			byte[] buffer = new byte[8];
			double[] numArray = new double[aBytes.Length / 8];
			for (int i = 0; i < aBytes.Length; i += 8)
			{
				Array.Copy(aBytes, i, buffer, 0, buffer.Length);
				buffer = Byte2SecsValue.Swap(buffer);
				double num2 = BitConverter.ToDouble(buffer, 0);
				numArray[i / 8] = num2;
			}
			return numArray;
		}

		internal static sbyte[] GetI1(byte[] aBytes)
		{
			sbyte[] numArray = new sbyte[aBytes.Length];
			for (int i = 0; i < aBytes.Length; i++)
			{
				int num2 = (int)(aBytes[i] & 255);
				if (num2 >= 128)
				{
					num2 = num2 - 255 - 1;
				}
				numArray[i] = (sbyte)num2;
			}
			return numArray;
		}

		internal static short[] GetI2(byte[] aBytes)
		{
			short[] numArray = new short[aBytes.Length / 2];
			for (int i = 0; i < aBytes.Length; i += 2)
			{
				short num2 = (short)((int)aBytes[i] << 8 | (int)aBytes[i + 1]);
				numArray[i / 2] = num2;
			}
			return numArray;
		}

		internal static int[] GetI4(byte[] aBytes)
		{
			int[] numArray = new int[aBytes.Length / 4];
			for (int i = 0; i < aBytes.Length; i += 4)
			{
				int num2 = (int)aBytes[i] << 24 | (int)aBytes[i + 1] << 16 | (int)aBytes[i + 2] << 8 | (int)aBytes[i + 3];
				numArray[i / 4] = num2;
			}
			return numArray;
		}

		internal static long[] GetI8(byte[] aBytes)
		{
			long[] numArray = new long[aBytes.Length / 8];
			for (int i = 0; i < aBytes.Length; i += 8)
			{
				long num2 = (long)((int)aBytes[i] << 24 | (int)aBytes[i + 1] << 16 | (int)aBytes[i + 2] << 8 | (int)aBytes[i + 3] | (int)aBytes[i + 4] << 24 | (int)aBytes[i + 5] << 16 | (int)aBytes[i + 6] << 8 | (int)aBytes[i + 7]);
				numArray[i / 8] = num2;
			}
			return numArray;
		}

		internal static long GetInt(byte[] aBytes)
		{
			long num = 0L;
			switch (aBytes.Length)
			{
			case 1:
			{
				int num2 = (int)(aBytes[0] & 255);
				return (long)num2;
			}
			case 2:
				return (long)Byte2SecsValue.GetI2(aBytes)[0];
			case 3:
				return num;
			case 4:
				return (long)Byte2SecsValue.GetI4(aBytes)[0];
			case 5:
			case 6:
			case 7:
				return num;
			case 8:
				return Byte2SecsValue.GetI8(aBytes)[0];
			default:
				return num;
			}
		}

		internal static long GetLong(byte[] aBytes)
		{
			return (long)((ulong)Byte2SecsValue.GetU4(aBytes)[0]);
		}

		internal static byte[] GetU1(byte[] aBytes)
		{
			return (byte[])aBytes.Clone();
		}

		internal static ushort[] GetU2(byte[] aBytes)
		{
			ushort[] numArray = new ushort[aBytes.Length / 2];
			for (int i = 0; i < aBytes.Length; i += 2)
			{
				ushort num2 = (ushort)aBytes[i];
				num2 = (ushort)(num2 << 8);
				num2 |= (ushort)aBytes[i + 1];
				numArray[i / 2] = num2;
			}
			return numArray;
		}

		internal static uint[] GetU4(byte[] aBytes)
		{
			uint[] numArray = new uint[aBytes.Length / 4];
			for (int i = 0; i < aBytes.Length; i += 4)
			{
				uint num2 = (uint)((int)aBytes[i] << 24 | (int)aBytes[i + 1] << 16 | (int)aBytes[i + 2] << 8 | (int)aBytes[i + 3]);
				numArray[i / 4] = num2;
			}
			return numArray;
		}

		internal static ulong[] GetU8(byte[] aBytes)
		{
			ulong[] numArray = new ulong[aBytes.Length / 8];
			for (int i = 0; i < aBytes.Length; i += 8)
			{
				ulong num2 = (ulong)((long)((int)aBytes[i] << 24 | (int)aBytes[i + 1] << 16 | (int)aBytes[i + 2] << 8 | (int)aBytes[i + 3] | (int)aBytes[i + 4] << 24 | (int)aBytes[i + 5] << 16 | (int)aBytes[i + 6] << 8 | (int)aBytes[i + 7]));
				numArray[i / 8] = num2;
			}
			return numArray;
		}

		internal static byte[] Swap(byte[] aBytes)
		{
			byte[] buffer = new byte[aBytes.Length];
			for (int i = 0; i < aBytes.Length; i++)
			{
				buffer[aBytes.Length - 1 - i] = aBytes[i];
			}
			return buffer;
		}
	}
}
