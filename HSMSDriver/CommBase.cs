using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace HSMSDriver
{
	internal abstract class CommBase : IDisposable
	{
		private bool auto;

		private bool checkSends = true;

		private IntPtr hPort;

		private bool online;

		private IntPtr ptrUWO = IntPtr.Zero;

		private Exception rxException;

		private bool rxExceptionReported;

		private Thread rxThread;

		private int stateBRK = 2;

		private int stateDTR = 2;

		private int stateRTS = 2;

		private int writeCount;

		private ManualResetEvent writeEvent = new ManualResetEvent(false);

		protected bool Break
		{
			get
			{
				return this.stateBRK == 1;
			}
			set
			{
				if (this.stateBRK <= 1)
				{
					this.CheckOnline();
					if (value)
					{
						if (Win32Com.EscapeCommFunction(this.hPort, 8u))
						{
							this.stateBRK = 0;
							return;
						}
						this.ThrowException("Unexpected Failure");
						return;
					}
					else
					{
						if (Win32Com.EscapeCommFunction(this.hPort, 9u))
						{
							this.stateBRK = 0;
							return;
						}
						this.ThrowException("Unexpected Failure");
					}
				}
			}
		}

		protected bool DTR
		{
			get
			{
				return this.stateDTR == 1;
			}
			set
			{
				if (this.stateDTR <= 1)
				{
					this.CheckOnline();
					if (value)
					{
						if (Win32Com.EscapeCommFunction(this.hPort, 5u))
						{
							this.stateDTR = 1;
							return;
						}
						this.ThrowException("Unexpected Failure");
						return;
					}
					else
					{
						if (Win32Com.EscapeCommFunction(this.hPort, 6u))
						{
							this.stateDTR = 0;
							return;
						}
						this.ThrowException("Unexpected Failure");
					}
				}
			}
		}

		protected bool DTRavailable
		{
			get
			{
				return this.stateDTR < 2;
			}
		}

		public bool Online
		{
			get
			{
				return this.online && this.CheckOnline();
			}
		}

		protected bool RTS
		{
			get
			{
				return this.stateRTS == 1;
			}
			set
			{
				if (this.stateRTS <= 1)
				{
					this.CheckOnline();
					if (value)
					{
						if (Win32Com.EscapeCommFunction(this.hPort, 3u))
						{
							this.stateRTS = 1;
							return;
						}
						this.ThrowException("Unexpected Failure");
						return;
					}
					else
					{
						if (Win32Com.EscapeCommFunction(this.hPort, 4u))
						{
							this.stateRTS = 1;
							return;
						}
						this.ThrowException("Unexpected Failure");
					}
				}
			}
		}

		protected bool RTSavailable
		{
			get
			{
				return this.stateRTS < 2;
			}
		}

		protected virtual bool AfterOpen()
		{
			return true;
		}

		protected virtual void BeforeClose(bool error)
		{
		}

		private bool CheckOnline()
		{
			if (this.rxException != null && !this.rxExceptionReported)
			{
				this.rxExceptionReported = true;
				this.ThrowException("rx");
			}
			if (this.online)
			{
				uint num;
				if (Win32Com.GetHandleInformation(this.hPort, out num))
				{
					return true;
				}
				this.ThrowException("Offline");
				return false;
			}
			else
			{
				if (this.auto && this.Open())
				{
					return true;
				}
				this.ThrowException("Offline");
				return false;
			}
		}

		private void CheckResult()
		{
			uint nNumberOfBytesTransferred = 0u;
			if (this.writeCount > 0)
			{
				if (Win32Com.GetOverlappedResult(this.hPort, this.ptrUWO, out nNumberOfBytesTransferred, this.checkSends))
				{
					this.writeCount = (int)((long)this.writeCount - (long)((ulong)nNumberOfBytesTransferred));
					if (this.writeCount != 0)
					{
						this.ThrowException("Send Timeout");
						return;
					}
				}
				else if ((long)Marshal.GetLastWin32Error() != 997L)
				{
					this.ThrowException("Unexpected failure");
				}
			}
		}

		public void Close()
		{
			if (this.online)
			{
				this.auto = false;
				this.BeforeClose(false);
				this.InternalClose();
				this.rxException = null;
			}
		}

		protected virtual CommBaseSettings CommSettings()
		{
			return new CommBaseSettings();
		}

		public void Dispose()
		{
			this.Close();
		}

		~CommBase()
		{
			this.Close();
		}

		public void Flush()
		{
			this.CheckOnline();
			this.CheckResult();
		}

		protected ModemStatus GetModemStatus()
		{
			this.CheckOnline();
			uint num;
			if (!Win32Com.GetCommModemStatus(this.hPort, out num))
			{
				this.ThrowException("Unexpected failure");
			}
			return new ModemStatus(num);
		}

		protected QueueStatus GetQueueStatus()
		{
			this.CheckOnline();
			uint num;
			COMSTAT comstat;
			if (!Win32Com.ClearCommError(this.hPort, out num, out comstat))
			{
				this.ThrowException("Unexpected failure");
			}
			COMMPROP commprop;
			if (!Win32Com.GetCommProperties(this.hPort, out commprop))
			{
				this.ThrowException("Unexpected failure");
			}
			return new QueueStatus(comstat.Flags, comstat.cbInQue, comstat.cbOutQue, commprop.dwCurrentRxQueue, commprop.dwCurrentTxQueue);
		}

		private void InternalClose()
		{
			Win32Com.CancelIo(this.hPort);
			if (this.rxThread != null)
			{
				this.rxThread.Abort();
				this.rxThread = null;
			}
			Win32Com.CloseHandle(this.hPort);
			if (this.ptrUWO != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(this.ptrUWO);
			}
			this.stateRTS = 2;
			this.stateDTR = 2;
			this.stateBRK = 2;
			this.online = false;
		}

		protected virtual void OnBreak()
		{
		}

		protected virtual void OnRing()
		{
		}

		protected virtual void OnRxChar(byte ch)
		{
		}

		protected virtual void OnRxException(Exception e)
		{
		}

		protected virtual void OnStatusChange(ModemStatus mask, ModemStatus state)
		{
		}

		protected virtual void OnTxDone()
		{
		}

		public bool Open()
		{
			DCB lpDCB = default(DCB);
			COMMTIMEOUTS lpCommTimeouts = default(COMMTIMEOUTS);
			OVERLAPPED overlapped = default(OVERLAPPED);
			if (!this.online)
			{
				CommBaseSettings settings = this.CommSettings();
				this.hPort = Win32Com.CreateFile(settings.port, 3221225472u, 0u, IntPtr.Zero, 3u, 1073741824u, IntPtr.Zero);
				if (this.hPort == (IntPtr)(-1))
				{
					if ((long)Marshal.GetLastWin32Error() != 5L)
					{
						throw new CommPortException("Port Open Failure");
					}
					return false;
				}
				else
				{
					this.online = true;
					lpCommTimeouts.ReadIntervalTimeout = 0;
					lpCommTimeouts.ReadTotalTimeoutConstant = 0;
					lpCommTimeouts.ReadTotalTimeoutMultiplier = 0;
					lpCommTimeouts.WriteTotalTimeoutConstant = settings.sendTimeoutConstant;
					lpCommTimeouts.WriteTotalTimeoutMultiplier = settings.sendTimeoutMultiplier;
					lpDCB.init(settings.parity == Parity.odd || settings.parity == Parity.even, settings.txFlowCTS, settings.txFlowDSR, (int)settings.useDTR, settings.rxGateDSR, !settings.txWhenRxXoff, settings.txFlowX, settings.rxFlowX, (int)settings.useRTS);
					lpDCB.BaudRate = settings.baudRate;
					lpDCB.ByteSize = (byte)settings.dataBits;
					lpDCB.Parity = (byte)settings.parity;
					lpDCB.StopBits = (byte)settings.stopBits;
					lpDCB.XoffChar = (byte)settings.XoffChar;
					lpDCB.XonChar = (byte)settings.XonChar;
					lpDCB.XoffLim = (short)settings.rxHighWater;
					lpDCB.XonLim = (short)settings.rxLowWater;
					if ((settings.rxQueue != 0 || settings.txQueue != 0) && !Win32Com.SetupComm(this.hPort, (uint)settings.rxQueue, (uint)settings.txQueue))
					{
						this.ThrowException("Bad queue settings");
					}
					if (!Win32Com.SetCommState(this.hPort, ref lpDCB))
					{
						this.ThrowException("Bad com settings");
					}
					if (!Win32Com.SetCommTimeouts(this.hPort, ref lpCommTimeouts))
					{
						this.ThrowException("Bad timeout settings");
					}
					this.stateBRK = 0;
					if (settings.useDTR == HSOutput.none)
					{
						this.stateDTR = 0;
					}
					if (settings.useDTR == HSOutput.online)
					{
						this.stateDTR = 1;
					}
					if (settings.useRTS == HSOutput.none)
					{
						this.stateRTS = 0;
					}
					if (settings.useRTS == HSOutput.online)
					{
						this.stateRTS = 1;
					}
					this.checkSends = settings.checkAllSends;
					overlapped.Offset = 0u;
					overlapped.OffsetHigh = 0u;
					if (this.checkSends)
					{
						overlapped.hEvent = this.writeEvent.SafeWaitHandle.DangerousGetHandle();
					}
					else
					{
						overlapped.hEvent = IntPtr.Zero;
					}
					this.ptrUWO = Marshal.AllocHGlobal(Marshal.SizeOf(overlapped));
					Marshal.StructureToPtr(overlapped, this.ptrUWO, true);
					this.writeCount = 0;
					this.rxException = null;
					this.rxExceptionReported = false;
					this.rxThread = new Thread(new ThreadStart(this.ReceiveThread));
					this.rxThread.Name = "CommBaseRx";
					this.rxThread.Priority = ThreadPriority.AboveNormal;
					this.rxThread.Start();
					Thread.Sleep(1);
					this.auto = false;
					if (this.AfterOpen())
					{
						this.auto = settings.autoReopen;
						return true;
					}
					this.Close();
				}
			}
			return false;
		}

		private void ReceiveThread()
		{
			byte[] lpBuffer = new byte[1];
			AutoResetEvent event2 = new AutoResetEvent(false);
			OVERLAPPED overlapped = default(OVERLAPPED);
			IntPtr lpOverlapped = Marshal.AllocHGlobal(Marshal.SizeOf(overlapped));
			overlapped.Offset = 0u;
			overlapped.OffsetHigh = 0u;
			overlapped.hEvent = event2.SafeWaitHandle.DangerousGetHandle();
			Marshal.StructureToPtr(overlapped, lpOverlapped, true);
			uint num2 = 0u;
			IntPtr lpEvtMask = Marshal.AllocHGlobal(Marshal.SizeOf(num2));
			try
			{
				while (Win32Com.SetCommMask(this.hPort, 509u))
				{
					Marshal.WriteInt32(lpEvtMask, 0);
					if (!Win32Com.WaitCommEvent(this.hPort, lpEvtMask, lpOverlapped))
					{
						if ((long)Marshal.GetLastWin32Error() != 997L)
						{
							throw new CommPortException("IO Error [002]");
						}
						event2.WaitOne();
					}
					num2 = (uint)Marshal.ReadInt32(lpEvtMask);
					if ((num2 & 128u) != 0u)
					{
						uint num3;
						if (!Win32Com.ClearCommError(this.hPort, out num3, IntPtr.Zero))
						{
							throw new CommPortException("IO Error [003]");
						}
						StringBuilder builder = new StringBuilder("UART Error: ", 40);
						if ((num3 & 8u) != 0u)
						{
							builder = builder.Append("Framing,");
						}
						if ((num3 & 1024u) != 0u)
						{
							builder = builder.Append("IO,");
						}
						if ((num3 & 2u) != 0u)
						{
							builder = builder.Append("Overrun,");
						}
						if ((num3 & 1u) != 0u)
						{
							builder = builder.Append("Receive Cverflow,");
						}
						if ((num3 & 4u) != 0u)
						{
							builder = builder.Append("Parity,");
						}
						if ((num3 & 256u) != 0u)
						{
							builder = builder.Append("Transmit Overflow,");
						}
						builder.Length--;
						throw new CommPortException(builder.ToString());
					}
					else
					{
						if ((num2 & 1u) != 0u)
						{
							while (true)
							{
								uint num4 = 0u;
								if (!Win32Com.ReadFile(this.hPort, lpBuffer, 1u, out num4, lpOverlapped))
								{
									if ((long)Marshal.GetLastWin32Error() != 997L)
									{
										break;
									}
									Win32Com.CancelIo(this.hPort);
									num4 = 0u;
								}
								if (num4 == 1u)
								{
									this.OnRxChar(lpBuffer[0]);
								}
								if (num4 <= 0u)
								{
									goto IL_1F7;
								}
							}
							throw new CommPortException("IO Error [004]");
						}
						IL_1F7:
						if ((num2 & 4u) != 0u)
						{
							this.OnTxDone();
						}
						if ((num2 & 64u) != 0u)
						{
							this.OnBreak();
						}
						uint val = 0u;
						if ((num2 & 8u) != 0u)
						{
							val |= 16u;
						}
						if ((num2 & 16u) != 0u)
						{
							val |= 32u;
						}
						if ((num2 & 32u) != 0u)
						{
							val |= 128u;
						}
						if ((num2 & 256u) != 0u)
						{
							val |= 64u;
						}
						uint num5;
						if (!Win32Com.GetCommModemStatus(this.hPort, out num5))
						{
							throw new CommPortException("IO Error [005]");
						}
						this.OnStatusChange(new ModemStatus(val), new ModemStatus(num5));
					}
				}
				throw new CommPortException("IO Error [001]");
			}
			catch (Exception exception)
			{
				if (lpEvtMask != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(lpEvtMask);
				}
				if (lpOverlapped != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(lpOverlapped);
				}
				if (!(exception is ThreadAbortException))
				{
					this.rxException = exception;
					this.OnRxException(exception);
				}
			}
		}

		protected void Send(byte[] tosend)
		{
			uint lpNumberOfBytesWritten = 0u;
			this.CheckOnline();
			this.CheckResult();
			this.writeCount = tosend.GetLength(0);
			if (Win32Com.WriteFile(this.hPort, tosend, (uint)this.writeCount, out lpNumberOfBytesWritten, this.ptrUWO))
			{
				this.writeCount = (int)((long)this.writeCount - (long)((ulong)lpNumberOfBytesWritten));
				return;
			}
			if ((long)Marshal.GetLastWin32Error() != 997L)
			{
				this.ThrowException("Unexpected failure");
			}
		}

		protected void Send(byte tosend)
		{
			byte[] buffer = new byte[]
			{
				tosend
			};
			this.Send(buffer);
		}

		protected void SendImmediate(byte tosend)
		{
			this.CheckOnline();
			if (!Win32Com.TransmitCommChar(this.hPort, tosend))
			{
				this.ThrowException("Transmission failure");
			}
		}

		protected void Sleep(int milliseconds)
		{
			Thread.Sleep(milliseconds);
		}

		protected void ThrowException(string reason)
		{
			if (Thread.CurrentThread == this.rxThread)
			{
				throw new CommPortException(reason);
			}
			if (this.online)
			{
				this.BeforeClose(true);
				this.InternalClose();
			}
			if (this.rxException == null)
			{
				throw new CommPortException(reason);
			}
			throw new CommPortException(this.rxException);
		}
	}
}
