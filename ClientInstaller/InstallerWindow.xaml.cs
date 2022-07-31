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
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using System.Diagnostics;

namespace ClientInstaller
{
    /// <summary>
    /// Interaction logic for InstallerWindow.xaml
    /// </summary>
    /// 

    public partial class InstallerWindow : Window
    {
        private static void ExtractEmbeddedResource(string outputDir, string resourceLocation, List<string> files)
        {
            foreach (string file in files)
            {
                using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation + @"." + file))
                {
                    using (System.IO.FileStream fileStream = new System.IO.FileStream(System.IO.Path.Combine(outputDir, file), System.IO.FileMode.Create))
                    {
                        for (int i = 0; i < stream.Length; i++)
                        {
                            fileStream.WriteByte((byte)stream.ReadByte());
                        }
                        fileStream.Close();
                    }
                }
            }
        }

        public InstallerWindow()
        {
            InitializeComponent();
        }

        private void InstallClientStartMenu(string GameDir)
        {
            string ClientPath = GameDir + "\\nvmp_launcher.exe";
            string CommonStartMenuPath = Environment.GetFolderPath( Environment.SpecialFolder.CommonStartMenu );
            string ApplicationStartMenuPath = System.IO.Path.Combine( CommonStartMenuPath, "Programs", "NVMP" );

            if (!Directory.Exists( ApplicationStartMenuPath ))
                Directory.CreateDirectory( ApplicationStartMenuPath );

            string ShortcutLocation = System.IO.Path.Combine( ApplicationStartMenuPath, "New Vegas Multiplayer.lnk" );


            IWshRuntimeLibrary.WshShell Shell        = new IWshRuntimeLibrary.WshShell();
            IWshRuntimeLibrary.IWshShortcut Shortcut = (IWshRuntimeLibrary.IWshShortcut) Shell.CreateShortcut( ShortcutLocation );
            Shortcut.Description  = "NV:MP Game Client";
            Shortcut.TargetPath   = ClientPath;
            Shortcut.Save();
        }

        private string InstallClientFiles(string GameDir)
        {
            ExtractEmbeddedResource(GameDir, "ClientInstaller.Res", new List<string> { "nvmp_launcher.exe" });
            try
            {
                File.Copy(Assembly.GetEntryAssembly().Location, GameDir + "\\nvmp_installer.exe", true);
            } catch (Exception)
            {
                return null;
            }

            return GameDir + "\\nvmp_installer.exe";
        }

        
        private void InstallClientRegistry(string UninstallerLocation)
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
                    RegistryKey key = null;

                    try
                    {
                        string guidText = SharedUtil.ProgramGUID;
                        key = parent.OpenSubKey(guidText, true) ??
                              parent.CreateSubKey(guidText);

                        if (key == null)
                        {
                            throw new Exception(String.Format("Unable to create uninstaller '{0}\\{1}'", SharedUtil.RegKeyPath, guidText));
                        }

                        Assembly asm = GetType().Assembly;
                        Version v = asm.GetName().Version;
                        string exe = "\"" + UninstallerLocation + "\"";

                        key.SetValue("DisplayName",  ProgramDetails.DisplayName);
                        key.SetValue("URLInfoAbout", ProgramDetails.URLInfoAbout);
                        key.SetValue("Contact",      ProgramDetails.Contact);
                        key.SetValue("Publisher",    ProgramDetails.Publisher);
                        key.SetValue("ApplicationVersion", v.ToString());
                        key.SetValue("DisplayIcon", exe);
                        key.SetValue("DisplayVersion", v.ToString(2));
                        key.SetValue("InstallDate", DateTime.Now.ToString("yyyyMMdd"));
                        key.SetValue("UninstallString", exe + " /uninstall");
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "An error occurred writing uninstall information to the registry.  NV:MP is installed, but can only be uninstalled manually through the command line.",
                        ex);
                }
            }
        }

        public void Install(Window caller, string GameDir)
        {
            if (GameDir == null)
                throw new Exception("Fallout directory not found, installation quitting.");

            StatusLabel.Content = "Installing client files...";

            string UninstallFile = InstallClientFiles( GameDir );
            if (UninstallFile == null)
                throw new Exception("Could not copy over the uninstallation program.");

            System.Threading.Thread.Sleep( 500 );

            StatusLabel.Content = "Adding program to registry...";

            InstallClientRegistry( UninstallFile );

            StatusLabel.Content = "Adding program to start menu...";

            InstallClientStartMenu( GameDir );

            System.Threading.Thread.Sleep(500);

            StatusLabel.Content = "Complete.";

            System.Threading.Thread.Sleep(1000);

            // Shut down the WPF interface.
            caller.Close();
            Close();

            // Start up the client to start the patching process.
            Process client = new Process();
            client.StartInfo.FileName         = GameDir + "\\nvmp_launcher.exe";

            if (!client.Start())
                throw new Exception( "Failed starting client, installation may be corrupted." );

            
        }
    }
}
