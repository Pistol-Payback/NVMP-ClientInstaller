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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Threading;

namespace ClientInstaller
{
    public class InstallStatus
    {
        private bool IsFalloutInstalled;     // Should be true.
        private bool IsNVMPInstalled;        // Should be false.

        public void Check()
        {
            IsFalloutInstalled = true;
            IsNVMPInstalled    = false;

            string FalloutDirectory = FalloutFinder.GameDir();
            if (FalloutDirectory == null)
            {
                IsFalloutInstalled = false;
                return;
            }

            if (File.Exists(FalloutDirectory + "\\nvmp_launcher.exe"))
            {
                IsNVMPInstalled = true;
                return;
            }
        }

        public string GetMessage()
        {
            if (!IsFalloutInstalled)
                return "Fallout: New Vegas is not installed, or could not be found.";

            if (IsNVMPInstalled)
                return "NV:MP is already installed, please uninstall before attempting to reinstall.";

            return null;
        }

        public bool CanInstall()
        {
            return (IsFalloutInstalled && (!IsNVMPInstalled));
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private InstallStatus Status;
        private InstallerWindow    InstallerWindowInstance   = null;
        private UninstallerWindow  UninstallerWindowInstance = null;

        public void OnInstallClick(object sender, RoutedEventArgs evt)
        {
            InstallerWindowInstance = new InstallerWindow();
            InstallerWindowInstance.Show();
            Hide();

            try {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ThreadStart( ()=>
                    InstallerWindowInstance.Install(this, FalloutFinder.GameDir() )));

            }
            catch (Exception e)
            {
                InstallerWindowInstance.Close();

                MessageBox.Show("Installation Error: " + e.Message);
                Show();
            }


        }

        public void DoUninstall()
        {
            UninstallerWindowInstance = new UninstallerWindow();
            UninstallerWindowInstance.Show();
            Close();

            try {
                UninstallerWindowInstance.Uninstall( FalloutFinder.GameDir() );
            } catch (Exception e)
            {
                MessageBox.Show("Uninstallation Error: " + e.Message);
            }

            UninstallerWindowInstance.Close();
        }

        public bool IsUninstallRequested()
        {
            string[] CmdArguments = Environment.GetCommandLineArgs();
            if (CmdArguments.Length < 2)
                return false;

            if (CmdArguments[1] == "/uninstall")
                return true;

            return false;
        }

        public MainWindow()
        {
            bool UninstallRequested;
            UninstallRequested = IsUninstallRequested();

            InitializeComponent();

            // Uninstall the program.
            if (UninstallRequested)
            {
                DoUninstall();
                return;
            }

            // Install the program.
            Status = new InstallStatus();
            Status.Check();

            string ErrorMessage;

            if (!Status.CanInstall())
            {
                ErrorMessage = Status.GetMessage();

                System.Windows.MessageBox.Show("ERROR: " + ErrorMessage);
                Close();
                return;
            }
        }
    }
}
