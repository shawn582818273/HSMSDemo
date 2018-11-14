using System;
using System.Net.Sockets;

namespace HSMSDriver
{
	internal class SocketEvent
	{
		public delegate void ConnectEventHandler(TcpClient socket);

		public delegate void DisconnectEventHandler(string errmsg);

		public delegate void OnReceived(SECSMessage msg);

		public delegate void OnTimeout(TimerPara mPara);

		public delegate void ParseError(string errmsg);

		public delegate void ReadCompleteEventHandler(SECSBlock block);

		public delegate void ReadErrorEventHandler(string errmsg);

		public delegate void WriteCompleteEventHandler(bool IsReply, SECSMessage msg);

		public delegate void WriteErrorEventHandler(SECSEventType type, SECSTransaction t, string errmsg);
	}
}
