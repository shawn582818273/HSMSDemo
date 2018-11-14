using System;

namespace HSMSDriver
{
	internal class CommPortException : ApplicationException
	{
		public CommPortException(Exception e) : base("Receive Thread Exception", e)
		{
		}

		public CommPortException(string desc) : base(desc)
		{
		}
	}
}
