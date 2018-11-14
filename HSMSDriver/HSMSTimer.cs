using System;
using System.Collections.Generic;
using System.Threading;

namespace HSMSDriver
{
	internal class HSMSTimer : AbstractThread
	{
		private class CheckTimerPara
		{
			public SECSMessage msg;

			public DateTime StartTime;

			public long SystemBytes;
		}

		private long linkTestInterval;

		private string name;

		private Dictionary<eTimeout, HSMSTimer.CheckTimerPara> otherTimerDict = new Dictionary<eTimeout, HSMSTimer.CheckTimerPara>();

		private object syncTimeout = new object();

		private long t3Interval;

		private Dictionary<long, HSMSTimer.CheckTimerPara> t3TimerDict = new Dictionary<long, HSMSTimer.CheckTimerPara>();

		private long t6Interval;

		private long t7Interval;

		private long t8Interval;

		private Queue<TimerPara> timeoutQueue = new Queue<TimerPara>();

		public event SocketEvent.OnTimeout OnHsmsTimeout;

		public override string Name
		{
			get
			{
				return this.name + "#Timer";
			}
		}

		public HSMSTimer(long aT3Interval, long aT6Interval, long aT7Interval, long aT8Interval, long aLinkTestInterval, string name)
		{
			this.t3Interval = aT3Interval;
			this.t6Interval = aT6Interval;
			this.t7Interval = aT7Interval;
			this.t8Interval = aT8Interval;
			this.linkTestInterval = aLinkTestInterval;
			this.name = name;
		}

		private void CheckOtherTimeout()
		{
			try
			{
				List<eTimeout> list = new List<eTimeout>();
				foreach (eTimeout timeout in this.otherTimerDict.Keys)
				{
					list.Add(timeout);
				}
				for (int i = 0; i < list.Count; i++)
				{
					DateTime now = DateTime.Now;
					eTimeout timeout2 = list[i];
					HSMSTimer.CheckTimerPara para = this.otherTimerDict[timeout2];
					double num2 = (now - para.StartTime).TotalMilliseconds / 1000.0;
					if ((timeout2 == eTimeout.T6 && num2 > (double)this.t6Interval) || (timeout2 == eTimeout.T7 && num2 > (double)this.t7Interval) || (timeout2 == eTimeout.T8 && num2 > (double)this.t8Interval) || (timeout2 == eTimeout.LinkTest && num2 > (double)this.linkTestInterval))
					{
						this.logger.Debug(string.Format("HSMSTimer::CheckOtherTimeout: {0}, {1}", num2, timeout2));
						this.otherTimerDict.Remove(timeout2);
						TimerPara para2 = new TimerPara
						{
							Type = timeout2,
							Msg = null
						};
						this.timeoutQueue.Enqueue(para2);
					}
					else if (timeout2 != eTimeout.T6 && timeout2 != eTimeout.T7 && timeout2 != eTimeout.T8 && timeout2 != eTimeout.LinkTest)
					{
						this.logger.Debug(string.Format("CheckOtherTimeout: {0}-{1}", timeout2, num2));
					}
				}
			}
			catch (Exception exception)
			{
				this.logger.Error("CheckOtherTimeout: ", exception);
			}
		}

		private void CheckT3Timeout()
		{
			try
			{
				List<long> list = new List<long>();
				foreach (long num in this.t3TimerDict.Keys)
				{
					list.Add(num);
				}
				for (int i = 0; i < list.Count; i++)
				{
					HSMSTimer.CheckTimerPara para = this.t3TimerDict[list[i]];
					double num2 = (DateTime.Now - para.StartTime).TotalMilliseconds / 1000.0;
					if (num2 > (double)this.t3Interval)
					{
						this.logger.Debug(string.Format("HSMSTimer::CheckT3Timeout: {0}, {1}", num2, para.SystemBytes));
						this.logger.Warn(string.Format("T3 Timeout: System Bytes={0}", para.SystemBytes));
						this.t3TimerDict.Remove(list[i]);
						TimerPara para2 = new TimerPara
						{
							Type = eTimeout.T3,
							Msg = para.msg
						};
						this.timeoutQueue.Enqueue(para2);
					}
				}
			}
			catch (Exception exception)
			{
				this.logger.Error("CheckT3Timeout: ", exception);
			}
		}

		protected override void Run()
		{
			while (this.running)
			{
				try
				{
					lock (this.syncTimeout)
					{
						this.CheckT3Timeout();
						this.CheckOtherTimeout();
						while (this.timeoutQueue.Count > 0)
						{
							TimerPara mPara = this.timeoutQueue.Dequeue();
							if (mPara.Type == eTimeout.T3 && mPara.Msg != null)
							{
								this.logger.Debug(string.Format("Timer#Run: Timeout Message System Bytes={0}", mPara.Msg.SystemBytes));
							}
							if (this.OnHsmsTimeout != null)
							{
								this.OnHsmsTimeout(mPara);
							}
						}
					}
				}
				catch (Exception exception)
				{
					this.logger.Error("Timer#Run", exception);
				}
				Thread.Sleep(50);
			}
		}

		public void StartLinkTestTimer()
		{
			this.StartTimer(eTimeout.LinkTest);
		}

		public void StartT3Timer(SECSMessage msg)
		{
			if (msg != null)
			{
				try
				{
					lock (this.syncTimeout)
					{
						HSMSTimer.CheckTimerPara para = new HSMSTimer.CheckTimerPara
						{
							SystemBytes = msg.SystemBytes,
							msg = msg,
							StartTime = DateTime.Now
						};
						this.t3TimerDict.Add(para.SystemBytes, para);
					}
					this.logger.Debug(string.Format("Timer::StartT3Timer, SystemBytes={0}", msg.SystemBytes));
				}
				catch (Exception exception)
				{
					this.logger.Error("Timer::StartT3Timer", exception);
				}
			}
		}

		public void StartT6Timer()
		{
			this.StartTimer(eTimeout.T6);
		}

		public void StartT7Timer()
		{
			this.StartTimer(eTimeout.T7);
		}

		public void StartT8Timer()
		{
			this.StartTimer(eTimeout.T8);
		}

		private void StartTimer(eTimeout e)
		{
			try
			{
				lock (this.syncTimeout)
				{
					HSMSTimer.CheckTimerPara para = new HSMSTimer.CheckTimerPara
					{
						SystemBytes = (long)e,
						msg = null,
						StartTime = DateTime.Now
					};
					this.otherTimerDict.Add(e, para);
				}
				this.logger.Debug(string.Format("Timer::StartTimer {0}", e));
			}
			catch (Exception exception)
			{
				this.logger.Error("Timer::StartTimer ", exception);
			}
		}

		public void StopLinkTestTimer()
		{
			this.StopTimer(eTimeout.LinkTest);
		}

		public void StopT3Timer(SECSMessage msg)
		{
			if (msg != null)
			{
				try
				{
					lock (this.syncTimeout)
					{
						if (this.t3TimerDict.ContainsKey(msg.SystemBytes))
						{
							this.t3TimerDict.Remove(msg.SystemBytes);
						}
					}
					this.logger.Debug(string.Format("Timer::StopT3Timer, SystemBytes={0}", msg.SystemBytes));
				}
				catch (Exception exception)
				{
					this.logger.Error("Timer::StopT3Timer", exception);
				}
			}
		}

		public void StopT6Timer()
		{
			this.StopTimer(eTimeout.T6);
		}

		public void StopT7Timer()
		{
			this.StopTimer(eTimeout.T7);
		}

		public void StopT8Timer()
		{
			this.StopTimer(eTimeout.T8);
		}

		private void StopTimer(eTimeout e)
		{
			try
			{
				lock (this.syncTimeout)
				{
					if (this.otherTimerDict.ContainsKey(e))
					{
						this.otherTimerDict.Remove(e);
					}
				}
				this.logger.Debug(string.Format("Timer::StopTimer {0}", e));
			}
			catch (Exception exception)
			{
				this.logger.Error("Timer::StopTimer", exception);
			}
		}
	}
}
