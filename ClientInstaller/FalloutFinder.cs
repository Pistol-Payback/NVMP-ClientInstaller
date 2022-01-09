using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.IO;

namespace ClientInstaller
{
    //-------------------------------------------------
    // Finds the machine's Fallout: New Vegas storage
    // location. 
    //-------------------------------------------------
    public static class FalloutFinder
    {
        static private string[][] RegistryKeys =
        {
            new string[] { "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 22380", "InstallLocation" },
            new string[] { "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\Steam App 22490", "InstallLocation" },
            new string[] { "Software\\GOG.com\\Games\\1312824873", "path" },
            new string[] { "Software\\GOG.com\\Games\\1454587428", "path" },
        };

        private static string FindDiscoverableGame()
        {
            foreach (var key in RegistryKeys)
            {
                using (var regKey = Registry.LocalMachine.OpenSubKey(key[0]))
                {
                    if (regKey != null)
                    {
                        var value = (string)regKey.GetValue(key[1]);
                        if (value != null)
                        {
                            if (value.Length != 0)
                            {
                                if (File.Exists($"{value}\\FalloutNV.exe"))
                                    return value;
                            }
                        }
                    }
                }
            }
            return null;
        }

        //-------------------------------------------------
        // Searches all library folders for FO:NV install
        // folder. Returns empty string on missing.
        //-------------------------------------------------
        public static string GameDir()
        {
            return FindDiscoverableGame();
        }
    }
}
