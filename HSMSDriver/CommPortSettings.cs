using System;
using System.IO;

namespace HSMSDriver
{
	internal class CommPortSettings : CommBaseSettings
	{
		public bool breakLineOnChar;

		public int charsInLine;

		public ASCII lineBreakChar;

		public bool showAsHex;

		public new static CommPortSettings LoadFromXML(Stream s)
		{
			return (CommPortSettings)CommBaseSettings.LoadFromXML(s, typeof(CommPortSettings));
		}
	}
}
