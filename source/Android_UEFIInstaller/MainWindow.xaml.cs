using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Management;

namespace Android_UEFIInstaller
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern bool FreeLibrary(IntPtr hModule);

        WindowsSecurity ws = new WindowsSecurity();
        IntPtr Handle;

        public MainWindow()
        {
            InitializeComponent();

            byte[] bytes = { 0x00,0x55, 0, 0x86};

            // If the system architecture is little-endian (that is, little end first), 
            // reverse the byte array. 
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            UInt16 i = BitConverter.ToUInt16(bytes, 0);

            
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
            if(!RequirementsCheck())
            {
                DisableUI();
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

        void GetMachineInfo()
        {
            //
            // SecureBoot Status
            //
            RegistryKey Subkey = Registry.LocalMachine.OpenSubKey(@"\SYSTEM\CurrentControlSet\Control\SecureBoot\State");
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
                Log.write("Manufacturer: "+ mo["Manufacturer"].ToString());
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
                Log.write("OS Type: ... fail");
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
            Handle = LoadLibrary( @"Win32UEFI.dll");
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
            UEFIInstaller u = new UEFIInstaller();
            DisableUI();

            //if (!u.Install(Environment.CurrentDirectory + @"\android-x86-4.4-r2.img", "E", "1000"))
            if (!u.Install(txtISOPath.Text,cboDrives.Text.Substring(0,1), cboSize.Text))
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
            cboSize.IsEnabled = false;
        }

        void EnableUI()
        {
            cmdInstall.IsEnabled = true;
            cmdRemove.IsEnabled = true;
            cboDrives.IsEnabled = true;
            cboSize.IsEnabled = true;
        }
    }
}
