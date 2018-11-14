using System;

namespace HSMSDriver
{
	public class SECS1Parameters
	{
		internal bool AcceptDupBlock;

		internal bool AutoBaud = true;

		private int baudrate = 9600;

		private short deviceID;

		internal bool IgnoreSystemBytes;

		internal bool IsHost = true;

		private eSECS1_MS masterSlave;

		private int retryCount = 3;

		private string serialPort = "COM3";

		private double t1 = 0.5;

		private double t2 = 1.0;

		private int t3 = 45;

		private int t4 = 45;

		public int Baudrate
		{
			get
			{
				return this.baudrate;
			}
			set
			{
				this.baudrate = value;
			}
		}

		public short DeviceID
		{
			get
			{
				return this.deviceID;
			}
			set
			{
				this.deviceID = value;
			}
		}

		public eSECS1_MS MasterSlave
		{
			get
			{
				return this.masterSlave;
			}
			set
			{
				this.masterSlave = value;
			}
		}

		public int RetryCount
		{
			get
			{
				return this.retryCount;
			}
			set
			{
				if (value < 0 || value > 10)
				{
					throw new ArgumentOutOfRangeException("SECS1 Retry Count Out of Range!");
				}
				this.retryCount = value;
			}
		}

		public string SerialPort
		{
			get
			{
				return this.serialPort;
			}
			set
			{
				this.serialPort = value;
			}
		}

		public double T1
		{
			get
			{
				return this.t1;
			}
			set
			{
				this.t1 = value;
			}
		}

		public double T2
		{
			get
			{
				return this.t2;
			}
			set
			{
				this.t2 = value;
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

		public int T4
		{
			get
			{
				return this.t4;
			}
			set
			{
				this.t4 = value;
			}
		}
	}
}
