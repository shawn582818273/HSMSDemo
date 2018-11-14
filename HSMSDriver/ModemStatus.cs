using System;

namespace HSMSDriver
{
	internal struct ModemStatus
	{
		private uint status;

		public bool cts
		{
			get
			{
				return (this.status & 16u) != 0u;
			}
		}

		public bool dsr
		{
			get
			{
				return (this.status & 32u) != 0u;
			}
		}

		public bool rlsd
		{
			get
			{
				return (this.status & 128u) != 0u;
			}
		}

		public bool ring
		{
			get
			{
				return (this.status & 64u) != 0u;
			}
		}

		internal ModemStatus(uint val)
		{
			this.status = val;
		}
	}
}
