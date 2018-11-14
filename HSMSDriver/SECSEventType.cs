using System;

namespace HSMSDriver
{
	public enum SECSEventType
	{
		PrimarySent,
		PrimaryRcvd,
		SecondarySent,
		SecondaryRcvd,
		HSMSConnected,
		HSMSDisconnected,
		Error,
		Warn
	}
}
