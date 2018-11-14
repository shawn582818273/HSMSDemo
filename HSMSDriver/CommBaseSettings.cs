using System;
using System.IO;
using System.Xml.Serialization;

namespace HSMSDriver
{
	internal class CommBaseSettings
	{
		public bool autoReopen;

		public int baudRate = 2400;

		public bool checkAllSends = true;

		public int dataBits = 8;

		public Parity parity;

		public string port = "COM1:";

		public bool rxFlowX;

		public bool rxGateDSR;

		public int rxHighWater = 2048;

		public int rxLowWater = 512;

		public int rxQueue;

		public int sendTimeoutConstant;

		public int sendTimeoutMultiplier;

		public StopBits stopBits;

		public bool txFlowCTS;

		public bool txFlowDSR;

		public bool txFlowX;

		public int txQueue;

		public bool txWhenRxXoff = true;

		public HSOutput useDTR;

		public HSOutput useRTS;

		public ASCII XoffChar = ASCII.DC3;

		public ASCII XonChar = ASCII.DC1;

		public static CommBaseSettings LoadFromXML(Stream s)
		{
			return CommBaseSettings.LoadFromXML(s, typeof(CommBaseSettings));
		}

		protected static CommBaseSettings LoadFromXML(Stream s, Type t)
		{
			XmlSerializer serializer = new XmlSerializer(t);
			CommBaseSettings result;
			try
			{
				result = (CommBaseSettings)serializer.Deserialize(s);
			}
			catch
			{
				result = null;
			}
			return result;
		}

		public void SaveAsXML(Stream s)
		{
			new XmlSerializer(base.GetType()).Serialize(s, this);
		}

		public void SetStandard(string Port, int Baud, Handshake Hs)
		{
			this.dataBits = 8;
			this.stopBits = StopBits.one;
			this.parity = Parity.none;
			this.port = Port;
			this.baudRate = Baud;
			switch (Hs)
			{
			case Handshake.none:
				this.txFlowCTS = false;
				this.txFlowDSR = false;
				this.txFlowX = false;
				this.rxFlowX = false;
				this.useRTS = HSOutput.online;
				this.useDTR = HSOutput.online;
				this.txWhenRxXoff = true;
				this.rxGateDSR = false;
				return;
			case Handshake.XonXoff:
				this.txFlowCTS = false;
				this.txFlowDSR = false;
				this.txFlowX = true;
				this.rxFlowX = true;
				this.useRTS = HSOutput.online;
				this.useDTR = HSOutput.online;
				this.txWhenRxXoff = true;
				this.rxGateDSR = false;
				this.XonChar = ASCII.DC1;
				this.XoffChar = ASCII.DC3;
				return;
			case Handshake.CtsRts:
				this.txFlowCTS = true;
				this.txFlowDSR = false;
				this.txFlowX = false;
				this.rxFlowX = false;
				this.useRTS = HSOutput.handshake;
				this.useDTR = HSOutput.online;
				this.txWhenRxXoff = true;
				this.rxGateDSR = false;
				return;
			case Handshake.DsrDtr:
				this.txFlowCTS = false;
				this.txFlowDSR = true;
				this.txFlowX = false;
				this.rxFlowX = false;
				this.useRTS = HSOutput.online;
				this.useDTR = HSOutput.handshake;
				this.txWhenRxXoff = true;
				this.rxGateDSR = false;
				return;
			default:
				return;
			}
		}
	}
}
