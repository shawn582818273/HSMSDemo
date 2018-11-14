using System;

namespace HSMSDriver
{
	internal class SECSBlock
	{
		public byte[] CheckSum;

		public byte[] DataItem;

		public byte[] Header;

		public bool IsControlMsg;

		public int Length;
	}
}
