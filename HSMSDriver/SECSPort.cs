using log4net;
using System;
using System.Xml.Linq;

namespace HSMSDriver
{
	public sealed class SECSPort
	{
		public delegate void SECSEventHandler(SECSPort secsPort, SECSEventType type, SECSTransaction trans, SECSErrors err, string errmsg);

		private bool autoDevice = true;

		private short deviceId;

		private HSMSParameters hsmsPara;

		private readonly HSMSPort hsmsPort;

		private bool isConnected;

		private bool isOpen;

		private readonly SECSLogConfigure logConfig = new SECSLogConfigure();

		private ILog logger;

		private string name;

		private eSECS_PORT_TYPE porttype = eSECS_PORT_TYPE.HSMS;

		private SECSS9FxMonitor s9FxMonitor;

		private SECS1Parameters secs1Para;

		private readonly SECS1Port secs1Port;

		private SECSLibrary secslib = new SECSLibrary();

		public event SECSPort.SECSEventHandler OnSECSEvent;

		internal bool AutoDevice
		{
			get
			{
				return this.autoDevice;
			}
			set
			{
				this.autoDevice = value;
			}
		}

		public bool Connected
		{
			get
			{
				return this.isConnected;
			}
		}

		public short DeviceID
		{
			get
			{
				return this.deviceId;
			}
			set
			{
				if (value < 0 || value > 32767)
				{
					throw new ArgumentOutOfRangeException("value", "Device ID Must Between 0 and 32767!");
				}
				this.deviceId = value;
			}
		}

		public HSMSParameters HsmsParameters
		{
			get
			{
				return this.hsmsPara;
			}
			set
			{
				this.hsmsPara = value;
			}
		}

		public SECSLibrary Library
		{
			get
			{
				return this.secslib;
			}
			set
			{
				this.secslib = value;
			}
		}

		public eLOG_LEVEL LogLevel
		{
			get
			{
				return this.logConfig.LogLevel;
			}
			set
			{
				this.logConfig.LogLevel = value;
			}
		}

		public string LogPath
		{
			get
			{
				return this.logConfig.LogPath;
			}
			set
			{
				this.logConfig.LogPath = value;
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
				if (this.PortIsOpen)
				{
					throw new InvalidOperationException("Port Is Open, Cannot Change the Port Name.");
				}
				this.name = value;
				this.logConfig.Name = value;
			}
		}

		public bool PortIsOpen
		{
			get
			{
				return this.isOpen;
			}
		}

		public eSECS_PORT_TYPE PortType
		{
			get
			{
				return this.porttype;
			}
			set
			{
				this.porttype = value;
			}
		}

		public SECS1Parameters Secs1Parameters
		{
			get
			{
				return this.secs1Para;
			}
			set
			{
				this.secs1Para = value;
			}
		}

		public SECSPort(string name)
		{
			this.Name = name;
			if (string.IsNullOrEmpty(this.name))
			{
				this.name = "SECSPORT" + SECSUtility.Now();
			}
			this.secs1Para = new SECS1Parameters();
			this.hsmsPara = new HSMSParameters();
			this.hsmsPort = new HSMSPort();
			this.secs1Port = new SECS1Port();
		}

		internal void CallSECSEvent(SECSEventType type, SECSTransaction trans, SECSErrors err, string errmsg)
		{
			if (type == SECSEventType.Error || type == SECSEventType.Warn)
			{
				this.logger.Error(string.Format("SystemBytes: {0}, ERRCODE: {1}, ERRDESC: {2}.", (trans != null) ? trans.SystemBytes.ToString() : "null", err, errmsg));
			}
			if (this.OnSECSEvent != null)
			{
				this.OnSECSEvent(this, type, trans, err, errmsg);
			}
		}

		public void ClosePort()
		{
			this.logger.Debug(string.Format("Start to Close the {0} SECS Port", this.Name));
			if (this.porttype == eSECS_PORT_TYPE.HSMS)
			{
				this.hsmsPort.OnHSMSEvent -= new HSMSPort.HSMSEventHandler(this.HsmsPort_OnHSMSEvent);
				this.hsmsPort.Terminate();
				this.isOpen = false;
			}
			else
			{
				this.secs1Port.OnSECS1Event -= new SECS1Port.SECS1EventHandler(this.Secs1Port_OnSECS1Event);
				this.secs1Port.Terminate();
			}
			if (this.s9FxMonitor != null)
			{
				this.s9FxMonitor.Stop();
				this.s9FxMonitor = null;
			}
			this.logger.Debug(string.Format("Completely Close the {0} SECS Port", this.Name));
		}

		private void HsmsPort_OnHSMSEvent(SECSEventType eventtype, SECSTransaction trans, SECSErrors err, string errmsg)
		{
			if (eventtype == SECSEventType.HSMSConnected)
			{
				this.isConnected = true;
			}
			else if (eventtype == SECSEventType.HSMSDisconnected)
			{
				this.isConnected = false;
			}
			if (this.s9FxMonitor.PreHandleSECSEvent(eventtype, trans, err))
			{
				this.CallSECSEvent(eventtype, trans, err, errmsg);
			}
		}

		public void OpenPort()
		{
			SECSLogManager.Instance.CreateNewLogger(this.logConfig);
			this.logger = LogManager.GetLogger("SECSwell", this.Name);
			this.logger.Debug(string.Format("Start to Open the {0} SECS Port", this.Name));
			SECSS9FxMonitor monitor = new SECSS9FxMonitor(this)
			{
				Logger = this.logger
			};
			this.s9FxMonitor = monitor;
			this.s9FxMonitor.Start();
			if (this.porttype == eSECS_PORT_TYPE.HSMS)
			{
				this.hsmsPort.Name = this.Name;
				this.hsmsPara.DeviceID = this.deviceId;
				this.hsmsPort.HsmsPara = this.hsmsPara;
				this.hsmsPort.OnHSMSEvent += new HSMSPort.HSMSEventHandler(this.HsmsPort_OnHSMSEvent);
				this.hsmsPort.Logger = this.logger;
				this.hsmsPort.Initialize();
				this.isOpen = true;
			}
			else
			{
				this.secs1Port.Name = this.Name;
				this.secs1Para.DeviceID = this.deviceId;
				this.secs1Port.SECS1Para = this.secs1Para;
				this.secs1Port.OnSECS1Event += new SECS1Port.SECS1EventHandler(this.Secs1Port_OnSECS1Event);
				this.secs1Port.Logger = this.logger;
				this.secs1Port.Initialize();
			}
			this.logger.Debug(string.Format("Completely Open the {0} SECS Port", this.Name));
		}

		public void Reply(SECSTransaction trans)
		{
			if (trans.Primary != null)
			{
				trans.Primary.Transaction = trans;
			}
			if (trans.Secondary != null)
			{
				trans.Secondary.Transaction = trans;
			}
			if (this.porttype == eSECS_PORT_TYPE.HSMS)
			{
				if (trans.Secondary != null)
				{
					if (trans.Primary != null)
					{
						trans.Secondary.SystemBytes = trans.Primary.SystemBytes;
					}
					if (!this.PortIsOpen)
					{
						this.CallSECSEvent(SECSEventType.SecondarySent, trans, SECSErrors.PortNotOpen, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.PortNotOpen));
						return;
					}
					if (!this.Connected)
					{
						this.CallSECSEvent(SECSEventType.SecondarySent, trans, SECSErrors.PortNotConnected, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.PortNotConnected));
						return;
					}
					this.hsmsPort.ReplyMessage(trans.Secondary);
					return;
				}
			}
			else if (trans.Secondary != null)
			{
				if (trans.Primary != null)
				{
					trans.Secondary.SystemBytes = trans.Primary.SystemBytes;
				}
				this.secs1Port.SendMessage(trans.Secondary);
			}
		}

		public void ReplyAsync(SECSTransaction t, XElement xmlSecondaryMessage)
		{
			SECSMessage message = new SECSMessage
			{
				Stream = t.Primary.Stream,
				Function = t.Primary.Function + 1,
				SystemBytes = t.Primary.SystemBytes,
				DeviceIdID = t.DeviceID,
				Root = this.Library.XElementToSECSItem(xmlSecondaryMessage)
			};
			t.Secondary = message;
			this.Reply(t);
		}

		private void Secs1Port_OnSECS1Event(SECSEventType eventtype, SECSTransaction trans, SECSErrors err, string errmsg)
		{
			if (this.s9FxMonitor.PreHandleSECSEvent(eventtype, trans, err))
			{
				this.CallSECSEvent(eventtype, trans, err, errmsg);
			}
		}

		public void Send(SECSTransaction trans)
		{
			if (trans.Primary != null)
			{
				trans.Primary.Transaction = trans;
			}
			if (trans.Secondary != null)
			{
				trans.Secondary.Transaction = trans;
			}
			if (this.porttype != eSECS_PORT_TYPE.HSMS)
			{
				this.secs1Port.SendMessage(trans.Primary);
				return;
			}
			if (!this.PortIsOpen)
			{
				this.CallSECSEvent(SECSEventType.PrimarySent, trans, SECSErrors.PortNotOpen, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.PortNotOpen));
				return;
			}
			if (!this.Connected)
			{
				this.CallSECSEvent(SECSEventType.PrimarySent, trans, SECSErrors.PortNotConnected, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.PortNotConnected));
				return;
			}
			this.hsmsPort.SendMessages(trans.Primary);
		}

		public void SendAsync(string transName, XElement xmlPrimaryMessage)
		{
			SECSTransaction trans = this.Library.FindTransaction(transName);
			if (trans == null)
			{
				throw new ArgumentException(string.Format("Transaction {0} Not Found in AbstractSECSLibrary.", transName));
			}
			trans.Name = transName;
			trans.Primary.Root = null;
			trans.Primary.Root = this.Library.XElementToSECSItem(xmlPrimaryMessage);
			trans.Secondary = null;
			this.Send(trans);
		}
	}
}
