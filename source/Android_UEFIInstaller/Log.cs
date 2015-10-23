using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Android_UEFIInstaller
{
    public static class Log
    {
        private static System.Windows.Controls.TextBox _buffer;
        private static System.Windows.Controls.TextBlock _lstatus;
        private static String _lbuffer;

        public static void write(String text)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _buffer.AppendText(text + Environment.NewLine);
            });
            _lbuffer += (text + Environment.NewLine);
        }

        public static void updateStatus(String text)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _lstatus.Text = text;
            }); 
        }

        public static void save()
        {

            String filePath = String.Format(config.LOG_FILE_PATH, DateTime.Now.Millisecond);
            File.WriteAllText(filePath, _lbuffer);
        }

        public static void SetLogBuffer(System.Windows.Controls.TextBox buffer)
        {
            _buffer = buffer;
        }

        public static void SetStatuslabel(System.Windows.Controls.TextBlock buffer)
        {
            _lstatus = buffer;
        }
    }
}
