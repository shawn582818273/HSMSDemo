using System;

namespace HSMSDriver
{
	public enum SECSErrors
	{
		None,
		T1TimeOut,
		T2TimeOut,
		T3TimeOut,
		T4TimeOut,
		T5TimeOut,
		T6TimeOut,
		T7TimeOut,
		T8TimeOut,
		RcvdNAK,
		RcvdAbortMessage,
		RcvdUnknownMessage,
		PortNotOpen,
		PortNotConnected,
		WriteError,
		ReadError,
		ParseError,
		UnrecognizedDeviceID,
		UnrecognizedStreamType,
		UnrecognizedFunctionType,
		IllegalData,
		ConversationTimeout
	}
}
