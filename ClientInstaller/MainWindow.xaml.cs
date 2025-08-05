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
using Microsoft.Win32;

namespace ClientInstaller
{
    public class InstallStatus
    {
        private string FalloutDirectoryOverride = null;

        public string FalloutDirectory
        {
            get
            {
                if (FalloutDirectoryOverride != null)
                    return FalloutDirectoryOverride;

                return FalloutFinder.GameDir();
            }

            set
            {
                FalloutDirectoryOverride = value;
            }
        }

        public bool IsFalloutInstalled => Directory.Exists(FalloutDirectory);

        public bool IsNVMPInstalled
        {
            get
            {
                try
                {
                    using (RegistryKey parent = Registry.LocalMachine.OpenSubKey(
                             SharedUtil.RegKeyPath, true))
                    {
                    if (parent == null)
                    {
                        return false;
                    }

                        RegistryKey key = null;

                        try
                        {
                            string guidText = SharedUtil.ProgramGUID;
                            key = parent.OpenSubKey(guidText, false);
                            return key != null;
                        }
                        finally
                        {
                            if (key != null)
                            {
                                key.Close();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                }

                return false;
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
        private InstallStatus      Status;
        private InstallerWindow    InstallerWindowInstance   = null;
        private UninstallerWindow  UninstallerWindowInstance = null;

        public void OnInstallClick(object sender, RoutedEventArgs evt)
        {
            if (Status.FalloutDirectory == null)
            {
                MessageBox.Show("Could not start the installation as the Fallout directory seems to be invalid!");
                return;
            }

            InstallerWindowInstance = new InstallerWindow();
            InstallerWindowInstance.Show();
            Hide();

            try {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ThreadStart( ()=>
                    InstallerWindowInstance.Install(this, Status.FalloutDirectory)));

            }
            catch (Exception e)
            {
                InstallerWindowInstance.Close();

                MessageBox.Show("Installation Error: " + e.ToString());
                Show();
            }


        }
        private void OpenSearchWindow_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                dlg.SelectedPath = Status.FalloutDirectory ?? Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK
                    && !string.IsNullOrWhiteSpace(dlg.SelectedPath))
                {
                    // override status and display it
                    Status.FalloutDirectory = dlg.SelectedPath;
                    FoundFilePathTextBox.Text = dlg.SelectedPath;
                }
            }
        }
        public void DoUninstall()
        {
            UninstallerWindowInstance = new UninstallerWindow();
            UninstallerWindowInstance.Show();
            Close();

            try {
                UninstallerWindowInstance.Uninstall( Status.FalloutDirectory );
            } catch (Exception e)
            {
                MessageBox.Show("Uninstallation Error: " + e.ToString());
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
            InitializeComponent();

            // Install the program.
            Status = new InstallStatus();

            //Show file path in UI
            FoundFilePathTextBox.Text = Status.FalloutDirectory ?? "";

            // Uninstall the program.
            if ( IsUninstallRequested() )
            {
                DoUninstall();
                return;
            }

            if (!Status.CanInstall())
            {
                if (Status.IsNVMPInstalled)
                {
                    var result = MessageBox.Show("ERROR: NV:MP is already installed, would you like to uninstall it?"
                        , "NV:MP Installer"
                        , MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        DoUninstall();
                        return;
                    }
                }

                if (!Status.IsFalloutInstalled)
                {
                    var result = MessageBox.Show("ERROR: Could not find an installation of Fallout: New Vegas. Would you like to manually select a folder to use for installation?"
                        , "NV:MP Installer"
                        , MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
                        {
                            var dresult = dialog.ShowDialog();
                            if (dresult == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                            {
                                Status.FalloutDirectory = dialog.SelectedPath;
                                return;
                            }
                        }
                    }
                }

                MessageBox.Show( Status.GetMessage(), "NV:MP Installer" );
                Close();
            }
        }
    }
}
