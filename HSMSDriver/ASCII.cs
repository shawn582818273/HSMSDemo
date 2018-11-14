using System;

namespace HSMSDriver
{
	internal enum ASCII : byte
	{
		ACK = 6,
		BELL,
		BS,
		CAN = 24,
		CR = 13,
		DC1 = 17,
		DC2,
		DC3,
		DC4,
		DEL = 127,
		EM = 25,
		ENQ = 5,
		EOT = 4,
		ESC = 27,
		ETB = 23,
		ETX = 3,
		FF = 12,
		FS = 28,
		GS,
		HT = 9,
		LF,
		NAK = 21,
		NULL = 0,
		RS = 30,
		SI = 15,
		SO = 14,
		SOH = 1,
		SP = 32,
		STH = 2,
		SUB = 26,
		SYN = 22,
		US = 31,
		VT = 11
	}
}
