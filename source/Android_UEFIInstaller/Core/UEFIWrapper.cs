using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    static class UEFIWrapper
    {

        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UEFI_Init();
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr UEFI_GetBootList();
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr[][] UEFI_GetBootDevices();
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UEFI_isUEFIAvailable();
        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool UEFI_MakeMediaBootOption([MarshalAsAttribute(UnmanagedType.LPWStr)]String Description, 
                                                           [MarshalAsAttribute(UnmanagedType.LPWStr)] String DiskLetter, 
                                                           [MarshalAsAttribute(UnmanagedType.LPWStr)] String Path);

        [DllImport(@"Win32UEFI.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I4)]
        public static extern int UEFI_DeleteBootOptionByDescription([MarshalAsAttribute(UnmanagedType.LPWStr)]String Description);
        /*
        [DllImport(@"Win32UEFI.dll")]
        void UEFI_MakeMediaBootOption(WCHAR* Description, WCHAR* DiskLetter, WCHAR* Path);
        
        EFI_BOOT_ORDER* UEFI_GetBootList();
        BDS_LOAD_OPTION** UEFI_GetBootDevices();
        int UEFI_GetBootCount();
        
        
        */
    }
}
