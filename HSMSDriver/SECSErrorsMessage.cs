using System;
using System.Collections.Generic;

namespace HSMSDriver
{
	internal class SECSErrorsMessage
	{
		private static Dictionary<SECSErrors, string> errorMessageDict;

		static SECSErrorsMessage()
		{
			SECSErrorsMessage.errorMessageDict = new Dictionary<SECSErrors, string>
			{
				{
					SECSErrors.None,
					""
				},
				{
					SECSErrors.T1TimeOut,
					"T1 TimeOut."
				},
				{
					SECSErrors.T2TimeOut,
					"T2 TimeOut."
				},
				{
					SECSErrors.T3TimeOut,
					"T3 TimeOut: Reply Timeout."
				},
				{
					SECSErrors.T4TimeOut,
					"T4 TimeOut."
				},
				{
					SECSErrors.T5TimeOut,
					"T5 TimeOut: Connect Separation Timeout."
				},
				{
					SECSErrors.T6TimeOut,
					"T6 TimeOut: Control Timeout."
				},
				{
					SECSErrors.T7TimeOut,
					"T7 TimeOut: Connection Idle Timeout."
				},
				{
					SECSErrors.T8TimeOut,
					"T8 TimeOut."
				},
				{
					SECSErrors.RcvdNAK,
					"Rcvd NAK."
				},
				{
					SECSErrors.RcvdAbortMessage,
					"Received Abort Message."
				},
				{
					SECSErrors.RcvdUnknownMessage,
					"Received  Unknown Message."
				},
				{
					SECSErrors.PortNotConnected,
					"Communication Port Not Connected."
				},
				{
					SECSErrors.PortNotOpen,
					"Communication Port Not Opened."
				},
				{
					SECSErrors.WriteError,
					"Send Error"
				},
				{
					SECSErrors.ReadError,
					"Read Error"
				},
				{
					SECSErrors.ParseError,
					"Parse Error"
				},
				{
					SECSErrors.UnrecognizedDeviceID,
					"Received Invalid DeviceID: Unrecognized Device ID."
				},
				{
					SECSErrors.UnrecognizedStreamType,
					"Received Invalid Message: Unrecognized Stream Type."
				},
				{
					SECSErrors.UnrecognizedFunctionType,
					"Received Invalid Message: Unrecognized Function Type."
				},
				{
					SECSErrors.IllegalData,
					"Received Invalid Message: Illegal Data."
				},
				{
					SECSErrors.ConversationTimeout,
					"Unreceived Conversation Message: Conversation Timeout."
				}
			};
		}

		public static string GetSECSErrorMessage(SECSErrors err)
		{
			if (SECSErrorsMessage.errorMessageDict.ContainsKey(err))
			{
				return SECSErrorsMessage.errorMessageDict[err];
			}
			return "";
		}
	}
}
