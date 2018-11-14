using System;

namespace HSMSDriver
{
	public class HSMSParameters
	{
		private eHSMS_CONNECT_MODE connectionMode = eHSMS_CONNECT_MODE.ACTIVE;

		private short deviceId;

		private bool isHost = true;

		private int linkTest = 120;

		private string localIP = "127.0.0.1";

		private int localPort = 6000;

		private string remoteIP = "127.0.0.1";

		private int remotePort = 5000;

		private int t3 = 45;

		private int t5 = 10;

		private int t6 = 5;

		private int t7 = 10;

		private int t8 = 10;

		public eHSMS_CONNECT_MODE ConnectionMode
		{
			get
			{
				return this.connectionMode;
			}
			set
			{
				this.connectionMode = value;
				if (this.connectionMode == eHSMS_CONNECT_MODE.ACTIVE)
				{
					this.isHost = true;
					return;
				}
				this.isHost = false;
			}
		}

		internal short DeviceID
		{
			get
			{
				return this.deviceId;
			}
			set
			{
				this.deviceId = value;
			}
		}

		public bool IsHost
		{
			get
			{
				return this.isHost;
			}
			set
			{
				this.isHost = value;
				if (this.isHost)
				{
					this.connectionMode = eHSMS_CONNECT_MODE.ACTIVE;
					return;
				}
				this.connectionMode = eHSMS_CONNECT_MODE.PASSIVE;
			}
		}

		public int LinkTest
		{
			get
			{
				return this.linkTest;
			}
			set
			{
				this.linkTest = value;
			}
		}

		public string LocalIP
		{
			get
			{
				return this.localIP;
			}
			set
			{
				this.localIP = value;
			}
		}

		public int LocalPort
		{
			get
			{
				return this.localPort;
			}
			set
			{
				this.localPort = value;
			}
		}

		public string RemoteIP
		{
			get
			{
				return this.remoteIP;
			}
			set
			{
				this.remoteIP = value;
			}
		}

		public int RemotePort
		{
			get
			{
				return this.remotePort;
			}
			set
			{
				this.remotePort = value;
			}
		}

		public int T3
		{
			get
			{
				return this.t3;
			}
			set
			{
				this.t3 = value;
			}
		}

		public int T5
		{
			get
			{
				return this.t5;
			}
			set
			{
				this.t5 = value;
			}
		}

		public int T6
		{
			get
			{
				return this.t6;
			}
			set
			{
				this.t6 = value;
			}
		}

		public int T7
		{
			get
			{
				return this.t7;
			}
			set
			{
				this.t7 = value;
			}
		}

		public int T8
		{
			get
			{
				return this.t8;
			}
			set
			{
				this.t8 = value;
			}
		}
	}
}
