using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Android_UEFIInstaller
{
    /// <summary>
    /// Interaction logic for AndroidMaterial.xaml
    /// </summary>
    public partial class AndroidMaterial : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

        WindowsSecurity ws = new WindowsSecurity();
        IntPtr Handle;
        public AndroidMaterial()
        {
            InitializeComponent();
            DateTime d = new DateTime(2015, 11, 6);
            if (d <= DateTime.Today)
            { 
                MessageBox.Show("This is an expired alpha testing version\nPlease check for the latest release, Application will exit ");
                Environment.Exit(0);
            }
            //
            //Update Version
            //
            txtVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //
            //Setup TxtLog for logging
            //
            Log.SetLogBuffer(txtlog);
            //
            //SetupGlobalExceptionHandler
            //
            SetupGlobalExceptionHandler();
            //
            //Log Some Info
            //
            Log.write("================Installer Info================");
            Log.write("Installer Directory:" + Environment.CurrentDirectory);
            Log.write("Installer Version:" + System.Reflection.Assembly.GetExecutingAssembly()
                                            .GetName()
                                            .Version
                                            .ToString());
            //
            // Machine Info
            //
            GetMachineInfo();
            //
            // Check if Requirements satisifed
            //
            if (!RequirementsCheck())
            {
                DisableUI();
                MessageBox.Show("Not all system requirements are met\nPlease check the installer log");
            }

            Log.write("==========================================");
        }

        public void SetupGlobalExceptionHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(MyHandler);
            AppDomain.CurrentDomain.ProcessExit += currentDomain_ProcessExit;

        }

        void currentDomain_ProcessExit(object sender, EventArgs e)
        {
            FreeLibrary(Handle);
            Log.save();
        }

        static void MyHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Log.write("MyHandler caught : " + e.Message);
            Log.write(String.Format("Runtime terminating: {0}", args.IsTerminating));
            Log.save();
        }

        public static bool IsAdministrator()
        {
            return (new WindowsPrincipal(WindowsIdentity.GetCurrent()))
                    .IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool IsCPU64bit()
        {
            //
            // Machine Info
            //
            ManagementObjectSearcher objOSDetails = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            ManagementObjectCollection osDetailsCollection = objOSDetails.Get();

            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("CPU Architecture: " + mo["Architecture"].ToString());
                Log.write("CPU Name: " + mo["Name"].ToString());

                UInt16 Arch = UInt16.Parse(mo["Architecture"].ToString());
                if (Arch == 9) //x64
                {
                    return true;
                }
            }
            return false;
        }

        void GetMachineInfo()
        {
            //
            // SecureBoot Status
            //
            RegistryKey Subkey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\SecureBoot\State");
            if (Subkey != null)
            {
                int val = (int)Subkey.GetValue("UEFISecureBootEnabled");
                if (val == 0)
                {
                    Log.write("Secure Boot ... Disabled");
                }
                else
                {
                    Log.write("Secure Boot ... Enabled");
                }
            }
            else
            {
                Log.write("Secure Boot ... Not Supported");
            }

            //
            // Machine Info
            //
            ManagementObjectSearcher objOSDetails = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem");
            ManagementObjectCollection osDetailsCollection = objOSDetails.Get();

            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("Manufacturer: " + mo["Manufacturer"].ToString());
                Log.write("Model: " + mo["Model"].ToString());
            }

            //
            // Motherboard Model
            //
            objOSDetails.Query = new ObjectQuery("SELECT * FROM Win32_BaseBoard");
            osDetailsCollection = objOSDetails.Get();
            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("Product: " + mo["Product"].ToString());
            }

            //
            // BIOS Version
            //
            objOSDetails.Query = new ObjectQuery("SELECT * FROM Win32_BIOS");
            osDetailsCollection = objOSDetails.Get();
            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("BIOS Version: " + mo["Caption"].ToString());
            }

            //
            // Graphics Card type
            //
            objOSDetails.Query = new ObjectQuery("SELECT * FROM Win32_VideoController");
            osDetailsCollection = objOSDetails.Get();
            Log.write("Available GPU(s):");
            foreach (ManagementObject mo in osDetailsCollection)
            {
                Log.write("GPU: " + mo["Description"].ToString());
            }
        }
        Boolean RequirementsCheck()
        {
            /*
             * App is running as admin
             * Access to NVRAM Granted
             * System has UEFI
             * System is running Windows 8 or higher
             * System is running on Windows 64-bit 
             * Target partition has enough space
             * 
             */
            Log.write("=============[REQUIREMENTS CHECK]============");
            //
            //Administrator check
            //
            if (IsAdministrator())
                Log.write("Administrator privilege ... ok");
            else
            {
                Log.write("Administrator privilege ... fail");
                return false;
            }
            //
            // 64-bit check
            //
            if (!Environment.Is64BitOperatingSystem)
            {
                Log.write("OS Type: 32-bit!");
            }
            //
            // Check if CPU Arch. is 64-bit
            //
            if (!IsCPU64bit())
            {
                Log.write("CPU Architecture is not supported!");
                return false;
            }
            //
            // OS Version Check
            //
            Log.write("OSVer: " + Environment.OSVersion.ToString());
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                switch (System.Environment.OSVersion.Version.Major)
                {
                    case 6:
                        if (System.Environment.OSVersion.Version.Minor >= 2)
                            Log.write("OperatingSystem Version ... ok");
                        break;
                    case 10:
                        Log.write("OperatingSystem Version ... ok");
                        break;
                    default:
                        return false;
                }
            }
            else
                return false;

            //
            //Load UEFI Library
            //
            Handle = LoadLibrary(@"Win32UEFI.dll");
            if (Handle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                Log.write(string.Format("Failed to load library (ErrorCode: {0})", errorCode));
                return false;
            }

            //
            //NVRAM Access
            //            
            if (ws.GetAccesstoNVRam())
                Log.write("Windows Security: Access NVRAM Privilege ... ok");
            else
            {
                Log.write("Windows Security: Access NVRAM Privilege ... Not All Set");
            }

            //
            //UEFI Check
            //
            if (UEFIWrapper.UEFI_isUEFIAvailable())
                Log.write("System Firmware: UEFI");
            else
            {
                Log.write("System Firmware: Other");
                return false;
            }

            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            String Path=txtISOPath.Text;
            String Drive=cboDrives.Text.Substring(0, 1);
            String Size = Convert.ToUInt64((sldrSize.Value * 1024 * 1024 * 1024)/512).ToString();
            
            if(!File.Exists(Path))
                MessageBox.Show("Android IMG File is not exist");
            
            if (Size == "0")
                MessageBox.Show("Data Size is not set");

            UEFIInstaller u = new UEFIInstaller();
            DisableUI();

            //if (!u.Install(Environment.CurrentDirectory + @"\android-x86-4.4-r2.img", "E", "1000"))
            if (!u.Install(Path, Drive, Size))
                MessageBox.Show("Install Failed");
            else
                MessageBox.Show("Install Done");

            EnableUI();
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (MessageBoxResult.No == MessageBox.Show("Are you sure you want to remove android ?", "Android Installer", MessageBoxButton.YesNo, MessageBoxImage.Question))
                return;

            DisableUI();
            UEFIInstaller u = new UEFIInstaller();
            u.Uninstall();
            MessageBox.Show("Uninstall Done");
            EnableUI();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".img";
            dlg.Filter = "Android System Image |*.iso;*.img";

            if (dlg.ShowDialog() == true)
            {
                txtISOPath.Text = dlg.FileName;
                cmdInstall.IsEnabled = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (String item in Environment.GetLogicalDrives())
            {
                cboDrives.Items.Add(item);
            }

            cboDrives.SelectedIndex = 0;
        }

        void DisableUI()
        {
            cmdInstall.IsEnabled = false;
            cmdRemove.IsEnabled = false;
            cboDrives.IsEnabled = false;
            sldrSize.IsEnabled = false;
        }

        void EnableUI()
        {
            cmdInstall.IsEnabled = true;
            cmdRemove.IsEnabled = true;
            cboDrives.IsEnabled = true;
            sldrSize.IsEnabled = true;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".img";
            dlg.Filter = "Android System Image |*.iso;*.img";

            if (dlg.ShowDialog() == true)
            {
                txtISOPath.Text = dlg.FileName;
                cmdInstall.IsEnabled = true;
            }
        }

        private void cboDrives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            long DiskSize = GetTotalFreeSpace(cboDrives.SelectedItem.ToString());

            sldrSize.Maximum = ((DiskSize - config.ANDROID_SYSTEM_SIZE) / 1024 / 1024 / 1024);
            sldrSize.Value = 0.1 * sldrSize.Maximum;
            sldrSize.TickFrequency = 0.01 * sldrSize.Maximum;
        }

        private void txtlog_TextChanged(object sender, TextChangedEventArgs e)
        {

            txtlog.ScrollToEnd();
        }

        private long GetTotalFreeSpace(string driveName)
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.Name == driveName)
                {
                    return drive.AvailableFreeSpace;
                }
            }
            return -1;
        }

    }
}
