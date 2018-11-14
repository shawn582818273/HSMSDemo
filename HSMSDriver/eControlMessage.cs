using System;

namespace HSMSDriver
{
	internal enum eControlMessage
	{
		DESELECT_REQ = 3,
		DESELECT_RSP,
		LINKTEST_REQ,
		LINKTEST_RSP,
		NORMAL = 0,
		REJECT = 7,
		SELECT_REQ = 1,
		SELECT_RSP,
		SEPARATE = 9
	}
}
