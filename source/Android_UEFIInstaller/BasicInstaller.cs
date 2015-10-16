using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Android_UEFIInstaller
{
    enum InstallationStep
    {
        CREATE_DIRECTORIES,
        EXTRACT_ISO,
        EXTRACT_SFS,
        CREATE_DATA,
        FORMAT_DATA,
        INSTALL_BOOT,
        /*
        MOUNT_EFI_PARTITION,
        INSTALL_BOOT,
        UEFI_ENTRY,
         */
        REVERT_ALL
        
    }
    abstract class BasicInstaller
    {

        PrivilegeClass.Privilege FirmwarePrivilege;
        public BasicInstaller()
        {
            FirmwarePrivilege = new PrivilegeClass.Privilege("SeSystemEnvironmentPrivilege");
        }

        public virtual Boolean Install(String ISOFilePath, String InstallDrive, String UserDataSize)
        {
            String InstallDirectory = String.Format(config.INSTALL_DIR, InstallDrive);
            Log.write(String.Format("====Install Started on {0}====", DateTime.Now));
            Log.write("-ISO File: " + ISOFilePath);
            Log.write("-TargetDrive: " + InstallDrive);
            Log.write("-UserData: " + UserDataSize);

            String OtherInstall = SearchForPreviousInstallation(config.INSTALL_FOLDER);
            if ( OtherInstall != "0")
            {
                Log.write("Another Installation found on: " + OtherInstall + @":\");
                return false;
            }

            if (!SetupDirectories(InstallDirectory))
                return false;

            if (!ExtractISO(ISOFilePath, InstallDirectory))
                goto cleanup;

            String[] FileList = {InstallDirectory + @"\kernel",
                                InstallDirectory + @"\initrd.img",
                                InstallDirectory + @"\ramdisk.img",
                                InstallDirectory + @"\system.img",
                                };
            if (!VerifyFiles(FileList))
                goto cleanup;

            if (!CreateDataParition(InstallDirectory, UserDataSize))
                goto cleanup;

            if (!FormatDataPartition(InstallDirectory))
                goto cleanup;

            
            if (!InstallBootObjects(null))
                goto cleanup;

            Log.write("==========================================");
            return true;

        cleanup:
            Log.write("==============Revert Installation==============");
            cleanup(InstallDirectory);
            UnInstallBootObjects(null);
            Log.write("==========================================");
            return false;
        }

        public void Uninstall(String InstallDrive="0")
        {
            String InstallDirectory = String.Format(config.INSTALL_DIR, InstallDrive);

            Log.write(String.Format("====Uninstall Started on {0}====", DateTime.Now));

            InstallDrive = SearchForPreviousInstallation(config.INSTALL_FOLDER);
            if (InstallDrive != "0")
            {
                cleanup(String.Format(config.INSTALL_DIR,InstallDrive));
            }
            else
            {
                Log.write("Android Installation Not Found");
            }
            FirmwarePrivilege.Enable();
            UnInstallBootObjects(null);
            FirmwarePrivilege.Revert();
            Log.write("==========================================");
        }

        String SearchForPreviousInstallation(String FolderName)
        {
            String[] drives = Environment.GetLogicalDrives();

            foreach (String drive in drives)
            {
                if (Directory.Exists(drive + FolderName))
                {
                    return drive.Substring(0, 1);
                }
            }

            return "0";
        }

        private Boolean SetupDirectories(String directory)
        {
            Log.write("-Setup Directories...");
            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Log.write("-Folder Created: " + directory);
                    return true;
                }
                else
                {
                    
                    Log.write(directory + " Already Exists");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.write("Error Creating OS folders:" + ex.Message.ToString() + "Dir:" + directory);
                return false;
            }
        }

        #region "ISO Extraction"
        private Boolean ExtractISO(String ISOFilePath, String ExtractDirectory)
        {
            //7z.exe x android-x86-4.4-r2.img "efi" "kernel" "ramdisk.img" "initrd.img" "system.sfs" -o"C:\Users\ExtremeGTX\Desktop\installer_test\extracted\"
            string ExecutablePath = Environment.CurrentDirectory + @"\7z.exe";
            string ExecutableArgs = String.Format("x {0} \"kernel\" \"ramdisk.img\" \"initrd.img\" \"system.sfs\" -o{1}", ISOFilePath, ExtractDirectory);    //{0} ISO Filename, {1} extraction dir
            //
            //Extracting ISO Contents
            //
            Log.updateStatus("Status: Extract ISO... Please wait");
            Log.write("-Extract ISO");
            if (!ExecuteCLICommand(ExecutablePath, ExecutableArgs))
                return false;

            //
            //Extracting System.sfs
            //
            Log.updateStatus("Status: Extract SFS... Please wait");
            Log.write("-Extract SFS");
            ExecutableArgs = String.Format(" x {0}\\system.sfs \"system.img\" -o{0}", ExtractDirectory);
            if (!ExecuteCLICommand(ExecutablePath, ExecutableArgs))
                return false;

            return true;
        }
        #endregion
        
        #region "Data Partition"
        private Boolean CreateDataParition(String directory,String Size)
        {
            
            Log.updateStatus("Status: Create Data.img... Please wait");
            Log.write("-Create Data.img");

            string ExecutablePath = Environment.CurrentDirectory + @"\dd.exe";
            string ExecutableArgs = String.Format(@"if=/dev/zero of={0}\data.img count={1}", directory, Size.ToString());

            if (!ExecuteCLICommand(ExecutablePath, ExecutableArgs))
                return false;

            return true;

        }

        private Boolean FormatDataPartition(String FilePath)
        {
            Log.updateStatus("Status: initialize Data.img... Please wait");
            Log.write("-Initialize Data.img");
            string ExecutablePath = Environment.CurrentDirectory + @"\mke2fs.exe";
            string ExecutableArgs = String.Format("-F -t ext4 \"{0}\\data.img\"", FilePath);

            if (!ExecuteCLICommand(ExecutablePath, ExecutableArgs))
                return false;

            return true;
        }
        #endregion

        private Boolean VerifyFiles(String[] FileList)
        {
            foreach (String file in FileList)
            {
                if (!File.Exists(file))
                {
                    Log.write("File: " + file + " not exist");
                    return false;
                }
            }

            return true;
        }

        protected abstract Boolean InstallBootObjects(Object extraData);
        protected abstract Boolean UnInstallBootObjects(Object extraData);

    
        protected virtual bool cleanup(String directory)
        {
            Log.write("-Cleaning up Android Directory ... " + directory);
            try
            {
                //Check if Directory Exist
                if (Directory.Exists(directory))
                {
                   Directory.Delete(directory, true);
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.write("Exception: " + ex.Message);
                return false;
            }

        }
        /*
        protected void revert(InstallationStep step, Object info)
        {
            switch (step)
            {
                case InstallationStep.REVERT_ALL:
                case InstallationStep.INSTALL_BOOT:
                    UnInstallBootObjects((int)info);
                    goto case InstallationStep.FORMAT_DATA;

                case InstallationStep.FORMAT_DATA:
                case InstallationStep.CREATE_DATA:
                case InstallationStep.EXTRACT_SFS:
                case InstallationStep.EXTRACT_ISO:
                    String iso = info as String;
                    //Log.write("Error: ISO Extraction failed > " + iso);
                    //Directory.Delete(InstallDirectory, true);
                   // Directory.EnumerateFileSystemEntries(InstallDirectory).Any();
                    break;

                case InstallationStep.CREATE_DIRECTORIES:
                    String dir = info as String;
                    Log.write("Error: Folder Exist > " + dir);
                    //System.Windows.MessageBox.Show(dir + " Already Exist\n" + "Installation Process will Stop", "Error", System.Windows.forms.MessageBoxButtons.OK);
                    break;

                default:
                    break;
            }
        }
        */
        protected Boolean ExecuteCLICommand(String FilePath, String args)
        {
            string CliExecutable = FilePath;
            string CliArguments = args;
            try
            {

                Process p = new Process();
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                Log.write("#Launch:" + CliExecutable + CliArguments);
                p.StartInfo.FileName = CliExecutable;
                p.StartInfo.Arguments = CliArguments;
                p.Start();
                p.WaitForExit();

                if (p.ExitCode != 0)
                {
                    Log.write(String.Format("Error Executing {0} with Args: {1}", FilePath, args));
                    Log.write("Error output:");
                    Log.write(p.StandardError.ReadToEnd());
                    Log.write(p.StandardOutput.ReadToEnd());
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.write("Exception: " + ex.Message);
                return false;
            }
        }
    }
}
