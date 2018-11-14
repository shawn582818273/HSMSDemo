using System;
using System.Text;

namespace HSMSDriver
{
	internal struct QueueStatus
	{
		private uint status;

		private uint inQueue;

		private uint outQueue;

		private uint inQueueSize;

		private uint outQueueSize;

		public bool ctsHold
		{
			get
			{
				return (this.status & 1u) != 0u;
			}
		}

		public bool dsrHold
		{
			get
			{
				return (this.status & 2u) != 0u;
			}
		}

		public bool rlsdHold
		{
			get
			{
				return (this.status & 4u) != 0u;
			}
		}

		public bool xoffHold
		{
			get
			{
				return (this.status & 8u) != 0u;
			}
		}

		public bool xoffSent
		{
			get
			{
				return (this.status & 16u) != 0u;
			}
		}

		public bool immediateWaiting
		{
			get
			{
				return (this.status & 64u) != 0u;
			}
		}

		public long InQueue
		{
			get
			{
				return (long)((ulong)this.inQueue);
			}
		}

		public long OutQueue
		{
			get
			{
				return (long)((ulong)this.outQueue);
			}
		}

		public long InQueueSize
		{
			get
			{
				return (long)((ulong)this.inQueueSize);
			}
		}

		public long OutQueueSize
		{
			get
			{
				return (long)((ulong)this.outQueueSize);
			}
		}

		internal QueueStatus(uint stat, uint inQ, uint outQ, uint inQs, uint outQs)
		{
			this.status = stat;
			this.inQueue = inQ;
			this.outQueue = outQ;
			this.inQueueSize = inQs;
			this.outQueueSize = outQs;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder("The reception queue is ", 60);
			if (this.inQueueSize == 0u)
			{
				builder.Append("of unknown size and ");
			}
			else
			{
				builder.Append(this.inQueueSize.ToString() + " bytes long and ");
			}
			if (this.inQueue == 0u)
			{
				builder.Append("is empty.");
			}
			else if (this.inQueue == 1u)
			{
				builder.Append("contains 1 byte.");
			}
			else
			{
				builder.Append("contains ");
				builder.Append(this.inQueue.ToString());
				builder.Append(" bytes.");
			}
			builder.Append(" The transmission queue is ");
			if (this.outQueueSize == 0u)
			{
				builder.Append("of unknown size and ");
			}
			else
			{
				builder.Append(this.outQueueSize.ToString() + " bytes long and ");
			}
			if (this.outQueue == 0u)
			{
				builder.Append("is empty");
			}
			else if (this.outQueue == 1u)
			{
				builder.Append("contains 1 byte. It is ");
			}
			else
			{
				builder.Append("contains ");
				builder.Append(this.outQueue.ToString());
				builder.Append(" bytes. It is ");
			}
			if (this.outQueue > 0u)
			{
				if (this.ctsHold || this.dsrHold || this.rlsdHold || this.xoffHold || this.xoffSent)
				{
					builder.Append("holding on");
					if (this.ctsHold)
					{
						builder.Append(" CTS");
					}
					if (this.dsrHold)
					{
						builder.Append(" DSR");
					}
					if (this.rlsdHold)
					{
						builder.Append(" RLSD");
					}
					if (this.xoffHold)
					{
						builder.Append(" Rx XOff");
					}
					if (this.xoffSent)
					{
						builder.Append(" Tx XOff");
					}
				}
				else
				{
					builder.Append("pumping data");
				}
			}
			builder.Append(". The immediate buffer is ");
			if (this.immediateWaiting)
			{
				builder.Append("full.");
			}
			else
			{
				builder.Append("empty.");
			}
			return builder.ToString();
		}
	}
}
