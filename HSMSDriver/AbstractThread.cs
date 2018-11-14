using log4net;
using System;
using System.Threading;

namespace HSMSDriver
{
	internal abstract class AbstractThread
	{
		protected ILog logger;

		protected bool running;

		protected Thread thread;

		public virtual ILog Logger
		{
			set
			{
				this.logger = value;
			}
		}

		public abstract string Name
		{
			get;
		}

		public ThreadState ThreadState
		{
			get
			{
				return this.thread.ThreadState;
			}
		}

		protected AbstractThread()
		{
			this.thread = new Thread(new ThreadStart(this.ThreadFunc))
			{
				IsBackground = true
			};
		}

		protected abstract void Run();

		public virtual void Start()
		{
			this.logger.Debug(string.Format("Start {0} Thread.", this.Name));
			this.thread.Name = this.Name;
			this.running = true;
			this.thread.Start();
		}

		public virtual void Stop()
		{
			this.logger.Debug(string.Format("Terminate {0} Thread.", this.Name));
			this.running = false;
		}

		protected void ThreadFunc()
		{
			this.logger.Debug(string.Format("{0} Thread Status = {1}", this.Name, this.running));
			this.Run();
			this.logger.Debug(string.Format("{0} Thread Status = {1}", this.Name, this.running));
		}
	}
}
