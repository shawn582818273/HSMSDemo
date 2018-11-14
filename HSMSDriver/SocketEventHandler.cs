using System;
using System.Net.Sockets;

namespace HSMSDriver
{
	internal class SocketEventHandler
	{
		public delegate void ConnectEventHandler(TcpClient socket);

		public delegate void DisconnectEventHandler(string errmsg);

		public delegate void ReadCompleteEventHandler(byte[] readbytes);

		public delegate void ReadErrorEventHandler(string errmsg);

		public delegate void WriteCompleteEventHandler(object sendObj, byte[] sendbytes);

		public delegate void WriteErrorEventHandler(string errmsg);
	}
}
