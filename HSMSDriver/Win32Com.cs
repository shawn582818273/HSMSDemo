using System;
using System.Runtime.InteropServices;

namespace HSMSDriver
{
	internal class Win32Com
	{
		internal const uint CE_BREAK = 16u;

		internal const uint CE_DNS = 2048u;

		internal const uint CE_FRAME = 8u;

		internal const uint CE_IOE = 1024u;

		internal const uint CE_MODE = 32768u;

		internal const uint CE_OOP = 4096u;

		internal const uint CE_OVERRUN = 2u;

		internal const uint CE_PTO = 512u;

		internal const uint CE_RXOVER = 1u;

		internal const uint CE_RXPARITY = 4u;

		internal const uint CE_TXFULL = 256u;

		internal const uint CLRBREAK = 9u;

		internal const uint CLRDTR = 6u;

		internal const uint CLRRTS = 4u;

		internal const uint ERROR_ACCESS_DENIED = 5u;

		internal const uint ERROR_FILE_NOT_FOUND = 2u;

		internal const uint ERROR_INVALID_NAME = 123u;

		internal const uint ERROR_IO_PENDING = 997u;

		internal const uint EV_BREAK = 64u;

		internal const uint EV_CTS = 8u;

		internal const uint EV_DSR = 16u;

		internal const uint EV_ERR = 128u;

		internal const uint EV_EVENT1 = 2048u;

		internal const uint EV_EVENT2 = 4096u;

		internal const uint EV_PERR = 512u;

		internal const uint EV_RING = 256u;

		internal const uint EV_RLSD = 32u;

		internal const uint EV_RX80FULL = 1024u;

		internal const uint EV_RXCHAR = 1u;

		internal const uint EV_RXFLAG = 2u;

		internal const uint EV_TXEMPTY = 4u;

		internal const uint FILE_FLAG_OVERLAPPED = 1073741824u;

		internal const uint GENERIC_READ = 2147483648u;

		internal const uint GENERIC_WRITE = 1073741824u;

		internal const int INVALID_HANDLE_VALUE = -1;

		internal const uint MS_CTS_ON = 16u;

		internal const uint MS_DSR_ON = 32u;

		internal const uint MS_RING_ON = 64u;

		internal const uint MS_RLSD_ON = 128u;

		internal const uint OPEN_EXISTING = 3u;

		internal const uint RESETDEV = 7u;

		internal const uint SETBREAK = 8u;

		internal const uint SETDTR = 5u;

		internal const uint SETRTS = 3u;

		internal const uint SETXOFF = 1u;

		internal const uint SETXON = 2u;

		[DllImport("kernel32.dll")]
		internal static extern bool BuildCommDCBAndTimeouts(string lpDef, ref DCB lpDCB, ref COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		internal static extern bool CancelIo(IntPtr hFile);

		[DllImport("kernel32.dll")]
		internal static extern bool ClearCommError(IntPtr hFile, out uint lpErrors, IntPtr lpStat);

		[DllImport("kernel32.dll")]
		internal static extern bool ClearCommError(IntPtr hFile, out uint lpErrors, out COMSTAT cs);

		[DllImport("kernel32.dll")]
		internal static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

		[DllImport("kernel32.dll")]
		internal static extern bool EscapeCommFunction(IntPtr hFile, uint dwFunc);

		[DllImport("kernel32.dll")]
		internal static extern bool GetCommModemStatus(IntPtr hFile, out uint lpModemStat);

		[DllImport("kernel32.dll")]
		internal static extern bool GetCommProperties(IntPtr hFile, out COMMPROP cp);

		[DllImport("kernel32.dll")]
		internal static extern bool GetCommState(IntPtr hFile, ref DCB lpDCB);

		[DllImport("kernel32.dll")]
		internal static extern bool GetCommTimeouts(IntPtr hFile, out COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		internal static extern bool GetHandleInformation(IntPtr hObject, out uint lpdwFlags);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool GetOverlappedResult(IntPtr hFile, IntPtr lpOverlapped, out uint nNumberOfBytesTransferred, bool bWait);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint nNumberOfBytesRead, IntPtr lpOverlapped);

		[DllImport("kernel32.dll")]
		internal static extern bool SetCommMask(IntPtr hFile, uint dwEvtMask);

		[DllImport("kernel32.dll")]
		internal static extern bool SetCommState(IntPtr hFile, [In] ref DCB lpDCB);

		[DllImport("kernel32.dll")]
		internal static extern bool SetCommTimeouts(IntPtr hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

		[DllImport("kernel32.dll")]
		internal static extern bool SetupComm(IntPtr hFile, uint dwInQueue, uint dwOutQueue);

		[DllImport("kernel32.dll")]
		internal static extern bool TransmitCommChar(IntPtr hFile, byte cChar);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool WaitCommEvent(IntPtr hFile, IntPtr lpEvtMask, IntPtr lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern bool WriteFile(IntPtr fFile, byte[] lpBuffer, uint nNumberOfBytesToWrite, out uint lpNumberOfBytesWritten, IntPtr lpOverlapped);
	}
}
