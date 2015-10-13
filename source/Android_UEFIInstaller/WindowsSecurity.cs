using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
	class WindowsSecurity
	{
		private const long ERROR_NOT_ALL_ASSIGNED = 1300;
		private const string NTSecurityPrivilege = "SeSystemEnvironmentPrivilege";
		private static Win32Native.LUID luid;
		private static Win32Native.TOKEN_PRIVILEGES tp;
		private static Win32Native.TOKEN_PRIVILEGES tp2;
		private static IntPtr hToken;

		public Boolean GetAccesstoNVRam()
		{
			

			if(! Win32Native.OpenProcessToken(Process.GetCurrentProcess().Handle,Win32Native.TOKEN_QUERY | Win32Native.TOKEN_ADJUST_PRIVILEGES,out hToken))
			{
				return false;
			}

			if (!EnablePrivilege(hToken, NTSecurityPrivilege))
			{
				return false;
			}

			return true;
		}

		Boolean EnablePrivilege(IntPtr HANDLE,string lpszPrivilege)
		{
			if (!Win32Native.LookupPrivilegeValue(null,lpszPrivilege,out luid))
			{
				return false;
			}

			tp.PrivilegeCount = 1;
			tp.Luid = luid;
			tp.Attributes = Win32Native.SE_PRIVILEGE_ENABLED;

			uint retlen;
			uint buflen = (uint)System.Runtime.InteropServices.Marshal.SizeOf(tp2);
			//if (!Win32Native.AdjustTokenPrivileges(HANDLE, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero))
			if (!Win32Native.AdjustTokenPrivileges(HANDLE, false, ref tp, buflen, ref tp2, out retlen))
			{
				return false;
			}

			if (System.Runtime.InteropServices.Marshal.GetLastWin32Error() != ERROR_NOT_ALL_ASSIGNED)
			{
				var win32Exception = new System.ComponentModel.Win32Exception();
				//throw new InvalidOperationException("AdjustTokenPrivileges failed.", win32Exception);
				return false;
			}

			return true;
		}
	}

}
