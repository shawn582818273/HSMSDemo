using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace HSMSDriver
{
    internal sealed class HSMSWriter : AbstractThread
    {
        private HSMSTimer mHsmsTimer;

        private Queue<SECSMessage> mQueueReply = new Queue<SECSMessage>();

        private Queue<SECSMessage> mQueueSend = new Queue<SECSMessage>();

        private string name;

        private object syncReplyObject = new object();

        private object syncSendObject = new object();

        private BinaryWriter writer;

        public event SocketEvent.DisconnectEventHandler OnDisconnected;

        public event SocketEvent.WriteCompleteEventHandler OnWriteCompleted;

        public event SocketEvent.WriteErrorEventHandler OnWriteError;

        public override string Name
        {
            get
            {
                return this.name + "#Writer";
            }
        }

        public HSMSWriter(HSMSTimer aTimer, BinaryWriter aWriter, string name)
        {
            this.mHsmsTimer = aTimer;
            this.writer = aWriter;
            this.name = name;
        }

        public void EnqueReply(SECSMessage msg)
        {
            lock (this.syncReplyObject)
            {
                this.mQueueReply.Enqueue(msg);
            }
        }

        public void EnqueSend(SECSMessage msg)
        {
            lock (this.syncSendObject)
            {
                this.mQueueSend.Enqueue(msg);
            }
        }

        public void FireDisconnect(string err)
        {
            this.Stop();
            if (this.OnDisconnected != null)
            {
                this.OnDisconnected(err);
            }
            this.logger.Debug("Writer::FireDisconnect() Invoked");
        }

        protected override void Run()
        {
            while (this.running)
            {
                try
                {
                    if (this.mQueueReply.Count > 0)
                    {
                        SECSMessage msg = null;
                        lock (this.syncReplyObject)
                        {
                            msg = this.mQueueReply.Dequeue();
                        }
                        if (msg != null)
                        {
                            this.logger.Debug("Writer#Run Send Secondary Message " + msg.SystemBytes.ToString());
                            this.WriteReplyMessage(msg);
                        }
                    }
                    if (this.mQueueSend.Count > 0)
                    {
                        SECSMessage message2 = null;
                        lock (this.syncSendObject)
                        {
                            message2 = this.mQueueSend.Dequeue();
                        }
                        if (message2 != null)
                        {
                            this.logger.Debug("Writer#Run Send Primary Message " + message2.SystemBytes.ToString());
                            this.WriteSendMessage(message2);
                        }
                    }
                    if (this.mQueueSend.Count > 0 || this.mQueueReply.Count > 0)
                    {
                        continue;
                    }
                }
                catch (Exception exception)
                {
                    this.logger.Error("Writer#Run: ", exception);
                    if (this.OnWriteError != null)
                    {
                        this.OnWriteError(SECSEventType.Error, null, string.Format("{0}: {1}", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.WriteError), exception.Message));
                    }
                    this.FireDisconnect(exception.Message);
                }
                Thread.Sleep(50);
            }
        }

        public void WriteControlMessage(long mSystemBytes, byte rspcode, eControlMessage stype, byte high, byte low)
        {
            try
            {
                byte[] bs = new byte[14];
                bs[0] = 0;
                bs[1] = 0;
                bs[2] = 0;
                bs[3] = 10;
                byte[] intBytes = SecsValue2Byte.GetIntBytes((int)mSystemBytes, 4);
                bs[10] = intBytes[0];
                bs[11] = intBytes[1];
                bs[12] = intBytes[2];
                bs[13] = intBytes[3];
                bs[4] = high;
                bs[5] = low;
                bs[7] = rspcode;
                bs[9] = (byte)stype;
                if (stype == eControlMessage.SELECT_REQ)
                {
                    this.mHsmsTimer.StartT6Timer();
                }
                else if (stype == eControlMessage.LINKTEST_REQ)
                {
                    this.mHsmsTimer.StartT6Timer();
                }
                this.logger.Debug(string.Format("[WriteControlMessage] [{0}-{2}] -- {1}", mSystemBytes, ByteStringBuilder.ToLogString(bs), stype));
                this.writer.Write(bs);
                this.writer.Flush();
            }
            catch (Exception exception)
            {
                this.logger.Error("WriteControlMessage", exception);
                if (this.OnWriteError != null)
                {
                    this.OnWriteError(SECSEventType.Error, null, string.Format("{0}: Socket Error.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.WriteError)));
                }
            }
        }

        public void WriteReplyMessage(SECSMessage msg)
        {
            if (msg == null)
            {
                this.logger.Debug("WriteReplyMessage msg==null.");
                return;
            }
            byte[] bs = null;
            try
            {
                bs = new SECSEncoding(msg).GetEncodingData((int)msg.DeviceIdID, msg.WBit, msg.SystemBytes);
            }
            catch (Exception exception)
            {
                this.logger.Error("WriteReplyMessage: encoder", exception);
                SECSTransaction t = msg.Transaction;
                if (this.OnWriteError != null)
                {
                    this.OnWriteError(SECSEventType.SecondarySent, t, string.Format("{0}: Invalid SECS Message Format or Data.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.WriteError)));
                }
            }
            try
            {
                if (bs != null && bs.Length > 0)
                {
                    this.writer.Write(bs);
                    this.writer.Flush();
                    //this.logger.Info(string.Format("[SEND] S{0}F{1} {2} SystemBytes={3}\n{4}", new object[]
                    //{
                    //    msg.Stream,
                    //    msg.Function,
                    //    msg.WBit ? "W" : "",
                    //    msg.SystemBytes,
                    //    ByteStringBuilder.ToLogString(bs)
                    //}));
                    //this.logger.Warn("[SEND] " + SecsItem2Str.GetSecsMessageStr(msg));
                    if (this.OnWriteCompleted != null)
                    {
                        this.OnWriteCompleted(true, msg);
                    }
                }
                else
                {
                    this.logger.Error("WriteReplyMessage after encoding, byte is null");
                }
            }
            catch (Exception exception2)
            {
                this.logger.Error("WriteReplyMessage", exception2);
                SECSTransaction transaction = msg.Transaction;
                if (this.OnWriteError != null)
                {
                    this.OnWriteError(SECSEventType.SecondarySent, transaction, string.Format("{0}: Socket Error.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.WriteError)));
                }
                this.FireDisconnect(exception2.Message);
            }
        }

        public void WriteSendMessage(SECSMessage msg)
        {
            if (msg == null)
            {
                this.logger.Error("WriteSendMessage msg == null");
                return;
            }
            byte[] bs = null;
            try
            {
                bs = new SECSEncoding(msg).GetEncodingData((int)msg.DeviceIdID, msg.WBit, msg.SystemBytes);
                if (bs.Length > 0 && msg.WBit)
                {
                    this.mHsmsTimer.StartT3Timer(msg);
                    this.logger.Debug(string.Format("WriteSendMessage: StartT3Timer {0}", msg.SystemBytes));
                }
            }
            catch (Exception exception)
            {
                this.logger.Error("WriteSendMessage: encoder", exception);
                SECSTransaction t = msg.Transaction;
                if (this.OnWriteError != null)
                {
                    this.OnWriteError(SECSEventType.PrimarySent, t, string.Format("{0}: Invalid SECS Message Format or Data.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.WriteError)));
                }
            }
            if (bs != null && bs.Length > 0)
            {
                try
                {
                    this.writer.Write(bs);
                    this.writer.Flush();
                    //this.logger.Info(string.Format("[SEND] S{0}F{1} {2} SystemBytes={3}\n{4}", new object[]
                    //{
                    //    msg.Stream,
                    //    msg.Function,
                    //    msg.WBit ? "W" : "",
                    //    msg.SystemBytes,
                    //    ByteStringBuilder.ToLogString(bs)
                    //}));
                    //this.logger.Warn("[SEND] " + SecsItem2Str.GetSecsMessageStr(msg));
                    if (this.OnWriteCompleted != null)
                    {
                        this.OnWriteCompleted(false, msg);
                    }
                }
                catch (Exception exception2)
                {
                    this.logger.Error("WriteSendMessage", exception2);
                    SECSTransaction transaction = msg.Transaction;
                    if (this.OnWriteError != null)
                    {
                        this.OnWriteError(SECSEventType.PrimarySent, transaction, string.Format("{0}: Socket Error.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.WriteError)));
                    }
                    this.FireDisconnect(exception2.Message);
                }
            }
        }
    }
}
