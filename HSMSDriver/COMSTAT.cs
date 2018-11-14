using System;

namespace HSMSDriver
{
	internal struct COMSTAT
	{
		internal const uint fCtsHold = 1u;

		internal const uint fDsrHold = 2u;

		internal const uint fRlsdHold = 4u;

		internal const uint fXoffHold = 8u;

		internal const uint fXoffSent = 16u;

		internal const uint fEof = 32u;

		internal const uint fTxim = 64u;

		internal uint Flags;

		internal uint cbInQue;

		internal uint cbOutQue;
	}
}
