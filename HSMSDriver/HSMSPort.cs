using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace HSMSDriver
{
    internal class HSMSPort
    {
        public delegate void HSMSEventHandler(SECSEventType eventtype, SECSTransaction trans, SECSErrors err, string errmsg);

        private eHSMS_PORT_STATUS _nowStatus;

        private eHSMS_PORT_STATUS _preStatus;

        private ILog logger;

        private long mCtrlSystemBytes;

        private SECSConnector mHsmsConnector;

        private HSMSParser mHsmsParser;

        private HSMSReader mHsmsReader;

        private HSMSTimer mHsmsTimer;

        private HSMSWriter mHsmsWriter;

        private Dictionary<long, SECSMessage> mNeedReplyMsg;

        private TcpClient mSECSSocket;

        private long mSystemBytes;

        private string name;

        private HSMSParameters portCfg;

        private BinaryReader reader;

        private SocketInfo socketinfo;

        private object syncNeedReplyObject;

        private object syncObject;

        private object syncSystemBytes;

        private BinaryWriter writer;

        public event HSMSPort.HSMSEventHandler OnHSMSEvent;

        public HSMSParameters HsmsPara
        {
            get
            {
                return this.portCfg;
            }
            set
            {
                this.portCfg = value;
            }
        }

        internal ILog Logger
        {
            get
            {
                return this.logger;
            }
            set
            {
                this.logger = value;
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

        public eHSMS_PORT_STATUS NowStatus
        {
            get
            {
                return this._nowStatus;
            }
        }

        public eHSMS_PORT_STATUS PreStatus
        {
            get
            {
                return this._preStatus;
            }
        }

        public HSMSPort()
        {
            this.syncObject = new object();
            this.syncNeedReplyObject = new object();
            this.mNeedReplyMsg = new Dictionary<long, SECSMessage>();
            this.name = "";
            this.syncSystemBytes = new object();
        }

        public HSMSPort(HSMSParameters aItemCfg, string name)
        {
            this.syncObject = new object();
            this.syncNeedReplyObject = new object();
            this.mNeedReplyMsg = new Dictionary<long, SECSMessage>();
            this.name = "";
            this.syncSystemBytes = new object();
            this.portCfg = aItemCfg;
            this.name = name;
        }

        private void CreateObject()
        {
            this.socketinfo = new SocketInfo();
            if (this.portCfg.ConnectionMode == eHSMS_CONNECT_MODE.ACTIVE)
            {
                this.socketinfo.ConnectMode = CONNECT_MODE.ACTIVE;
                this.socketinfo.IpAddress = this.portCfg.RemoteIP;
                this.socketinfo.Port = this.portCfg.RemotePort.ToString();
            }
            else
            {
                this.socketinfo.ConnectMode = CONNECT_MODE.PASSIVE;
                this.socketinfo.IpAddress = this.portCfg.LocalIP;
                this.socketinfo.Port = this.portCfg.LocalPort.ToString();
            }
            this.StartHsmsConnector();
            this.StartHsmsParser();
            this.StartHsmsTimer();
        }

        private void FireSelected()
        {
            this.UpdateStatus(eHSMS_PORT_STATUS.SELECT);
        }

        private void FireSelected(byte aSelectStatus)
        {
            switch (aSelectStatus)
            {
                case 0:
                    this.FireSelected();
                    break;
                case 1:
                case 2:
                case 3:
                    break;
                default:
                    return;
            }
        }

        private long GetCtrlSystemBytes()
        {
            if (this.mCtrlSystemBytes < 1L)
            {
                this.mCtrlSystemBytes = 2130706432L;
            }
            long num;
            this.mCtrlSystemBytes = (num = this.mCtrlSystemBytes) + 1L;
            return num;
        }

        private void HandleParseError(string errmsg)
        {
            if (this.OnHSMSEvent != null)
            {
                this.OnHSMSEvent(SECSEventType.Error, null, SECSErrors.ParseError, errmsg);
            }
        }

        private void HandleRcvdAbortMessage(SECSMessage sent, SECSMessage rcvd)
        {
            SECSTransaction trans = sent.Transaction;
            trans.Secondary = rcvd;
            if (this.OnHSMSEvent != null)
            {
                this.OnHSMSEvent(SECSEventType.Error, trans, SECSErrors.RcvdAbortMessage, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.RcvdAbortMessage));
            }
        }

        private void HandleRcvdPrimaryMessage(SECSMessage rcvd)
        {
            if (this.OnHSMSEvent != null)
            {
                SECSTransaction trans = new SECSTransaction
                {
                    Primary = rcvd,
                    SystemBytes = rcvd.SystemBytes,
                    Secondary = null,
                    ExpectReply = rcvd.WBit,
                    DeviceID = rcvd.DeviceIdID
                };
                this.OnHSMSEvent(SECSEventType.PrimaryRcvd, trans, SECSErrors.None, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.None));
            }
        }

        private void HandleRcvdSecondaryMessage(SECSMessage rcvd, SECSMessage sent)
        {
            if (this.OnHSMSEvent != null)
            {
                SECSTransaction trans = sent.Transaction;
                trans.Secondary = rcvd;
                this.OnHSMSEvent(SECSEventType.SecondaryRcvd, trans, SECSErrors.None, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.None));
            }
        }

        private void HandleRcvdUnknownMessage(SECSMessage rcvd)
        {
            SECSTransaction trans = new SECSTransaction
            {
                Primary = null,
                Secondary = rcvd
            };
            if (this.OnHSMSEvent != null)
            {
                this.OnHSMSEvent(SECSEventType.Error, trans, SECSErrors.RcvdUnknownMessage, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.RcvdUnknownMessage));
            }
        }

        private void HandleReadError(string errmsg)
        {
            if (this.OnHSMSEvent != null)
            {
                this.OnHSMSEvent(SECSEventType.Error, null, SECSErrors.ReadError, errmsg);
            }
        }

        private void HandleWriteError(SECSEventType type, SECSTransaction t, string errmsg)
        {
            if (this.OnHSMSEvent != null)
            {
                this.OnHSMSEvent(SECSEventType.Error, t, SECSErrors.WriteError, string.Format("{0} {1}", type.ToString(), errmsg));
            }
        }

        public void Initialize()
        {
            try
            {
                this.logger.Debug("HSMSPort::Initialize execute.");
                lock (this.syncObject)
                {
                    this.InitializeParameters();
                    this.CreateObject();
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("HSMSPort::Initialize", exception);
            }
        }

        private void InitializeParameters()
        {
            this.mSystemBytes = 1L;
            this.mCtrlSystemBytes = 2130706432L;
        }

        private void OnConnected(TcpClient client)
        {
            try
            {
                lock (this.syncObject)
                {
                    this.logger.Debug(string.Format("HSMSPort::OnConnected Status={0}", this.NowStatus));
                    this.UpdateStatus(eHSMS_PORT_STATUS.CONNECT);
                    this.mSECSSocket = client;
                    this.mSECSSocket.ReceiveTimeout = 0;
                    this.reader = new BinaryReader(this.mSECSSocket.GetStream());
                    this.writer = new BinaryWriter(this.mSECSSocket.GetStream());
                    this.StartHsmsReader();
                    this.StartHsmsWriter();
                    this.mHsmsTimer.StartLinkTestTimer();
                    this.StopHsmsConnector();
                    if (this.OnHSMSEvent != null)
                    {
                        this.OnHSMSEvent(SECSEventType.HSMSConnected, null, SECSErrors.None, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.None));
                    }
                }
                if (this.portCfg.ConnectionMode == eHSMS_CONNECT_MODE.ACTIVE)
                {
                    long ctrlSystemBytes = this.GetCtrlSystemBytes();
                    this.mHsmsWriter.WriteControlMessage(ctrlSystemBytes, 0, eControlMessage.SELECT_REQ, 255, 255);
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("HSMSPort::OnConnected", exception);
            }
        }

        private void OnDisconnect(string errmsg)
        {
            try
            {
                if (this.OnHSMSEvent != null)
                {
                    this.OnHSMSEvent(SECSEventType.HSMSDisconnected, null, SECSErrors.None, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.None));
                }
                lock (this.syncObject)
                {
                    if (this.NowStatus >= eHSMS_PORT_STATUS.CONNECT)
                    {
                        this.logger.Error("HSMSPort::OnDisconnect Reconnect");
                        this.UpdateStatus(eHSMS_PORT_STATUS.DISCONNECT);
                        this.StopHsmsReader();
                        this.StopHsmsWriter();
                        this.mHsmsTimer.StopLinkTestTimer();
                        this.TerminateSocket();
                        this.StartHsmsConnector();
                    }
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("HSMSPort::OnDisconnect Error. ", exception);
            }
        }

        private void OnReadHsms(SECSBlock aHSMSItem)
        {
            try
            {
                if (this.NowStatus == eHSMS_PORT_STATUS.TERMINATE)
                {
                    this.logger.Error("HSMSPort::OnReadHsms=> Port TERMINATE.");
                    return;
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("HSMSPort::OnReadHsms", exception);
            }
            byte[] aBytes = new byte[4];
            Array.Copy(aHSMSItem.Header, 6, aBytes, 0, aBytes.Length);
            long mSystemBytes = Byte2SecsValue.GetLong(aBytes);
            byte num = aHSMSItem.Header[5];
            if (num == 0)
            {
                this.logger.Debug("HSMSPort::OnReadHsms Not control message.");
                aHSMSItem.IsControlMsg = false;
                this.mHsmsParser.QueueEnque(aHSMSItem);
                return;
            }
            this.logger.Debug("HSMSPort::OnReadHsms control message.");
            lock (this.syncObject)
            {
                aHSMSItem.IsControlMsg = true;
                switch (num)
                {
                    case 1:
                        this.mHsmsTimer.StopT7Timer();
                        if (this.NowStatus != eHSMS_PORT_STATUS.SELECT)
                        {
                            this.mHsmsWriter.WriteControlMessage(mSystemBytes, 0, eControlMessage.SELECT_RSP, 255, 255);
                            this.FireSelected();
                        }
                        else
                        {
                            this.mHsmsWriter.WriteControlMessage(mSystemBytes, 1, eControlMessage.SELECT_RSP, 255, 255);
                        }
                        break;
                    case 2:
                        this.mHsmsTimer.StopT6Timer();
                        this.FireSelected(aHSMSItem.Header[3]);
                        break;
                    case 3:
                        this.UpdateStatus(eHSMS_PORT_STATUS.CONNECT);
                        this.mHsmsWriter.WriteControlMessage(mSystemBytes, 0, eControlMessage.DESELECT_RSP, 255, 255);
                        break;
                    case 5:
                        this.mHsmsWriter.WriteControlMessage(mSystemBytes, 0, eControlMessage.LINKTEST_RSP, 255, 255);
                        break;
                    case 6:
                        this.mHsmsTimer.StopT6Timer();
                        break;
                    case 7:
                        this.TraceRejectReqCode(aHSMSItem.Header[3]);
                        break;
                    case 9:
                        this.OnDisconnect("Separate.");
                        break;
                }
            }
        }

        private void OnReceived(SECSMessage msg)
        {
            try
            {
                if (msg.Function % 2 == 0)
                {
                    SECSMessage message = null;
                    if (this.mNeedReplyMsg.ContainsKey(msg.SystemBytes))
                    {
                        message = this.mNeedReplyMsg[msg.SystemBytes];
                        this.mHsmsTimer.StopT3Timer(message);
                        lock (this.syncNeedReplyObject)
                        {
                            this.mNeedReplyMsg.Remove(msg.SystemBytes);
                        }
                        if (msg.Stream < 1 || msg.Function < 1)
                        {
                            this.HandleRcvdAbortMessage(message, msg);
                        }
                        else
                        {
                            this.HandleRcvdSecondaryMessage(msg, message);
                        }
                    }
                    else
                    {
                        this.HandleRcvdUnknownMessage(msg);
                    }
                }
                else
                {
                    this.HandleRcvdPrimaryMessage(msg);
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("Port::OnReceived Error.", exception);
            }
        }

        private void OnSocketWriteComplete(bool IsReply, SECSMessage msg)
        {
            try
            {
                SECSTransaction trans = msg.Transaction;
                if (this.OnHSMSEvent != null)
                {
                    if (IsReply)
                    {
                        this.OnHSMSEvent(SECSEventType.SecondarySent, trans, SECSErrors.None, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.None));
                    }
                    else
                    {
                        trans.DeviceID = msg.DeviceIdID;
                        trans.SystemBytes = msg.SystemBytes;
                        trans.ExpectReply = msg.WBit;
                        this.OnHSMSEvent(SECSEventType.PrimarySent, trans, SECSErrors.None, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.None));
                    }
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("Port#Sending Error", exception);
            }
        }

        private void OnTimeout(TimerPara aPara)
        {
            try
            {
                if (aPara.Msg != null && this.mNeedReplyMsg.ContainsKey(aPara.Msg.SystemBytes))
                {
                    lock (this.syncNeedReplyObject)
                    {
                        this.mNeedReplyMsg.Remove(aPara.Msg.SystemBytes);
                    }
                }
                eTimeout type = aPara.Type;
                if (type != eTimeout.LinkTest)
                {
                    switch (type)
                    {
                        case eTimeout.T8:
                            if (this.OnHSMSEvent != null)
                            {
                                this.OnHSMSEvent(SECSEventType.Error, null, SECSErrors.T8TimeOut, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.T8TimeOut));
                            }
                            this.OnDisconnect(SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.T8TimeOut));
                            break;
                        case eTimeout.T7:
                            if (this.OnHSMSEvent != null)
                            {
                                this.OnHSMSEvent(SECSEventType.Error, null, SECSErrors.T7TimeOut, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.T7TimeOut));
                            }
                            this.OnDisconnect(SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.T7TimeOut));
                            break;
                        case eTimeout.T6:
                            if (this.OnHSMSEvent != null)
                            {
                                this.OnHSMSEvent(SECSEventType.Error, null, SECSErrors.T6TimeOut, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.T6TimeOut));
                            }
                            this.OnDisconnect(SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.T6TimeOut));
                            break;
                        case eTimeout.T5:
                            if (this.OnHSMSEvent != null)
                            {
                                this.OnHSMSEvent(SECSEventType.Error, null, SECSErrors.T5TimeOut, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.T5TimeOut));
                            }
                            break;
                        case eTimeout.T3:
                            if (this.OnHSMSEvent != null)
                            {
                                SECSTransaction trans = aPara.Msg.Transaction;
                                trans.Secondary = null;
                                this.OnHSMSEvent(SECSEventType.Error, trans, SECSErrors.T3TimeOut, SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.T3TimeOut));
                            }
                            break;
                    }
                }
                else
                {
                    long ctrlSystemBytes = this.GetCtrlSystemBytes();
                    this.mHsmsWriter.WriteControlMessage(ctrlSystemBytes, 0, eControlMessage.LINKTEST_REQ, 255, 255);
                    this.mHsmsTimer.StartLinkTestTimer();
                }
            }
            catch (Exception exception)
            {
                this.logger.Debug("Port#OnTimeout", exception);
            }
        }

        public void ReplyMessage(SECSMessage msg)
        {
            msg.DeviceIdID = this.portCfg.DeviceID;
            if (this.mHsmsWriter != null)
            {
                this.mHsmsWriter.EnqueReply(msg);
            }
        }

        public void SendMessages(SECSMessage msg)
        {
            msg.DeviceIdID = this.portCfg.DeviceID;
            msg.SystemBytes = this.SystemBytesCountUp();
            if (msg.WBit)
            {
                lock (this.syncNeedReplyObject)
                {
                    this.mNeedReplyMsg.Add(msg.SystemBytes, msg);
                }
            }
            if (this.mHsmsWriter != null)
            {
                this.mHsmsWriter.EnqueSend(msg);
            }
        }

        private void StartHsmsConnector()
        {
            this.mHsmsConnector = new SECSConnector(this.name, this.socketinfo);
            this.mHsmsConnector.OnConnected += new SocketEventHandler.ConnectEventHandler(this.OnConnected);
            this.mHsmsConnector.OnReadError += new SocketEvent.ReadErrorEventHandler(this.HandleReadError);
            this.mHsmsConnector.Logger = this.Logger;
            this.mHsmsConnector.Start();
        }

        private void StartHsmsParser()
        {
            this.mHsmsParser = new HSMSParser(this.name);
            this.mHsmsParser.OnReceived += new SocketEvent.OnReceived(this.OnReceived);
            this.mHsmsParser.OnParseError += new SocketEvent.ParseError(this.HandleParseError);
            this.mHsmsParser.Logger = this.logger;
            this.mHsmsParser.Start();
        }

        private void StartHsmsReader()
        {
            this.mHsmsReader = new HSMSReader(this.reader, this.mHsmsTimer, this.name);
            this.mHsmsReader.OnDisconnected += new SocketEvent.DisconnectEventHandler(this.OnDisconnect);
            this.mHsmsReader.OnReadCompleted += new SocketEvent.ReadCompleteEventHandler(this.OnReadHsms);
            this.mHsmsReader.OnReadError += new SocketEvent.ReadErrorEventHandler(this.HandleReadError);
            this.mHsmsReader.Logger = this.Logger;
            this.mHsmsReader.Start();
        }

        private void StartHsmsTimer()
        {
            this.mHsmsTimer = new HSMSTimer((long)this.portCfg.T3, (long)this.portCfg.T6, (long)this.portCfg.T7, (long)this.portCfg.T8, (long)this.portCfg.LinkTest, this.name);
            this.mHsmsTimer.OnHsmsTimeout += new SocketEvent.OnTimeout(this.OnTimeout);
            this.mHsmsTimer.Logger = this.logger;
            this.mHsmsTimer.Start();
        }

        private void StartHsmsWriter()
        {
            this.mHsmsWriter = new HSMSWriter(this.mHsmsTimer, this.writer, this.Name);
            this.mHsmsWriter.OnDisconnected += new SocketEvent.DisconnectEventHandler(this.OnDisconnect);
            this.mHsmsWriter.OnWriteCompleted += new SocketEvent.WriteCompleteEventHandler(this.OnSocketWriteComplete);
            this.mHsmsWriter.OnWriteError += new SocketEvent.WriteErrorEventHandler(this.HandleWriteError);
            this.mHsmsWriter.Logger = this.logger;
            this.mHsmsWriter.Start();
        }

        private void StopHsmsConnector()
        {
            if (this.mHsmsConnector != null)
            {
                this.mHsmsConnector.Stop();
                this.mHsmsConnector.OnConnected -= new SocketEventHandler.ConnectEventHandler(this.OnConnected);
                this.mHsmsConnector.OnReadError -= new SocketEvent.ReadErrorEventHandler(this.HandleReadError);
                this.mHsmsConnector = null;
            }
        }

        private void StopHsmsParser()
        {
            if (this.mHsmsParser != null)
            {
                this.mHsmsParser.Stop();
                this.mHsmsParser.OnReceived -= new SocketEvent.OnReceived(this.OnReceived);
                this.mHsmsParser.OnParseError -= new SocketEvent.ParseError(this.HandleParseError);
            }
        }

        private void StopHsmsReader()
        {
            if (this.mHsmsReader != null)
            {
                this.mHsmsReader.Stop();
                this.mHsmsReader.OnDisconnected -= new SocketEvent.DisconnectEventHandler(this.OnDisconnect);
                this.mHsmsReader.OnReadCompleted -= new SocketEvent.ReadCompleteEventHandler(this.OnReadHsms);
                this.mHsmsReader.OnReadError -= new SocketEvent.ReadErrorEventHandler(this.HandleReadError);
                this.mHsmsReader = null;
            }
        }

        private void StopHsmsTimer()
        {
            if (this.mHsmsTimer != null)
            {
                this.mHsmsTimer.Stop();
                this.mHsmsTimer.OnHsmsTimeout -= new SocketEvent.OnTimeout(this.OnTimeout);
            }
        }

        private void StopHsmsWriter()
        {
            if (this.mHsmsWriter != null)
            {
                this.mHsmsWriter.Stop();
                this.mHsmsWriter.OnDisconnected -= new SocketEvent.DisconnectEventHandler(this.OnDisconnect);
                this.mHsmsWriter.OnWriteCompleted -= new SocketEvent.WriteCompleteEventHandler(this.OnSocketWriteComplete);
                this.mHsmsWriter.OnWriteError -= new SocketEvent.WriteErrorEventHandler(this.HandleWriteError);
            }
        }

        public long SystemBytesCountUp()
        {
            long result;
            lock (this.syncSystemBytes)
            {
                if (this.mSystemBytes < 1L)
                {
                    this.mSystemBytes = 1L;
                    result = 1L;
                }
                else
                {
                    long num;
                    this.mSystemBytes = (num = this.mSystemBytes) + 1L;
                    result = num;
                }
            }
            return result;
        }

        public void Terminate()
        {
            this.UpdateStatus(eHSMS_PORT_STATUS.TERMINATE);
            try
            {
                lock (this.syncObject)
                {
                    this.StopHsmsReader();
                    try
                    {
                        if (this.PreStatus == eHSMS_PORT_STATUS.SELECT)
                        {
                            long ctrlSystemBytes = this.GetCtrlSystemBytes();
                            this.mHsmsWriter.WriteControlMessage(ctrlSystemBytes, 0, eControlMessage.SEPARATE, 255, 255);
                            Thread.Sleep(60);
                        }
                    }
                    catch (Exception exception)
                    {
                        this.logger.Error("Terminate", exception);
                    }
                    this.StopHsmsParser();
                    this.StopHsmsConnector();
                    this.StopHsmsWriter();
                    Thread.Sleep(90);
                    this.StopHsmsTimer();
                    this.TerminateSocket();
                    Thread.Sleep(90);
                }
            }
            catch (Exception exception2)
            {
                this.logger.Error("Terminate2", exception2);
            }
        }

        private void TerminateSocket()
        {
            try
            {
                this.mSECSSocket.Close();
                this.logger.Debug("HSMSPort::TerminateSocket execute.");
            }
            catch (Exception exception)
            {
                this.logger.Debug("HSMSPort::TerminateSocket Error.", exception);
            }
        }

        private void TraceRejectReqCode(byte aCode)
        {
        }

        private void UpdateStatus(eHSMS_PORT_STATUS status)
        {
            this._preStatus = this._nowStatus;
            this._nowStatus = status;
        }
    }
}
