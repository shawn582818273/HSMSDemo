using System;

namespace HSMSDriver
{
	internal struct DCB
	{
		internal int DCBlength;

		internal int BaudRate;

		internal int PackedValues;

		internal short wReserved;

		internal short XonLim;

		internal short XoffLim;

		internal byte ByteSize;

		internal byte Parity;

		internal byte StopBits;

		internal byte XonChar;

		internal byte XoffChar;

		internal byte ErrorChar;

		internal byte EofChar;

		internal byte EvtChar;

		internal short wReserved1;

		internal void init(bool parity, bool outCTS, bool outDSR, int dtr, bool inDSR, bool txc, bool xOut, bool xIn, int rts)
		{
			this.DCBlength = 28;
			this.PackedValues = 32769;
			if (parity)
			{
				this.PackedValues |= 2;
			}
			if (outCTS)
			{
				this.PackedValues |= 4;
			}
			if (outDSR)
			{
				this.PackedValues |= 8;
			}
			this.PackedValues |= (dtr & 3) << 4;
			if (inDSR)
			{
				this.PackedValues |= 64;
			}
			if (txc)
			{
				this.PackedValues |= 128;
			}
			if (xOut)
			{
				this.PackedValues |= 256;
			}
			if (xIn)
			{
				this.PackedValues |= 512;
			}
			this.PackedValues |= (rts & 3) << 12;
		}
	}
}
