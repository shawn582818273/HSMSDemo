using System;

namespace HSMSDriver
{
	internal class MsgType
	{
		public const int Aborted = 0;

		public const int Disconnected = 1;

		public const int InvalidDeviceID = 2;

		public const int InvalidSecondary = 7;

		public const int Received = 3;

		public const int Selected = 4;

		public const int Sent = 8;

		public const int Timeout = 5;

		public const int Unkown = 6;
	}
}
