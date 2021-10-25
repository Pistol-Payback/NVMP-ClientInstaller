using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientInstaller
{
    public class ProgramDetails
    {
        public static string DisplayName  = "NV:MP";
        public static string Publisher    = "NV:MP Team";
        public static string URLInfoAbout = "http://nv-mp.com";
        public static string Contact      = "forum@nlan.org"; 
    }

    public class SharedUtil
    {
        public static string UninstallRegKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        public static string UninstallGUID       = @"{3FAA6664-C4C1-4754-8D5F-2B7C621E9297}";
    }
}
