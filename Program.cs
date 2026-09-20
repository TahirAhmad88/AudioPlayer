using System;
using System.Threading;
using System.Windows.Forms;

namespace AudioPlayerApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            Application.Run(new Form1());
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            MessageBox.Show("An unexpected error occurred:\r\n" + e.Exception.Message,
                "AudioPlayer Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string message = (e.ExceptionObject as Exception)?.Message ?? "Unknown fatal error.";
            MessageBox.Show("A fatal error occurred:\r\n" + message,
                "AudioPlayer Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}