using System;

namespace HSMSDriver
{
	internal enum eHSMS_PORT_STATUS
	{
		CONNECT,
		CONNECTING = -4,
		DISCONNECT = -1,
		SELECT = 1,
		TERMINATE = -2,
		UNKNOWN = -3
	}
}
