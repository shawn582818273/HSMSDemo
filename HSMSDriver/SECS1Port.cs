using log4net;
using System;
using System.Threading;

namespace HSMSDriver
{
	internal class SECS1Port
	{
		internal delegate void SECS1EventHandler(SECSEventType eventtype, SECSTransaction trans, SECSErrors err, string errmsg);

		private CommPort commport;

		private SECS1EventExecutor eventExecutor;

		private eSECS1_PORT_STATUS portstatus;

		private SECS1Reader reader;

		private object syncObject = new object();

		private SystemBytes sysbytes = new SystemBytes();

		private SECS1Timer timer;

		private SECS1Writer writer;

		internal event SECS1Port.SECS1EventHandler OnSECS1Event;

		internal ILog Logger
		{
			get;
			set;
		}

		internal string Name
		{
			get;
			set;
		}

		internal eSECS1_PORT_STATUS PortStatus
		{
			get
			{
				return this.portstatus;
			}
		}

		internal SECS1Parameters SECS1Para
		{
			get;
			set;
		}

		private void DataReceived(byte bytedata)
		{
			this.reader.DataReceived(bytedata);
		}

		private void eventExecutor_OnSECS1Event(SECSEventType eventtype, SECSTransaction trans, SECSErrors err, string errmsg)
		{
			if (this.OnSECS1Event != null)
			{
				this.OnSECS1Event(eventtype, trans, err, errmsg);
			}
		}

		private void HandleT1Timeout()
		{
			this.StopTimer(eTimeout.T1);
			if (this.portstatus == eSECS1_PORT_STATUS.PortCmpl || this.portstatus == eSECS1_PORT_STATUS.PortRcvd)
			{
				this.reader.Clear();
				this.SendNAK();
				this.UpdatePortStatus(eSECS1_PORT_STATUS.PortCmpl, eSECS1_PORT_STATUS.PortIdle);
				this.Logger.Error("T1 Timeout when waiting for the length bytes.");
				this.eventExecutor.NotifyERROR(SECSErrors.T1TimeOut, null);
				return;
			}
			this.Logger.Debug("T1 Timeout, but invalid PortStatus.");
		}

		private void HandleT2Timeout()
		{
			this.StopTimer(eTimeout.T2);
			if (this.portstatus == eSECS1_PORT_STATUS.PortCtrl || this.portstatus == eSECS1_PORT_STATUS.PortCmpl)
			{
				if (this.writer.RetryCount <= 0)
				{
					while (this.writer.SendBlocks.Count > 0)
					{
						this.writer.SendBlocks.Dequeue();
					}
					if (this.portstatus == eSECS1_PORT_STATUS.PortCtrl)
					{
						this.Logger.Error("T2 Timeout when waiting for EQT Handshake signal.");
					}
					else
					{
						this.Logger.Error("T2 Timeout when waiting for ACK or NAK signal.");
					}
					this.UpdatePortStatus(this.portstatus, eSECS1_PORT_STATUS.PortCmpl);
					Thread.Sleep(50);
					this.UpdatePortStatus(eSECS1_PORT_STATUS.PortCmpl, eSECS1_PORT_STATUS.PortIdle);
					this.eventExecutor.NotifyERROR(SECSErrors.T2TimeOut, this.writer.SendMsg);
					this.writer.SendMsg = null;
				}
				else
				{
					if (this.portstatus == eSECS1_PORT_STATUS.PortCmpl)
					{
						this.UpdatePortStatus(eSECS1_PORT_STATUS.PortCmpl, eSECS1_PORT_STATUS.PortCtrl);
					}
					this.SendENQ();
					this.Logger.Debug("T2 Timeout, Retry to Send ENQ");
					this.writer.RetryCount = this.writer.RetryCount - 1;
				}
			}
			if (this.portstatus == eSECS1_PORT_STATUS.PortRcvd)
			{
				this.reader.Clear();
				this.UpdatePortStatus(eSECS1_PORT_STATUS.PortRcvd, eSECS1_PORT_STATUS.PortCmpl);
				this.SendNAK();
				this.UpdatePortStatus(eSECS1_PORT_STATUS.PortCmpl, eSECS1_PORT_STATUS.PortIdle);
				this.Logger.Error("T2 Timeout when waiting for the length bytes.");
				this.eventExecutor.NotifyERROR(SECSErrors.T2TimeOut, null);
			}
		}

		private void HandleT3Timeout(TimerPara p)
		{
			if (p.Msg != null)
			{
				this.StopTimer(p.Msg);
			}
			this.Logger.Debug("T3 Timeout when waiting for the reply message.");
			this.eventExecutor.NotifyEVENT(SECSEventType.SecondaryRcvd, p.Msg.Transaction, SECSErrors.T3TimeOut);
		}

		private void HandleT4Timeout()
		{
			this.reader.ClearAll();
			this.Logger.Debug(string.Format("PortStatus={0}, Rcvd Multi Block Message, but T4 Timeout.", this.portstatus));
			this.Logger.Error("T4 Timeout when receive multi block message.");
			this.eventExecutor.NotifyERROR(SECSErrors.T4TimeOut, null);
		}

		internal void Initialize()
		{
			this.timer = new SECS1Timer(this.SECS1Para.T1, this.SECS1Para.T2, (long)this.SECS1Para.T3, (long)this.SECS1Para.T4, this.SECS1Para.SerialPort);
			this.timer.OnSECS1Timeout += new SECS1Timer.OnTimeout(this.OnTimeout);
			this.timer.Logger = this.Logger;
			this.eventExecutor = new SECS1EventExecutor(this.SECS1Para.SerialPort, this);
			this.eventExecutor.OnSECS1Event += new SECS1Port.SECS1EventHandler(this.eventExecutor_OnSECS1Event);
			this.eventExecutor.Logger = this.Logger;
			this.writer = new SECS1Writer(this.SECS1Para.SerialPort, this);
			this.writer.Logger = this.Logger;
			this.reader = new SECS1Reader(this.SECS1Para.SerialPort, this, this.writer, this.eventExecutor);
			this.reader.Logger = this.Logger;
			this.commport = new CommPort();
			this.commport.CommSetting.baudRate = this.SECS1Para.Baudrate;
			this.commport.CommSetting.port = this.SECS1Para.SerialPort;
			this.commport.DataReceived += new CommPort.DataReceivedHandler(this.DataReceived);
			this.portstatus = eSECS1_PORT_STATUS.PortIdle;
			this.writer.Start();
			this.timer.Start();
			this.eventExecutor.Start();
			this.commport.Open();
		}

		private void OnTimeout(TimerPara p)
		{
			if (p.Type == eTimeout.T2)
			{
				this.HandleT2Timeout();
				return;
			}
			if (p.Type == eTimeout.T3)
			{
				this.HandleT3Timeout(p);
				return;
			}
			if (p.Type == eTimeout.T1)
			{
				this.HandleT1Timeout();
				return;
			}
			this.HandleT4Timeout();
		}

        internal void SendACK()
        {
            this.commport.SendByte(6);
            this.Logger.Info("[SEND] (ACK) 06\n");
        }

        internal void SendBody()
		{
			SECSMessage sendMsg = this.writer.SendMsg;
			this.commport.SendByte((byte)this.writer.SendBlock.Length);
			this.commport.SendByte(this.writer.SendBlock.Header);
			if (this.writer.SendBlock.DataItem.Length > 0)
			{
				this.commport.SendByte(this.writer.SendBlock.DataItem);
			}
			this.commport.SendByte(this.writer.SendBlock.CheckSum);
			//this.Logger.Info(string.Format("[SEND] S{0}F{1} {2} SystemBytes = {3} Length = {4} HDR = {5}\nDATA = {6}\nCSUM = {7}", new object[]
			//{
			//	sendMsg.Stream,
			//	sendMsg.Function,
			//	sendMsg.WBit ? "W" : "",
			//	sendMsg.SystemBytes,
			//	this.writer.SendBlock.Length,
			//	ByteStringBuilder.ToLogString(this.writer.SendBlock.Header),
			//	ByteStringBuilder.ToLogString(this.writer.SendBlock.DataItem),
			//	ByteStringBuilder.ToLogString(this.writer.SendBlock.CheckSum)
			//}));
		}

		internal void SendENQ()
		{
			this.commport.SendByte(5);
			this.Logger.Info("[SEND] (ENQ) 05");
			this.StartTimer(eTimeout.T2);
		}

		internal void SendEOT()
		{
			this.commport.SendByte(4);
			this.Logger.Info("[SEND] (EOT) 04");
			this.StartTimer(eTimeout.T2);
		}

		internal void SendMessage(SECSMessage msg)
		{
			msg.DeviceIdID = this.SECS1Para.DeviceID;
			msg.SystemBytes = this.sysbytes.GetSystemBytes();
			this.writer.Enqueue(msg);
		}

		internal void SendNAK()
		{
			this.commport.SendByte(21);
			this.Logger.Info("[SEND] (NAK) 15\n");
		}

		internal void StartTimer(eTimeout e)
		{
			if (e == eTimeout.T1)
			{
				this.timer.StartT1Timer();
				return;
			}
			if (e == eTimeout.T2)
			{
				this.timer.StartT2Timer();
				return;
			}
			if (e != eTimeout.T4)
			{
				throw new ArgumentException(string.Format("Only Support T1, T2, T4 Timer, but now Timer is {0}", e));
			}
			this.timer.StartT4Timer();
		}

		internal void StartTimer(SECSMessage msg)
		{
			this.timer.StartT3Timer(msg);
		}

		internal void StopTimer(eTimeout e)
		{
			if (e == eTimeout.T1)
			{
				this.timer.StopT1Timer();
				return;
			}
			if (e == eTimeout.T2)
			{
				this.timer.StopT2Timer();
				return;
			}
			if (e != eTimeout.T4)
			{
				throw new ArgumentException(string.Format("Only Support T1, T2, T4 Timer, but now Timer is {0}", e));
			}
			this.timer.StopT4Timer();
		}

		internal void StopTimer(SECSMessage msg)
		{
			this.timer.StopT3Timer(msg);
		}

		internal void Terminate()
		{
			this.commport.DataReceived -= new CommPort.DataReceivedHandler(this.DataReceived);
			this.timer.OnSECS1Timeout -= new SECS1Timer.OnTimeout(this.OnTimeout);
			this.eventExecutor.OnSECS1Event -= new SECS1Port.SECS1EventHandler(this.eventExecutor_OnSECS1Event);
			this.writer.Stop();
			this.timer.Stop();
			this.eventExecutor.Stop();
			this.commport.Close();
		}

		internal int UpdatePortStatus(eSECS1_PORT_STATUS ef, eSECS1_PORT_STATUS et)
		{
			int result;
			lock (this.syncObject)
			{
				if (this.portstatus == ef)
				{
					this.Logger.Debug(string.Format("Update Port Status from {0} to {1}", ef, et));
					this.portstatus = et;
					result = 0;
				}
				else
				{
					this.Logger.Debug(string.Format("Update Port Status from {0} to {1}, but now is: {2}", ef, et, this.portstatus));
					result = 1;
				}
			}
			return result;
		}
	}
}
