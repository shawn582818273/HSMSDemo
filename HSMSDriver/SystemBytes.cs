using System;

namespace HSMSDriver
{
	internal class SystemBytes
	{
		private long mCtrlSystemBytes;

		private long mSystemBytes = 1L;

		private object syncObj1 = new object();

		private object syncObj2 = new object();

		public long GetCtrlSystemBytes()
		{
			long result;
			lock (this.syncObj2)
			{
				if (this.mCtrlSystemBytes <= 0L)
				{
					this.mCtrlSystemBytes = 1L;
				}
				long num2;
				this.mCtrlSystemBytes = (num2 = this.mCtrlSystemBytes) + 1L;
				long num = num2;
				result = num;
			}
			return result;
		}

		public long GetSystemBytes()
		{
			long result;
			lock (this.syncObj1)
			{
				if (this.mSystemBytes < 0L)
				{
					this.mSystemBytes = 1L;
					result = 1L;
				}
				else
				{
					long num2;
					this.mSystemBytes = (num2 = this.mSystemBytes) + 1L;
					long num = num2;
					result = num;
				}
			}
			return result;
		}
	}
}
