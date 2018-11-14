using System;

namespace HSMSDriver
{
	internal class SECS1EventArgs : EventArgs
	{
		public SECSErrors ErrorCode;

		public string ErrorMsg;

		public SECSEventType EventType;

		public SECSTransaction Trans;
	}
}
