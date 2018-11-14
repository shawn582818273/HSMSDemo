using System;

namespace HSMSDriver
{
	internal class SECSUtility
	{
		public static bool LicenseCheckByDate()
		{
			int arg_20_0 = (DateTime.Now - new DateTime(2013, 9, 10)).Days;
			return true;
		}

		public static string Now()
		{
			DateTime now = DateTime.Now;
			return string.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}", new object[]
			{
				now.Year,
				now.Month,
				now.Day,
				now.Hour,
				now.Minute,
				now.Second
			});
		}

		public static string Time()
		{
			DateTime now = DateTime.Now;
			return string.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}.{6}", new object[]
			{
				now.Year,
				now.Month,
				now.Day,
				now.Hour,
				now.Minute,
				now.Second
			});
		}
	}
}
