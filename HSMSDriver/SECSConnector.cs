using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace HSMSDriver
{
	internal class SECSConnector : AbstractThread
	{
		protected bool reconnect;

		protected SocketInfo socketInfo;

		public event SocketEventHandler.ConnectEventHandler OnConnected;

		public event SocketEvent.ReadErrorEventHandler OnReadError;

		public override string Name
		{
			get
			{
				return string.Format("{0}-{1} Connector", this.socketInfo.DriverName, this.socketInfo.Protocol);
			}
		}

		public bool Reconnect
		{
			get
			{
				return this.reconnect;
			}
			set
			{
				this.reconnect = value;
			}
		}

		public SECSConnector(string loggername, SocketInfo socketInfo)
		{
			this.socketInfo = socketInfo;
		}

		public void OpenActiveConnection()
		{
			try
			{
				IPEndPoint point = new IPEndPoint(IPAddress.Parse(this.socketInfo.IpAddress), int.Parse(this.socketInfo.Port));
				TcpClient socket = new TcpClient();
				socket.Connect(point);
				if (this.socketInfo.Timeout == 0)
				{
					socket.ReceiveTimeout = 6000;
					socket.SendTimeout = 6000;
				}
				else
				{
					socket.ReceiveTimeout = this.socketInfo.Timeout + 1000;
					socket.SendTimeout = this.socketInfo.Timeout + 1000;
				}
				if (this.OnConnected == null || !this.running)
				{
					socket.Close();
				}
				else
				{
					this.running = false;
					this.OnConnected(socket);
				}
				this.running = false;
			}
			catch (Exception exception)
			{
				if (this.running)
				{
					this.logger.Error(exception.Message, exception);
					if (this.OnReadError != null)
					{
						this.OnReadError(string.Format("{0}: {1}.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ReadError), exception.Message));
					}
				}
			}
		}

		private void OpenPassiveConnection()
		{
			try
			{
				TcpListener listener = new TcpListener(IPAddress.Parse(this.socketInfo.IpAddress), int.Parse(this.socketInfo.Port));
				listener.Start();
				TcpClient socket = listener.AcceptTcpClient();
				listener.Stop();
				this.OnConnected(socket);
				this.running = false;
			}
			catch (Exception exception)
			{
				this.logger.Error(exception.Message);
				if (this.OnReadError != null)
				{
					this.OnReadError(string.Format("{0}: {1}.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ReadError), exception.Message));
				}
			}
		}

		protected override void Run()
		{
			while (this.running)
			{
				try
				{
					if (this.Reconnect)
					{
						Thread.Sleep(this.socketInfo.ConnectInterval);
						if (this.socketInfo.IsActiveMode)
						{
							this.OpenActiveConnection();
						}
						else
						{
							this.OpenPassiveConnection();
						}
					}
					else
					{
						if (this.socketInfo.IsActiveMode)
						{
							this.OpenActiveConnection();
						}
						else
						{
							this.OpenPassiveConnection();
						}
						if (this.running)
						{
							Thread.Sleep(this.socketInfo.ConnectInterval);
						}
					}
				}
				catch (ThreadInterruptedException exception)
				{
					this.logger.Error("Connect Trace : " + exception.ToString());
					if (this.OnReadError != null)
					{
						this.OnReadError(string.Format("{0}: {1}.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ReadError), exception.Message));
					}
					break;
				}
				catch (Exception exception2)
				{
					this.logger.Error("CONNECT Error", exception2);
					if (this.OnReadError != null)
					{
						this.OnReadError(string.Format("{0}: {1}.", SECSErrorsMessage.GetSECSErrorMessage(SECSErrors.ReadError), exception2.Message));
					}
				}
			}
		}
	}
}
