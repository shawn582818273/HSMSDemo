using System;
using System.Runtime.InteropServices;

namespace HSMSDriver
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct SECSFormat2
	{
		public const int ASCII = 16;

		public const int BINARY = 8;

		public const int BOOLEAN = 9;

		public const int CHAR2 = 18;

		public const int F4 = 36;

		public const int F8 = 32;

		public const int I1 = 25;

		public const int I2 = 26;

		public const int I4 = 28;

		public const int I8 = 24;

		public const int JIS8 = 17;

		public const int LIST = 0;

		public const int U1 = 41;

		public const int U2 = 42;

		public const int U4 = 44;

		public const int U8 = 40;

		public static string Format2Str(int fmt)
		{
			if (fmt <= 9)
			{
				if (fmt == 0)
				{
					return "L";
				}
				switch (fmt)
				{
				case 8:
					return "B";
				case 9:
					return "BOOLEAN";
				}
			}
			else
			{
				switch (fmt)
				{
				case 16:
					return "A";
				case 17:
					return "JIS8";
				case 18:
					return "CHAR2";
				case 19:
				case 20:
				case 21:
				case 22:
				case 23:
				case 27:
					break;
				case 24:
					return "I8";
				case 25:
					return "I1";
				case 26:
					return "I2";
				case 28:
					return "I4";
				default:
					if (fmt == 32)
					{
						return "F8";
					}
					switch (fmt)
					{
					case 36:
						return "F4";
					case 40:
						return "U8";
					case 41:
						return "U1";
					case 42:
						return "U2";
					case 44:
						return "U4";
					}
					break;
				}
			}
			return "UNKNOWN";
		}

		public static int Str2Format(string fmt)
		{
			if (fmt == "A")
			{
				return 16;
			}
			if (fmt == "L")
			{
				return 0;
			}
			if (fmt == "B")
			{
				return 8;
			}
			if (fmt == "BOOLEAN")
			{
				return 9;
			}
			if (fmt == "F4")
			{
				return 36;
			}
			if (fmt == "F8")
			{
				return 32;
			}
			if (fmt == "U1")
			{
				return 41;
			}
			if (fmt == "U2")
			{
				return 42;
			}
			if (fmt == "U4")
			{
				return 44;
			}
			if (fmt == "U8")
			{
				return 40;
			}
			if (fmt == "I1")
			{
				return 25;
			}
			if (fmt == "I2")
			{
				return 26;
			}
			if (fmt == "I4")
			{
				return 28;
			}
			if (fmt == "I8")
			{
				return 24;
			}
			if (fmt == "CHAR2")
			{
				return 18;
			}
			if (fmt == "JIS8")
			{
				return 17;
			}
			return -1;
		}
	}
}
