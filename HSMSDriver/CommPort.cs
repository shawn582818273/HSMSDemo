using System;

namespace HSMSDriver
{
	internal class CommPort : CommBase
	{
		public delegate void DataReceivedHandler(byte data);

		private bool Immediate;

		private CommPortSettings settings = new CommPortSettings();

		public event CommPort.DataReceivedHandler DataReceived;

		public CommBaseSettings CommSetting
		{
			get
			{
				return this.settings;
			}
		}

		protected override bool AfterOpen()
		{
			base.GetModemStatus();
			return true;
		}

		protected override void BeforeClose(bool e)
		{
			bool arg_0B_0 = this.settings.autoReopen;
		}

		protected override CommBaseSettings CommSettings()
		{
			return this.settings;
		}

		protected override void OnBreak()
		{
		}

		protected override void OnRxChar(byte c)
		{
			if (this.DataReceived != null)
			{
				this.DataReceived(c);
			}
		}

		protected override void OnStatusChange(ModemStatus c, ModemStatus v)
		{
		}

		public void SendByte(byte c)
		{
			try
			{
				if (this.Immediate)
				{
					base.SendImmediate(c);
				}
				else
				{
					base.Send(c);
				}
			}
			catch (CommPortException)
			{
				throw;
			}
		}

		public void SendByte(byte[] data)
		{
			base.Send(data);
		}

		public bool SendCtrl(string s)
		{
			ASCII nULL = ASCII.NULL;
			try
			{
				nULL = (ASCII)Enum.Parse(nULL.GetType(), s, true);
			}
			catch
			{
				return false;
			}
			this.SendByte((byte)nULL);
			return true;
		}
	}
}
