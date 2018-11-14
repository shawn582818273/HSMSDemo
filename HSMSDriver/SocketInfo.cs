using System;

namespace HSMSDriver
{
	internal class SocketInfo : ICloneable
	{
		private int connectInterval = 5000;

		private CONNECT_MODE connectMode;

		private string driverName = "";

		private string ipAddress = "";

		private string port = "0";

		private string protocol;

		private int timeout;

		private int timeoutCheckCount = 1;

		private long timeoutCheckSeconds = 60L;

		public int ConnectInterval
		{
			get
			{
				return this.connectInterval;
			}
			set
			{
				this.connectInterval = value;
			}
		}

		public CONNECT_MODE ConnectMode
		{
			get
			{
				return this.connectMode;
			}
			set
			{
				this.connectMode = value;
			}
		}

		public string DriverName
		{
			get
			{
				return this.driverName;
			}
			set
			{
				this.driverName = value;
			}
		}

		public string IpAddress
		{
			get
			{
				return this.ipAddress;
			}
			set
			{
				this.ipAddress = value;
			}
		}

		public bool IsActiveMode
		{
			get
			{
				return this.ConnectMode == CONNECT_MODE.ACTIVE;
			}
		}

		public string Port
		{
			get
			{
				return this.port;
			}
			set
			{
				this.port = value;
			}
		}

		public string Protocol
		{
			get
			{
				return this.protocol;
			}
			set
			{
				this.protocol = value;
			}
		}

		public int Timeout
		{
			get
			{
				return this.timeout;
			}
			set
			{
				this.timeout = value;
			}
		}

		public int TimeoutCheckCount
		{
			get
			{
				return this.timeoutCheckCount;
			}
			set
			{
				this.timeoutCheckCount = value;
			}
		}

		public long TimeoutCheckSeconds
		{
			get
			{
				return this.timeoutCheckSeconds;
			}
			set
			{
				this.timeoutCheckSeconds = value;
			}
		}

		public object Clone()
		{
			return base.MemberwiseClone() as SocketInfo;
		}
	}
}
