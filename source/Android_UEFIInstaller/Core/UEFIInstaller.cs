using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    class UEFIInstaller : BasicInstaller
    {
        enum ErrorCodes
        {
            MOUNT_EFI_PARTITION,
            INSTALL_BOOT,
            UEFI_ENTRY
        }
        
            /* 
             * 
             * Create dirs
             * Install Android Files
             * Make data.img
             * Install Boot files
             * Create UEFI Entry
             * cleanup temp data
             * 
             * #####
             * Folders:
             *           :\Android
             *          Z:\EFI\Android
             * Files:
             *           :\Android\kernel    
             *           :\Android\initrd
             *           :\Android\ramdisk.img
             *           :\Android\system.img
             *           :\Android\data.img
             *           
             * 
             *           :\EFI\Android\grubx64.efi
             *           :\EFI\Android\grub.cfg
             */


        protected override bool InstallBootObjects(Object extraData)
        {
            String EFI_DIR = config.UEFI_PARTITION_MOUNTPOINT + config.UEFI_DIR;
            Log.write("===Installing Boot Objects===");

            if (!MountFirmwarePartition())
                return false;

            if (!CreateBootDirectory(EFI_DIR))
                return false;

            if (!CopyBootFiles(EFI_DIR))
                return false;


            if (!CreateUEFIBootOption(config.UEFI_PARTITION_MOUNTPOINT))
                return false;
                

            if (!UnMountFirmwarePartition())
                return false;

            return true;
        }

        protected override bool UnInstallBootObjects(Object extraData)
        {

            Log.write("===Removing Boot Objects===");
            MountFirmwarePartition();
            if (UEFIWrapper.UEFI_Init())
            {
                Log.write("-Remove Android UEFI Entry");
                int ret = UEFIWrapper.UEFI_DeleteBootOptionByDescription(config.BOOT_ENTRY_TEXT);
                Log.write("-UEFI: " + ret);
            }
            else
            {
                Log.write("-UEFI Init ... fail");
            }
            base.cleanup(config.UEFI_PARTITION_MOUNTPOINT + config.UEFI_DIR);
            UnMountFirmwarePartition();

            return true;
        }

        private bool MountFirmwarePartition()
        {
            Log.updateStatus("Mounting EFI Partition...");
            Log.write("-Mounting EFI Partition...");
            
            string MOUNT_EXE = @"C:\Windows\System32\mountvol.exe";
            string MOUNT_CMD = String.Format(" Z: /S");

            
            if (!ExecuteCLICommand(MOUNT_EXE, MOUNT_CMD))
            {
                return false;
            }

            return true;
        }

        private bool UnMountFirmwarePartition()
        {
            Log.updateStatus("UnMounting EFI Partition...");
            Log.write("-UnMounting EFI Partition...");
            string UNMOUNT_EXE = @"C:\Windows\System32\mountvol.exe";
            string UNMOUNT_CMD = String.Format(" Z: /D");

            if (!ExecuteCLICommand(UNMOUNT_EXE,UNMOUNT_CMD))
            {
                return false;
            }

            return true;
        }


        private Boolean CreateBootDirectory(string directory)
        {
            
            Log.write("-Setup Boot Directory...");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Log.write("-Boot Folder Created: " + directory);
            }
            else
            {
                Log.write("-Boot Directory is Already Exist");
                return false;
            }

            return true;
        }

        private Boolean CopyBootFiles(String directory)
        {
            Log.write("-Copy Boot files");
            try
            {
                if (Environment.Is64BitOperatingSystem)
                    File.Copy(Environment.CurrentDirectory + @"\" + config.UEFI_GRUB_BIN64, directory + @"\" + config.UEFI_GRUB_BIN64, false);
                else
                    File.Copy(Environment.CurrentDirectory + @"\" + config.UEFI_GRUB_BIN32, directory + @"\" + config.UEFI_GRUB_BIN32, false);
                
                if (!config.RemixOS_Found)
                    File.Copy(Environment.CurrentDirectory + @"\" + config.UEFI_GRUB_CONFIG, directory + @"\" + config.UEFI_GRUB_CONFIG, false);    //Android-x86
                else
                    File.Copy(Environment.CurrentDirectory + @"\" + config.UEFI_GRUB_RX_CONFIG, directory + @"\" + config.UEFI_GRUB_CONFIG, false);    //RemixOS

                return true;
            }
            catch (Exception ex)
            {
                Log.write(ex.Message);
                return false;
            }
            
        }

        private Boolean CreateUEFIBootOption(String Drive)
        {
            String _Drive = String.Format(@"\\.\{0}",Drive);

            Log.write("-Add UEFI Entry");
            
            if (!UEFIWrapper.UEFI_Init())
            {
                Log.write("UEFI Init Fail");
                return false;
            }

            if (Environment.Is64BitOperatingSystem)
            {

                if (!UEFIWrapper.UEFI_MakeMediaBootOption(config.BOOT_ENTRY_TEXT, _Drive, config.UEFI_DIR + config.UEFI_GRUB_BIN64))
                {
                    Log.write("UEFI 64-bit Entry Fail");
                    return false;
                }
            }
            else
            {
                if (!UEFIWrapper.UEFI_MakeMediaBootOption(config.BOOT_ENTRY_TEXT, _Drive, config.UEFI_DIR + config.UEFI_GRUB_BIN32))
                {
                    Log.write("UEFI 32-bit Entry Fail");
                    return false;
                }
            }
            return true;
        }
    }
}
