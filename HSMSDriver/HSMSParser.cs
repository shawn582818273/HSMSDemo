using System;
using System.Collections.Generic;
using System.Threading;

namespace HSMSDriver
{
	internal class HSMSParser : AbstractThread
	{
		private Queue<SECSBlock> mQueueConvert = new Queue<SECSBlock>();

		private string name;

		private object syncObject = new object();

		public event SocketEvent.ParseError OnParseError;

		public event SocketEvent.OnReceived OnReceived;

		public override string Name
		{
			get
			{
				return this.name + "#Parser";
			}
		}

		public HSMSParser(string name)
		{
			this.name = name;
		}

		public void Parse(SECSBlock mHSMSItem)
		{
			try
			{
				SECSDecoding decoding = new SECSDecoding();
				string str = ByteStringBuilder.ToLogString(mHSMSItem.Header);
				string str2 = ByteStringBuilder.ToLogString(mHSMSItem.DataItem);
				SECSMessage msg = decoding.Byte_TO_SecsMessage(mHSMSItem.Header);
				msg.Root = decoding.Byte_TO_SecsItem(mHSMSItem.DataItem);
				msg.Header = mHSMSItem.Header;
				//this.logger.Info(string.Format("[RECV] S{0}F{1} {2} System Bytes={3} {4} {5}", new object[]
				//{
				//	msg.Stream,
				//	msg.Function,
				//	msg.WBit ? "W" : "",
				//	msg.SystemBytes,
				//	str,
				//	str2
				//}));
				//this.logger.Warn("[RECV] " + SecsItem2Str.GetSecsMessageStr(msg));
				if (this.OnReceived != null)
				{
					this.OnReceived(msg);
				}
			}
			catch (Exception exception)
			{
				this.logger.Error("Parser#Parse", exception);
				if (this.OnParseError != null)
				{
					this.OnParseError(string.Format("{0}: {1}", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ParseError), exception.Message));
				}
			}
		}

		public void QueueEnque(SECSBlock aHSMSItem)
		{
			lock (this.syncObject)
			{
				this.mQueueConvert.Enqueue(aHSMSItem);
			}
		}

		protected override void Run()
		{
			while (this.running)
			{
				try
				{
					if (this.mQueueConvert.Count >= 1)
					{
						SECSBlock mHSMSItem = null;
						lock (this.syncObject)
						{
							mHSMSItem = this.mQueueConvert.Dequeue();
						}
						if (mHSMSItem != null)
						{
							this.Parse(mHSMSItem);
						}
						else
						{
							this.logger.Debug("Parser#Run Invoked. block ==null");
						}
						continue;
					}
				}
				catch (Exception exception)
				{
					this.logger.Error("Parser#Run", exception);
					if (this.OnParseError != null)
					{
						this.OnParseError(string.Format("{0}: {1}", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ParseError), exception.Message));
					}
				}
				Thread.Sleep(50);
			}
		}
	}
}
