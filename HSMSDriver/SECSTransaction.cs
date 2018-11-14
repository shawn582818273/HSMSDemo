using System;

namespace HSMSDriver
{
	[Serializable]
	public class SECSTransaction
	{
		private string desc;

		private short deviceId;

		private bool expectreply;

		private bool inprocess;

		private string name;

		private SECSMessage primary;

		private SECSMessage secondary;

		private long sysbytes;

		private object tag;

		public string Description
		{
			get
			{
				if (this.desc != string.Empty)
				{
					return this.desc;
				}
				if (this.Primary != null)
				{
					return this.primary.Description;
				}
				return "";
			}
			set
			{
				this.desc = value;
			}
		}

		public short DeviceID
		{
			get
			{
				return this.deviceId;
			}
			internal set
			{
				this.deviceId = value;
			}
		}

		public bool ExpectReply
		{
			get
			{
				return this.expectreply;
			}
			set
			{
				this.expectreply = value;
			}
		}

		internal bool InProcess
		{
			get
			{
				return this.inprocess;
			}
			set
			{
				this.inprocess = value;
			}
		}

		public string Name
		{
			get
			{
				if (this.name != string.Empty)
				{
					return this.name;
				}
				if (this.Primary != null)
				{
					return string.Format("S{0}F{1}", this.primary.Stream, this.primary.Function);
				}
				return "";
			}
			set
			{
				this.name = value;
			}
		}

		public SECSMessage Primary
		{
			get
			{
				return this.primary;
			}
			set
			{
				this.primary = value;
				if (this.primary != null)
				{
					this.primary.Transaction = this;
				}
			}
		}

		public SECSMessage Secondary
		{
			get
			{
				return this.secondary;
			}
			set
			{
				this.secondary = value;
			}
		}

		public long SystemBytes
		{
			get
			{
				return this.sysbytes;
			}
			internal set
			{
				this.sysbytes = value;
			}
		}

		public object Tag
		{
			get
			{
				return this.tag;
			}
			set
			{
				this.tag = value;
			}
		}

		public SECSTransaction() : this("", "")
		{
		}

		public SECSTransaction(string name, string desc)
		{
			this.name = string.Empty;
			this.desc = string.Empty;
			this.Name = name;
			this.Description = desc;
		}

		public void Reply(SECSPort sPort)
		{
			if (sPort != null)
			{
				sPort.Reply(this);
			}
		}

		public void Send(SECSPort sPort)
		{
			if (sPort != null)
			{
				sPort.Send(this);
			}
		}
	}
}
