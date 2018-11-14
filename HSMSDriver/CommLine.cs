using System;
using System.IO;
using System.Text;
using System.Threading;

namespace HSMSDriver
{
	internal abstract class CommLine : CommBase
	{
		internal class CommLineSettings : CommBaseSettings
		{
			public int rxStringBufferSize = 256;

			public ASCII rxTerminator = ASCII.CR;

			public ASCII[] rxFilter;

			public int transactTimeout = 500;

			public ASCII[] txTerminator;

			public new static CommLine.CommLineSettings LoadFromXML(Stream s)
			{
				return (CommLine.CommLineSettings)CommBaseSettings.LoadFromXML(s, typeof(CommLine.CommLineSettings));
			}
		}

		private byte[] RxBuffer;

		private uint RxBufferP;

		private ASCII RxTerm;

		private ASCII[] TxTerm;

		private ASCII[] RxFilter;

		private string RxString = "";

		private ManualResetEvent TransFlag = new ManualResetEvent(true);

		private uint TransTimeout;

		protected void Send(string toSend)
		{
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			uint num = (uint)aSCIIEncoding.GetByteCount(toSend);
			if (this.TxTerm != null)
			{
				num += (uint)this.TxTerm.GetLength(0);
			}
			byte[] array = new byte[num];
			byte[] bytes = aSCIIEncoding.GetBytes(toSend);
			int i;
			for (i = 0; i <= bytes.GetUpperBound(0); i++)
			{
				array[i] = bytes[i];
			}
			if (this.TxTerm != null)
			{
				int j = 0;
				while (j <= this.TxTerm.GetUpperBound(0))
				{
					array[i] = (byte)this.TxTerm[j];
					j++;
					i++;
				}
			}
			base.Send(array);
		}

		protected string Transact(string toSend)
		{
			this.Send(toSend);
			this.TransFlag.Reset();
			if (!this.TransFlag.WaitOne((int)this.TransTimeout, false))
			{
				base.ThrowException("Timeout");
			}
			string rxString2;
			lock (this.RxString)
			{
				rxString2 = this.RxString;
			}
			return rxString2;
		}

		protected void Setup(CommLine.CommLineSettings s)
		{
			this.RxBuffer = new byte[s.rxStringBufferSize];
			this.RxTerm = s.rxTerminator;
			this.RxFilter = s.rxFilter;
			this.TransTimeout = (uint)s.transactTimeout;
			this.TxTerm = s.txTerminator;
		}

		protected virtual void OnRxLine(string s)
		{
		}

		protected override void OnRxChar(byte ch)
		{
			if (ch != (byte)this.RxTerm && (ulong)this.RxBufferP <= (ulong)((long)this.RxBuffer.GetUpperBound(0)))
			{
				bool flag = true;
				if (this.RxFilter != null)
				{
					for (int i = 0; i <= this.RxFilter.GetUpperBound(0); i++)
					{
						if (this.RxFilter[i] == (ASCII)ch)
						{
							flag = false;
						}
					}
				}
				if (flag)
				{
					this.RxBuffer[(int)((uint)((UIntPtr)this.RxBufferP))] = ch;
					this.RxBufferP += 1u;
				}
				return;
			}
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			lock (this.RxString)
			{
				this.RxString = aSCIIEncoding.GetString(this.RxBuffer, 0, (int)this.RxBufferP);
			}
			this.RxBufferP = 0u;
			if (this.TransFlag.WaitOne(0, false))
			{
				this.OnRxLine(this.RxString);
				return;
			}
			this.TransFlag.Set();
		}
	}
}
