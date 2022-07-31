using System;
using System.Collections.Generic;
using System.Linq;
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
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace ClientInstaller
{
    /// <summary>
    /// Interaction logic for UninstallerWindow.xaml
    /// </summary>
    public partial class UninstallerWindow : Window
    {
        private List<string> UninstallClientFiles(string GameDir)
        {
            if (!Directory.Exists( GameDir ))
                throw new Exception( "Fallout directory not found." );

            List<string> FailedFiles = new List<string>();

            try {
                File.Delete( GameDir + "\\nvmp_launcher.exe");
            } catch (Exception) { FailedFiles.Add("nvmp_launcher.exe"); }

            try
            {
                File.Delete(GameDir + "\\.nvmp_version");
            }
            catch (Exception) { FailedFiles.Add("nvmp_launcher.exe"); }

            try
            {
                Directory.Delete(GameDir + "\\nvmp", true);
            } catch (Exception) { FailedFiles.Add("nvmp\\"); }

            return FailedFiles;
        }

        private void UninstallRegistry()
        {
            using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(
                         SharedUtil.RegKeyPath, true))
            {
                if (parent == null)
                {
                    throw new Exception("Windows registry key not found.");
                }
                try
                {
                    string guidText = SharedUtil.ProgramGUID;
                    parent.DeleteSubKey(guidText, true);

                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to remove key from registry. ", ex);
                }
            }
        }

        public void UninstallStartMenu()
        {

            string CommonStartMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu);
            string ApplicationStartMenuPath = System.IO.Path.Combine(CommonStartMenuPath, "Programs", "NVMP");

            if (Directory.Exists(ApplicationStartMenuPath))
                Directory.Delete(ApplicationStartMenuPath, true);

        }

        public void Uninstall(string GameDir)
        {
            string ErrorMessage = null;

            // Kill NVMP and Fallout is they're open.
            foreach (var process in Process.GetProcessesByName("Fallout.exe"))
            {
                process.Kill();
            }

            foreach (var process in Process.GetProcessesByName("nvmp_launcher.exe"))
            {
                process.Kill();
            }

            foreach (var process in Process.GetProcessesByName("NVMP.exe"))
            {
                process.Kill();
            }

            if (GameDir != null)
            {
                List<string> FailedFiles = new List<string>();
                try {
                    FailedFiles = UninstallClientFiles(GameDir);
                } catch (Exception e)
                {
                    ErrorMessage = e.Message;
                }

                if (FailedFiles.Count > 0)
                {
                    if (ErrorMessage == null)
                        ErrorMessage = "";

                    ErrorMessage = "unable to remove files: " + String.Join(", ", FailedFiles.ToArray()) + (ErrorMessage.Length > 0 ? "\n" + ErrorMessage : ErrorMessage);
                }

            }

            try
            {
                UninstallRegistry();
            } catch (Exception)
            {
            }

            try
            {
                UninstallStartMenu();
            } catch (Exception)
            {
            }

            Close();

            if (ErrorMessage != null)
            {
                MessageBox.Show("Partially uninstalled NV:MP from your system, " + ErrorMessage );
            }
            else
            {
                MessageBox.Show("Uninstalled NV:MP from your system." );
            }
        }

        public UninstallerWindow()
        {
            InitializeComponent();
        }
    }
}
