using System;

namespace HSMSDriver
{
	[Serializable]
	public class SECSMessage : ICloneable
	{
		private string desc;

		private short deviceId;

		private int function;

		private bool isHost;

		private byte[] mhdr;

		private string name;

		private SECSItem root;

		private int stream;

		private long systemBytes;

		private bool wBit;

		public string Description
		{
			get
			{
				return this.desc;
			}
			set
			{
				this.desc = value;
			}
		}

		internal short DeviceIdID
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

		public int Function
		{
			get
			{
				return this.function;
			}
			set
			{
				this.function = value;
			}
		}

		internal byte[] Header
		{
			get
			{
				return this.mhdr;
			}
			set
			{
				this.mhdr = value;
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
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
			set
			{
				this.name = value;
			}
		}

		public SECSItem Root
		{
			get
			{
				return this.root;
			}
			set
			{
				this.root = value;
			}
		}

		public int Stream
		{
			get
			{
				return this.stream;
			}
			set
			{
				this.stream = value;
			}
		}

		public long SystemBytes
		{
			get
			{
				return this.systemBytes;
			}
			internal set
			{
				this.systemBytes = value;
			}
		}

		public SECSTransaction Transaction
		{
			get;
			set;
		}

		public bool WBit
		{
			get
			{
				return this.wBit;
			}
			set
			{
				this.wBit = value;
			}
		}

		public SECSMessage() : this("", "")
		{
		}

		public SECSMessage(string name, string desc)
		{
			this.name = name;
			this.desc = desc;
		}

		public object Clone()
		{
			SECSMessage message = (SECSMessage)base.MemberwiseClone();
			if (this.root != null)
			{
				message.root = (SECSItem)this.root.Clone();
			}
			return message;
		}

		public override string ToString()
		{
			return SecsItem2Str.GetSecsMessageStr(this);
		}
	}
}
