using System;
using System.IO;
using System.Threading;

namespace HSMSDriver
{
    internal class HSMSReader : AbstractThread
    {
        private HSMSTimer mHsmsTimer;

        private string name;

        private BinaryReader reader;

        private object syncObject = new object();

        public event SocketEvent.DisconnectEventHandler OnDisconnected;

        public event SocketEvent.ReadCompleteEventHandler OnReadCompleted;

        public event SocketEvent.ReadErrorEventHandler OnReadError;

        public override string Name
        {
            get
            {
                return this.name + "#Reader";
            }
        }

        public HSMSReader(BinaryReader aReader, HSMSTimer aTimer, string name)
        {
            this.mHsmsTimer = aTimer;
            this.reader = aReader;
            this.name = name;
        }

        public SECSBlock ByteToBlock(byte[] aBytes)
        {
            SECSBlock block = new SECSBlock
            {
                Length = aBytes.Length,
                Header = new byte[10],
                DataItem = new byte[aBytes.Length - 10]
            };
            Array.Copy(aBytes, 0, block.Header, 0, block.Header.Length);
            Array.Copy(aBytes, 10, block.DataItem, 0, block.DataItem.Length);
            string str = ByteStringBuilder.ToLogString(block.Header);
            this.logger.Debug(string.Format("Reader#ByteToBlock Header: {0}", str));
            string str2 = ByteStringBuilder.ToLogString(block.DataItem);
            this.logger.Debug(string.Format("Reader#ByteToBlock Data: {0}", str2));
            return block;
        }

        private void Disconnect(string err)
        {
            this.Stop();
            this.logger.Debug("Reader#FireDisconnect Invoked.");
            if (this.OnDisconnected != null)
            {
                this.OnDisconnected(err);
            }
        }

        private byte[] ReadBody(int aLength)
        {
            int num = 0;
            byte[] bs = new byte[aLength];
            while (num < bs.Length)
            {
                this.mHsmsTimer.StartT8Timer();
                int num2 = this.reader.Read(bs, num, bs.Length - num);
                this.mHsmsTimer.StopT8Timer();
                num += num2;
            }
            string str = ByteStringBuilder.ToLogString(bs);
            this.logger.Debug(string.Format("Read Data: {0} -- {1}", aLength, str));
            return bs;
        }

        private void ReadLength(byte[] bs)
        {
            int num = 0;
            try
            {
                while (num < bs.Length)
                {
                    int num2 = this.reader.Read(bs, num, bs.Length - num);
                    num += num2;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        protected override void Run()
        {
            while (this.running)
            {
                byte[] bs = new byte[4];
                try
                {
                    this.ReadLength(bs);
                    int aLength = (int)Byte2SecsValue.GetInt(bs);
                    if (aLength > 0)
                    {
                        SECSBlock block = this.ByteToBlock(this.ReadBody(aLength));
                        if (this.OnReadCompleted != null)
                        {
                            this.OnReadCompleted(block);
                        }
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
                catch (OutOfMemoryException exception)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    this.logger.Error("Reader#Run", exception);
                    if (this.OnReadError != null)
                    {
                        this.OnReadError(string.Format("{0}: Out Of Memory.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ReadError)));
                    }
                    this.Disconnect(exception.Message);
                }
                catch (IOException exception2)
                {
                    this.logger.Error("Reader#Run", exception2);
                    if (this.OnReadError != null)
                    {
                        this.OnReadError(string.Format("{0}: Socket Error.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ReadError)));
                    }
                    this.Disconnect(exception2.Message);
                }
                catch (Exception exception3)
                {
                    this.logger.Error("Reader#Run", exception3);
                    if (this.OnReadError != null)
                    {
                        this.OnReadError(string.Format("{0}: {1}.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ReadError), exception3.Message));
                    }
                    this.Disconnect(exception3.Message);
                }
            }
        }
    }
}
