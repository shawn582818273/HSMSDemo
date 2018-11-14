using System;
using System.Collections.Generic;
using System.Threading;

namespace HSMSDriver
{
	internal class SECS1Timer : AbstractThread
	{
		private struct CheckTimerPara
		{
			public long SystemBytes;

			public SECSMessage msg;

			public DateTime StartTime;
		}

		public delegate void OnTimeout(TimerPara mPara);

		private Dictionary<eTimeout, SECS1Timer.CheckTimerPara> mOtherTimers = new Dictionary<eTimeout, SECS1Timer.CheckTimerPara>();

		private double mT1Interval;

		private double mT2Interval;

		private long mT3Interval;

		private long mT4Interval;

		private Queue<TimerPara> mTimeoutQueue = new Queue<TimerPara>();

		private Dictionary<long, SECS1Timer.CheckTimerPara> mTimerMsg = new Dictionary<long, SECS1Timer.CheckTimerPara>();

		private bool multiT3;

		private string name;

		private object syncTimeout = new object();

		public event SECS1Timer.OnTimeout OnSECS1Timeout;

		public override string Name
		{
			get
			{
				return this.name + "-SECS1Timer";
			}
		}

		public SECS1Timer(double aT1Interval, double aT2Interval, long aT3Interval, long aT4Interval, string name)
		{
			this.mT1Interval = aT1Interval;
			this.mT2Interval = aT2Interval;
			this.mT3Interval = aT3Interval;
			this.mT4Interval = aT4Interval;
			this.multiT3 = true;
			this.name = name;
		}

		private void CheckOtherTimeout()
		{
			try
			{
				List<eTimeout> list = new List<eTimeout>();
				foreach (eTimeout timeout in this.mOtherTimers.Keys)
				{
					list.Add(timeout);
				}
				for (int i = 0; i < list.Count; i++)
				{
					DateTime now = DateTime.Now;
					eTimeout timeout2 = list[i];
					double num2 = (now - this.mOtherTimers[timeout2].StartTime).TotalMilliseconds / 1000.0;
					if ((timeout2 == eTimeout.T1 && num2 > this.mT1Interval) || (timeout2 == eTimeout.T2 && num2 > this.mT2Interval) || (timeout2 == eTimeout.T4 && num2 > (double)this.mT3Interval))
					{
						this.logger.Debug(string.Format("SECS1Timer::CheckOtherTimeout: {0}, {1}", num2, timeout2));
						this.mOtherTimers.Remove(timeout2);
						TimerPara para2 = new TimerPara
						{
							Type = timeout2,
							Msg = null
						};
						this.mTimeoutQueue.Enqueue(para2);
					}
					else if (timeout2 != eTimeout.T1 && timeout2 != eTimeout.T2 && timeout2 != eTimeout.T4)
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
				foreach (long num in this.mTimerMsg.Keys)
				{
					list.Add(num);
				}
				for (int i = 0; i < list.Count; i++)
				{
					SECS1Timer.CheckTimerPara para = this.mTimerMsg[list[i]];
					double num2 = (DateTime.Now - para.StartTime).TotalMilliseconds / 1000.0;
					if (num2 > (double)this.mT3Interval)
					{
						this.logger.Debug(string.Format("SECS1Timer::CheckT3Timeout: {0}, {1}", num2, para.SystemBytes));
						this.logger.Info(string.Format("T3 Timeout: {0}, {1}", num2, para.SystemBytes));
						this.mTimerMsg.Remove(list[i]);
						TimerPara para2 = new TimerPara
						{
							Type = eTimeout.T3,
							Msg = para.msg
						};
						this.mTimeoutQueue.Enqueue(para2);
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
						while (this.mTimeoutQueue.Count > 0)
						{
							TimerPara mPara = this.mTimeoutQueue.Dequeue();
							if (mPara.Type == eTimeout.T3 && mPara.Msg != null)
							{
								this.logger.Debug(string.Format("HSMSTimer::Run(): Timeout Message systembyte={0}", mPara.Msg.SystemBytes));
							}
							if (this.OnSECS1Timeout != null)
							{
								this.OnSECS1Timeout(mPara);
							}
						}
					}
				}
				catch (Exception exception)
				{
					this.logger.Error("HSMSTimer::Run ", exception);
				}
				Thread.Sleep(100);
			}
		}

		public void StartT1Timer()
		{
			this.StartTimer(eTimeout.T1);
		}

		public void StartT2Timer()
		{
			this.StartTimer(eTimeout.T2);
		}

		public void StartT3Timer(SECSMessage msg)
		{
			if (msg != null)
			{
				try
				{
					lock (this.syncTimeout)
					{
						SECS1Timer.CheckTimerPara para;
						para.SystemBytes = msg.SystemBytes;
						para.msg = msg;
						para.StartTime = DateTime.Now;
						this.mTimerMsg.Add(para.SystemBytes, para);
					}
					this.logger.Debug(string.Format("Start T3 Timer, SystemBytes={0}", msg.SystemBytes));
				}
				catch (Exception exception)
				{
					this.logger.Error("SECS1Timer::StartT3Timer ", exception);
				}
			}
		}

		public void StartT4Timer()
		{
			this.StartTimer(eTimeout.T4);
		}

		private void StartTimer(eTimeout e)
		{
			try
			{
				lock (this.syncTimeout)
				{
					SECS1Timer.CheckTimerPara para = new SECS1Timer.CheckTimerPara
					{
						SystemBytes = (long)e,
						msg = null,
						StartTime = DateTime.Now
					};
					this.mOtherTimers.Add(e, para);
				}
				if (e != eTimeout.T1)
				{
					this.logger.Debug(string.Format("Start {0} Timer", e));
				}
			}
			catch (Exception exception)
			{
				this.logger.Error("HSMSTimer::StartTimer ", exception);
			}
		}

		public void StopT1Timer()
		{
			this.StopTimer(eTimeout.T1);
		}

		public void StopT2Timer()
		{
			this.StopTimer(eTimeout.T2);
		}

		public void StopT3Timer(SECSMessage msg)
		{
			if (msg != null)
			{
				try
				{
					lock (this.syncTimeout)
					{
						if (this.mTimerMsg.ContainsKey(msg.SystemBytes))
						{
							this.mTimerMsg.Remove(msg.SystemBytes);
						}
					}
					this.logger.Debug(string.Format("Stop T3 Timer, SystemBytes={0}", msg.SystemBytes));
				}
				catch (Exception exception)
				{
					this.logger.Error("SECS1Timer::StopT3Timer ", exception);
				}
			}
		}

		public void StopT4Timer()
		{
			this.StopTimer(eTimeout.T4);
		}

		private void StopTimer(eTimeout e)
		{
			try
			{
				lock (this.syncTimeout)
				{
					if (this.mOtherTimers.ContainsKey(e))
					{
						this.mOtherTimers.Remove(e);
					}
				}
				if (e != eTimeout.T1)
				{
					this.logger.Debug(string.Format("Stop {0} Timer", e));
				}
			}
			catch (Exception exception)
			{
				this.logger.Error("HSMSTimer::StopTimer ", exception);
			}
		}
	}
}
