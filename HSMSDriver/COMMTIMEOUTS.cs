using System;

namespace HSMSDriver
{
	internal struct COMMTIMEOUTS
	{
		internal int ReadIntervalTimeout;

		internal int ReadTotalTimeoutMultiplier;

		internal int ReadTotalTimeoutConstant;

		internal int WriteTotalTimeoutMultiplier;

		internal int WriteTotalTimeoutConstant;
	}
}
