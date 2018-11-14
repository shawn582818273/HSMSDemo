using System;

namespace HSMSDriver
{
	internal struct OVERLAPPED
	{
		internal UIntPtr Internal;

		internal UIntPtr InternalHigh;

		internal uint Offset;

		internal uint OffsetHigh;

		internal IntPtr hEvent;
	}
}
