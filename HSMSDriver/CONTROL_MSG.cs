using System;

namespace HSMSDriver
{
	internal enum CONTROL_MSG
	{
		Deselect_Request = 3,
		Deselect_Response,
		Linktest_Request,
		Linktest_Response,
		Reject_Request,
		Select_Request = 1,
		Select_Response,
		Separate_Request = 9
	}
}
