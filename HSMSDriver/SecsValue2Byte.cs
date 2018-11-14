using System;

namespace HSMSDriver
{
	internal class SecsValue2Byte
	{
		internal static byte[] GetBinaryBytes(byte[] data)
		{
			return (byte[])data.Clone();
		}

		internal static byte[] GetBooleanBytes(bool[] data)
		{
			byte[] buffer = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				if (data[i])
				{
					buffer[i] = 1;
				}
				else
				{
					buffer[i] = 0;
				}
			}
			return buffer;
		}

		internal static byte[] GetF4Bytes(float[] data)
		{
			byte[] buffer = new byte[data.Length * 4];
			for (int i = 0; i < data.Length; i++)
			{
				byte[] aBytes = new byte[4];
				Array.Copy(BitConverter.GetBytes(data[i]), aBytes, 4);
				Array.Copy(Byte2SecsValue.Swap(aBytes), 0, buffer, 4 * i, 4);
			}
			return buffer;
		}

		internal static byte[] GetF8Bytes(double[] data)
		{
			byte[] buffer = new byte[data.Length * 8];
			for (int i = 0; i < data.Length; i++)
			{
				byte[] aBytes = new byte[8];
				Array.Copy(BitConverter.GetBytes(data[i]), aBytes, 8);
				Array.Copy(Byte2SecsValue.Swap(aBytes), 0, buffer, 8 * i, 8);
			}
			return buffer;
		}

		internal static byte[] GetI1Bytes(sbyte[] data)
		{
			byte[] buffer = new byte[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				buffer[i] = (byte)data[i];
			}
			return buffer;
		}

		internal static byte[] GetI2Bytes(short[] data)
		{
			byte[] buffer = new byte[data.Length * 2];
			for (int i = 0; i < data.Length; i++)
			{
				buffer[2 * i] = (byte)(((int)data[i] & 65280) >> 8);
				buffer[2 * i + 1] = (byte)(data[i] & 255);
			}
			return buffer;
		}

		internal static byte[] GetI4Bytes(int[] data)
		{
			byte[] buffer = new byte[data.Length * 4];
			for (int i = 0; i < data.Length; i++)
			{
				buffer[4 * i] = (byte)((data[i] & -16777216) >> 24);
				buffer[4 * i + 1] = (byte)((data[i] & 16711680) >> 16);
				buffer[4 * i + 2] = (byte)((data[i] & 65280) >> 8);
				buffer[4 * i + 3] = (byte)(data[i] & 255);
			}
			return buffer;
		}

		internal static byte[] GetI8Bytes(long[] data)
		{
			byte[] buffer = new byte[data.Length * 8];
			for (int i = 0; i < data.Length; i++)
			{
				byte[] aBytes = new byte[8];
				Array.Copy(BitConverter.GetBytes(data[i]), aBytes, 8);
				Array.Copy(Byte2SecsValue.Swap(aBytes), 0, buffer, 8 * i, 8);
			}
			return buffer;
		}

		internal static byte[] GetIntBytes(int data, int aFormatLength)
		{
			if (aFormatLength == 2)
			{
				return new byte[]
				{
					(byte)((data & 65280) >> 8),
					(byte)(data & 255)
				};
			}
			return new byte[]
			{
				(byte)((data & -16777216) >> 24),
				(byte)((data & 16711680) >> 16),
				(byte)((data & 65280) >> 8),
				(byte)(data & 255)
			};
		}

		internal static byte[] GetU2Bytes(ushort[] data)
		{
			byte[] buffer = new byte[data.Length * 2];
			for (int i = 0; i < data.Length; i++)
			{
				buffer[2 * i] = (byte)((data[i] & 65280) >> 8);
				buffer[2 * i + 1] = (byte)(data[i] & 255);
			}
			return buffer;
		}

		internal static byte[] GetU4Bytes(uint[] data)
		{
			byte[] buffer = new byte[data.Length * 4];
			for (int i = 0; i < data.Length; i++)
			{
				buffer[4 * i] = (byte)(((ulong)data[i] & 18446744073692774400uL) >> 24);
				buffer[4 * i + 1] = (byte)((data[i] & 16711680u) >> 16);
				buffer[4 * i + 2] = (byte)((data[i] & 65280u) >> 8);
				buffer[4 * i + 3] = (byte)(data[i] & 255u);
			}
			return buffer;
		}

		internal static byte[] GetU8Bytes(ulong[] data)
		{
			byte[] buffer = new byte[data.Length * 8];
			for (int i = 0; i < data.Length; i++)
			{
				byte[] aBytes = new byte[8];
				Array.Copy(BitConverter.GetBytes(data[i]), aBytes, 8);
				Array.Copy(Byte2SecsValue.Swap(aBytes), 0, buffer, 8 * i, 8);
			}
			return buffer;
		}
	}
}
