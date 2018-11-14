using System;

namespace HSMSDriver
{
	internal struct COMMPROP
	{
		internal ushort wPacketLength;

		internal ushort wPacketVersion;

		internal uint dwServiceMask;

		internal uint dwReserved1;

		internal uint dwMaxTxQueue;

		internal uint dwMaxRxQueue;

		internal uint dwMaxBaud;

		internal uint dwProvSubType;

		internal uint dwProvCapabilities;

		internal uint dwSettableParams;

		internal uint dwSettableBaud;

		internal ushort wSettableData;

		internal ushort wSettableStopParity;

		internal uint dwCurrentTxQueue;

		internal uint dwCurrentRxQueue;

		internal uint dwProvSpec1;

		internal uint dwProvSpec2;

		internal byte wcProvChar;
	}
}
