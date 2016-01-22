using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    static class config
    {

        /* General Config */
        public const String BOOT_ENTRY_TEXT = "Android-OS";
        public const long   ANDROID_SYSTEM_SIZE = 2147483648; /* Disk should have at least 2GB Free space for System ONLY */
        /* OS-Dir Config */
        public const String INSTALL_FOLDER = "AndroidOS";
        public const String INSTALL_DIR = @"{0}:\" + INSTALL_FOLDER;
        
        /* UEFI Config */
        public const String UEFI_DIR = @"\EFI\Android\";
        public const String UEFI_PARTITION_MOUNTPOINT = "Z:";
        public const String UEFI_GRUB_BIN64 = "grubx64.efi";
        public const String UEFI_GRUB_BIN32 = "grubia32.efi";
        public const String UEFI_GRUB_CONFIG = "grub.cfg";
        public const String UEFI_GRUB_RX_CONFIG = "grub_remix.cfg";
        public static bool RemixOS_Found = false;

        /* Log file */
        public const string LOG_FILE_PATH = @"C:\AndroidInstall_{0}.log";

        /* Win32UEFI Library */
        public const string UEFI_LIB_PATH = "Win32UEFI.dll";
        
    }
}
