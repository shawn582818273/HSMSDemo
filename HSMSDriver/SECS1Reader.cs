using log4net;
using System;
using System.Collections.Generic;

namespace HSMSDriver
{
	internal class SECS1Reader
	{
		private SECS1EventExecutor eventExecutor;

		private string name;

		private List<byte> rcvdBlock = new List<byte>();

		private int rcvdBlockLength;

		private List<SECSBlock> rcvdBlocks = new List<SECSBlock>();

		private SECS1Port secs1;

		private object syncRcvd = new object();

		private SECS1Writer writer;

		internal ILog Logger
		{
			get;
			set;
		}

		internal SECS1Reader(string name, SECS1Port secs1, SECS1Writer writer, SECS1EventExecutor exe)
		{
			this.name = name;
			this.secs1 = secs1;
			this.writer = writer;
			this.eventExecutor = exe;
		}

		internal void Clear()
		{
			lock (this.syncRcvd)
			{
				this.rcvdBlock.Clear();
				this.rcvdBlockLength = 0;
			}
		}

		internal void ClearAll()
		{
			lock (this.syncRcvd)
			{
				this.rcvdBlock.Clear();
				this.rcvdBlocks.Clear();
				this.rcvdBlockLength = 0;
			}
		}

		internal void DataReceived(byte bytedata)
		{
			if (this.secs1.PortStatus != eSECS1_PORT_STATUS.PortRcvd && (bytedata == 4 || bytedata == 5 || bytedata == 6 || bytedata == 21))
			{
				this.RcvdHandshake((eHANDSHAKE)bytedata);
				return;
			}
			this.RcvdData(bytedata);
		}

		private void HandleContention()
		{
			if (this.secs1.PortStatus != eSECS1_PORT_STATUS.PortCtrl)
			{
				this.Logger.Debug(string.Format("Port Status={0}, Not PortCtrl, Error Contention.", this.secs1.PortStatus));
				return;
			}
			if (this.secs1.SECS1Para.MasterSlave == eSECS1_MS.EQP)
			{
				this.Logger.Debug("Contention, Ignore ENQ and wait EOT to continue sending data.");
				return;
			}
			this.secs1.StopTimer(eTimeout.T2);
			this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortCtrl, eSECS1_PORT_STATUS.PortCmpl);
			this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortCmpl, eSECS1_PORT_STATUS.PortCtrl);
			this.secs1.SendEOT();
			this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortCtrl, eSECS1_PORT_STATUS.PortRcvd);
			this.Logger.Debug("Contention, Reply to Send EOT for receive data.");
			this.Clear();
		}

		private void RcvdData(byte data)
		{
			if (this.secs1.PortStatus == eSECS1_PORT_STATUS.PortRcvd)
			{
				lock (this.syncRcvd)
				{
					this.rcvdBlock.Add(data);
					if (this.rcvdBlockLength == 0)
					{
						this.rcvdBlockLength = (int)this.rcvdBlock[0];
						this.secs1.StopTimer(eTimeout.T2);
						if (this.rcvdBlockLength < 10 || this.rcvdBlockLength > 254)
						{
							this.Logger.Debug(string.Format("RECV Invalid Length Bytes: {0}.", this.rcvdBlockLength));
							this.rcvdBlockLength = 0;
							this.rcvdBlock.Clear();
							this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortRcvd, eSECS1_PORT_STATUS.PortCmpl);
							this.secs1.StartTimer(eTimeout.T1);
							goto IL_3E2;
						}
					}
					else
					{
						this.secs1.StopTimer(eTimeout.T1);
					}
					byte[] buffer = this.rcvdBlock.ToArray();
					if (this.rcvdBlockLength == this.rcvdBlock.Count - 3)
					{
						byte[] buffer2 = new byte[10];
						Array.Copy(buffer, 1, buffer2, 0, 10);
						byte[] buffer3 = new byte[2];
						Array.Copy(buffer, buffer.Length - 2, buffer3, 0, 2);
						byte[] buffer4 = new byte[buffer.Length - 13];
						Array.Copy(buffer, buffer2.Length + 1, buffer4, 0, buffer4.Length);
						//this.Logger.Info(string.Format("[RECV] HDR = {0}\nDATA = {1}\nCSUM = {2}", SecsItem2Str.GetBinaryStr(buffer2), SecsItem2Str.GetBinaryStr(buffer4), SecsItem2Str.GetBinaryStr(buffer3)));
						int num = (int)buffer3[0] << 8 | (int)buffer3[1];
						int num2 = (int)(buffer2[4] & 128);
						int num3 = 0;
						for (int i = 1; i < this.rcvdBlock.Count - 2; i++)
						{
							num3 += (int)this.rcvdBlock[i];
						}
						if (num != num3)
						{
							this.secs1.UpdatePortStatus(this.secs1.PortStatus, eSECS1_PORT_STATUS.PortIdle);
							this.secs1.SendNAK();
							this.Logger.Debug("Checksum not match, Reply to send NAK.");
						}
						else
						{
							if (this.rcvdBlocks.Count > 0)
							{
								this.secs1.StopTimer(eTimeout.T4);
							}
							this.secs1.UpdatePortStatus(this.secs1.PortStatus, eSECS1_PORT_STATUS.PortIdle);
							this.secs1.SendACK();
							if (num2 == 0)
							{
								SECSBlock block = new SECSBlock
								{
									Header = buffer2,
									DataItem = buffer4,
									CheckSum = buffer3
								};
								this.rcvdBlocks.Add(block);
								this.Logger.Debug("RECV One of Multi-Block Ok, Send ACK.");
								this.secs1.StartTimer(eTimeout.T4);
							}
							else if (this.rcvdBlocks.Count == 0)
							{
								this.Logger.Debug("RECV Single-Block Message Ok, Send ACK.");
								SECSDecoding decoding = new SECSDecoding();
								SECSMessage msg = decoding.Byte_TO_SecsMessage(buffer2);
								SECSItem item = decoding.Byte_TO_SecsItem(buffer4);
								if (item != null)
								{
									msg.Root = item;
								}
								this.eventExecutor.NofityRECV(msg);
							}
							else
							{
								this.Logger.Debug("RECV Multi-Block Message Ok, Send ACK.");
								SECSDecoding decoding2 = new SECSDecoding();
								SECSMessage message2 = decoding2.Byte_TO_SecsMessage(buffer2);
								List<byte> list = new List<byte>();
								for (int j = 0; j < this.rcvdBlocks.Count; j++)
								{
									list.AddRange(this.rcvdBlocks[j].DataItem);
								}
								this.rcvdBlocks.Clear();
								list.AddRange(buffer4);
								byte[] aDataByte = list.ToArray();
								SECSItem item2 = decoding2.Byte_TO_SecsItem(aDataByte);
								if (item2 != null)
								{
									message2.Root = item2;
								}
								this.eventExecutor.NofityRECV(message2);
							}
						}
					}
					else
					{
						this.secs1.StartTimer(eTimeout.T1);
					}
					IL_3E2:;
				}
				return;
			}
			if (this.secs1.PortStatus == eSECS1_PORT_STATUS.PortCmpl)
			{
				this.secs1.StopTimer(eTimeout.T1);
				this.Logger.Info(string.Format("PortCmpl, Invalid data {0}", data));
				this.secs1.StartTimer(eTimeout.T1);
				return;
			}
			this.Logger.Info(string.Format("Invalid Status - {0}, Invalid data: {1}", this.secs1.PortStatus, data));
		}

		private void RcvdHandshake(eHANDSHAKE e)
		{
			//this.Logger.Info(string.Format("[RECV] ({0}) {1:D2} {2}", e, (int)e, (e == eHANDSHAKE.ACK || e == eHANDSHAKE.NAK) ? "\n" : ""));
			if (this.secs1.PortStatus != eSECS1_PORT_STATUS.PortIdle)
			{
				if (this.secs1.PortStatus == eSECS1_PORT_STATUS.PortCtrl)
				{
					if (e == eHANDSHAKE.EOT)
					{
						if (this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortCtrl, eSECS1_PORT_STATUS.PortSend) == 0)
						{
							this.secs1.StopTimer(eTimeout.T2);
							this.secs1.SendBody();
							this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortSend, eSECS1_PORT_STATUS.PortCmpl);
							this.secs1.StartTimer(eTimeout.T2);
							return;
						}
						this.Logger.Debug("RECV EOT, Error Happen. Update Port Status to PortSend Failed!");
						return;
					}
					else
					{
						if (e == eHANDSHAKE.ENQ)
						{
							this.Logger.Debug("RECV ENQ, Contention Happen.");
							this.HandleContention();
							return;
						}
						if (e == eHANDSHAKE.ACK)
						{
							this.Logger.Debug(string.Format("Port Status={0}, Invalid ACK.", this.secs1.PortStatus));
							return;
						}
						this.Logger.Debug(string.Format("Port Status={0}, Invalid NAK.", this.secs1.PortStatus));
						return;
					}
				}
				else if (this.secs1.PortStatus == eSECS1_PORT_STATUS.PortSend)
				{
					if (e == eHANDSHAKE.ENQ)
					{
						this.Logger.Debug(string.Format("Port Status=%s, Invalid ENQ.", this.secs1.PortStatus));
						return;
					}
					if (e == eHANDSHAKE.EOT)
					{
						this.Logger.Debug(string.Format("Port Status=%s, Invalid EOT.", this.secs1.PortStatus));
						return;
					}
					if (e == eHANDSHAKE.ACK)
					{
						this.Logger.Debug(string.Format("Port Status=%s, Invalid ACK.", this.secs1.PortStatus));
						return;
					}
					this.Logger.Debug(string.Format("Port Status=%s, Invalid NAK.", this.secs1.PortStatus));
					return;
				}
				else
				{
					if (this.secs1.PortStatus == eSECS1_PORT_STATUS.PortRcvd)
					{
						this.RcvdData((byte)e);
						return;
					}
					if (this.secs1.PortStatus == eSECS1_PORT_STATUS.PortCmpl)
					{
						if (e == eHANDSHAKE.ACK)
						{
							this.secs1.StopTimer(eTimeout.T2);
							if (this.writer.SendBlocks.Count > 0)
							{
								this.writer.SendBlock = this.writer.SendBlocks.Dequeue();
								this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortCmpl, eSECS1_PORT_STATUS.PortIdle);
								this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortIdle, eSECS1_PORT_STATUS.PortCtrl);
								this.secs1.SendENQ();
								this.writer.RetryCount = this.secs1.SECS1Para.RetryCount;
								this.Logger.Debug("Send Next Block, Send ENQ.");
								return;
							}
							bool isPrimarySent = true;
							if (this.writer.SendMsg.Function % 2 == 0)
							{
								isPrimarySent = false;
							}
							this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortCmpl, eSECS1_PORT_STATUS.PortIdle);
							this.eventExecutor.NotifySENT(isPrimarySent, this.writer.SendMsg);
							this.writer.SendMsg = null;
							return;
						}
						else if (e == eHANDSHAKE.NAK)
						{
							this.secs1.StopTimer(eTimeout.T2);
							if (this.writer.RetryCount > 0)
							{
								this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortSend, eSECS1_PORT_STATUS.PortCtrl);
								this.secs1.SendENQ();
								this.writer.RetryCount = this.writer.RetryCount - 1;
								this.Logger.Debug("RECV NAK, retry to Send ENQ.");
								return;
							}
							this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortSend, eSECS1_PORT_STATUS.PortIdle);
							this.Logger.Debug("RECV NAK. Retry Complete.");
							this.eventExecutor.NotifyEVENT(SECSEventType.Error, this.writer.SendMsg.Transaction, SECSErrors.RcvdNAK);
							this.writer.SendMsg = null;
							return;
						}
					}
				}
			}
			else
			{
				if (e == eHANDSHAKE.ENQ)
				{
					if (this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortIdle, eSECS1_PORT_STATUS.PortCtrl) == 0)
					{
						this.secs1.SendEOT();
						this.secs1.UpdatePortStatus(eSECS1_PORT_STATUS.PortCtrl, eSECS1_PORT_STATUS.PortRcvd);
						lock (this.syncRcvd)
						{
							this.rcvdBlockLength = 0;
							this.rcvdBlock.Clear();
							return;
						}
					}
					this.Logger.Debug("RECV ENQ, Contention Happen.");
					this.HandleContention();
					return;
				}
				if (e == eHANDSHAKE.EOT)
				{
					this.Logger.Debug(string.Format("Port Status={0}, Invalid EOT.", this.secs1.PortStatus));
					return;
				}
				if (e == eHANDSHAKE.ACK)
				{
					this.Logger.Debug(string.Format("Port Status={0}, Invalid ACK.", this.secs1.PortStatus));
					return;
				}
				this.Logger.Debug(string.Format("Port Status={0}, Invalid NAK.", this.secs1.PortStatus));
			}
		}
	}
}
