using System;
using System.IO;

namespace HSMSDriver
{
	internal class CommLineSettings : CommBaseSettings
	{
		public int rxStringBufferSize = 256;

		public ASCII rxTerminator = ASCII.CR;

		public ASCII[] rxFilter;

		public int transactTimeout = 500;

		public ASCII[] txTerminator;

		public new static CommLineSettings LoadFromXML(Stream s)
		{
			return (CommLineSettings)CommBaseSettings.LoadFromXML(s, typeof(CommLineSettings));
		}
	}
}
