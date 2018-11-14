using System;
using System.Collections.Generic;
using System.Threading;

namespace HSMSDriver
{
	internal class SECS1Writer : AbstractThread
	{
		private string name;

		private SECS1Port secs1;

		private SECSMessage sendmsg;

		private Queue<SECSMessage> sendQueue = new Queue<SECSMessage>();

		private object syncObject = new object();

		public override string Name
		{
			get
			{
				return this.name + "#SECS1Writer";
			}
		}

		internal int RetryCount
		{
			get;
			set;
		}

		internal SECSBlock SendBlock
		{
			get;
			set;
		}

		internal Queue<SECSBlock> SendBlocks
		{
			get;
			set;
		}

		internal SECSMessage SendMsg
		{
			get
			{
				return this.sendmsg;
			}
			set
			{
				if (this.sendmsg != value)
				{
					lock (this.syncObject)
					{
						this.sendmsg = value;
					}
				}
			}
		}

		internal SECS1Writer(string name, SECS1Port secs1)
		{
			this.name = name;
			this.secs1 = secs1;
		}

		internal void Enqueue(SECSMessage msg)
		{
			lock (this.syncObject)
			{
				this.sendQueue.Enqueue(msg);
			}
		}

		private Queue<SECSBlock> GetSECS1Bytes(byte[] encodingBytes)
		{
			Queue<SECSBlock> queue = new Queue<SECSBlock>();
			int num2 = (encodingBytes.Length - 14) / 244;
			int data;
			for (int i = 0; i < num2; i = i++)
			{
				data = 0;
				SECSBlock block = new SECSBlock
				{
					Length = 254,
					IsControlMsg = false
				};
				byte[] buffer = new byte[10];
				Array.Copy(encodingBytes, 4, buffer, 0, 10);
				byte[] buffer2 = new byte[254];
				Array.Copy(encodingBytes, 10 + i * 244, buffer2, 0, 254);
				buffer[5] = (byte)(i + 1);
				block.Header = buffer;
				block.DataItem = buffer2;
				for (int j = 0; j < buffer.Length; j++)
				{
					data += (int)(buffer[j] & 255);
				}
				for (int k = 0; k < buffer2.Length; k++)
				{
					data += (int)(buffer2[k] & 255);
				}
				byte[] buffer3 = SecsValue2Byte.GetIntBytes(data, 4);
				block.CheckSum = new byte[4];
				Array.Copy(buffer3, 2, block.CheckSum, 0, 2);
				queue.Enqueue(block);
			}
			data = 0;
			int num3 = encodingBytes.Length - 14 - num2 * 244;
			SECSBlock block2 = new SECSBlock();
			byte[] buffer4 = new byte[10];
			Array.Copy(encodingBytes, 4, buffer4, 0, 10);
			buffer4[4] = 128;
			buffer4[5] = (byte)(num2 + 1);
			byte[] buffer5 = new byte[encodingBytes.Length - 14 - num2 * 244];
			Array.Copy(encodingBytes, 14 + num2 * 244, buffer5, 0, buffer5.Length);
			block2.Length = num3 + 10;
			block2.IsControlMsg = false;
			block2.Header = buffer4;
			block2.DataItem = buffer5;
			for (int l = 0; l < buffer4.Length; l++)
			{
				data += (int)(buffer4[l] & 255);
			}
			for (int m = 0; m < buffer5.Length; m++)
			{
				data += (int)(buffer5[m] & 255);
			}
			byte[] intBytes = SecsValue2Byte.GetIntBytes(data, 4);
			block2.CheckSum = new byte[2];
			Array.Copy(intBytes, 2, block2.CheckSum, 0, 2);
			queue.Enqueue(block2);
			return queue;
		}

		protected override void Run()
		{
			while (this.running)
			{
				try
				{
					if (this.SendMsg == null && this.sendQueue.Count > 0)
					{
						lock (this.syncObject)
						{
							this.SendMsg = this.sendQueue.Dequeue();
						}
					}
					if (this.SendMsg != null && this.secs1.PortStatus == eSECS1_PORT_STATUS.PortIdle)
					{
						lock (this.syncObject)
						{
							byte[] encodingBytes = new SECSEncoding(this.SendMsg).GetEncodingData((int)this.SendMsg.DeviceIdID, this.SendMsg.WBit, this.SendMsg.SystemBytes);
							this.SendBlocks = this.GetSECS1Bytes(encodingBytes);
							this.SendBlock = this.SendBlocks.Dequeue();
							while (this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortIdle, eSECS1_PORT_STATUS.PortCtrl) != 0)
							{
								Thread.Sleep(50);
							}
							this.secs1.StopTimer(eTimeout.T2);
							this.secs1.SendENQ();
							this.RetryCount = this.secs1.SECS1Para.RetryCount;
						}
					}
				}
				catch (Exception exception)
				{
					this.logger.Error("HSMSTimer.Run ", exception);
				}
				Thread.Sleep(50);
			}
		}
	}
}
