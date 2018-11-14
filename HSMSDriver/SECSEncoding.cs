using System;
using System.Collections.Generic;
using System.Linq;

namespace HSMSDriver
{
	internal class SECSEncoding
	{
		private SECSMessage mMsg;

		private Queue<byte> mTotalBytes = new Queue<byte>();

		public SECSEncoding(SECSMessage aMsg)
		{
			this.mMsg = aMsg;
		}

		private void AppendByte(byte aByte)
		{
			this.mTotalBytes.Enqueue(aByte);
		}

		private void AppendBytes(byte[] data)
		{
			if (data != null)
			{
				for (int i = 0; i < data.Length; i++)
				{
					this.mTotalBytes.Enqueue(data[i]);
				}
			}
		}

		public byte[] GetEncodingData(int device, bool wbit, long systembytes)
		{
			byte[] data = null;
			SECSItem root = null;
			long aLength;
			if (this.mMsg == null)
			{
				aLength = 10L;
			}
			else
			{
				root = this.mMsg.Root;
				if (root == null)
				{
					aLength = 10L;
				}
				else
				{
					data = root.Raw();
					aLength = (long)(data.Length + 10);
				}
			}
			this.MakeMsgLengthBytes(aLength);
			this.MakeMsgHeader(device, wbit, systembytes);
			if (root != null && data != null && data.Length > 0)
			{
				this.AppendBytes(data);
			}
			return this.mTotalBytes.ToArray<byte>();
		}

		private void MakeMsgHeader(int device, bool wbit, long systembytes)
		{
			byte[] data = new byte[10];
			byte[] intBytes = SecsValue2Byte.GetIntBytes(device, 2);
			data[0] = intBytes[0];
			data[1] = intBytes[1];
			try
			{
				if (wbit)
				{
					data[2] = (byte)(this.mMsg.Stream - 128);
				}
				else
				{
					data[2] = (byte)this.mMsg.Stream;
				}
				data[3] = (byte)this.mMsg.Function;
			}
			catch (Exception)
			{
			}
			intBytes = SecsValue2Byte.GetIntBytes((int)systembytes, 4);
			data[6] = intBytes[0];
			data[7] = intBytes[1];
			data[8] = intBytes[2];
			data[9] = intBytes[3];
			this.mMsg.Header = data;
			this.AppendBytes(data);
		}

		private void MakeMsgLengthBytes(long aLength)
		{
			byte[] intBytes = SecsValue2Byte.GetIntBytes((int)aLength, 4);
			this.AppendBytes(intBytes);
		}
	}
}
