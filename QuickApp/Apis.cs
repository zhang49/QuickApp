using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickApp
{
    public class Apis
    {
        public static bool StartApp(String filePath)
        {
            Process p = Process.Start(filePath);
            if (p.Handle != IntPtr.Zero)
            {
                return true;
            }
            return false;
        }
        public static string[] GetRecentFiles()
        {
            string appDataPath = Environment.GetEnvironmentVariable("AppData");
            string recentPath = appDataPath + "\\Microsoft\\Windows\\Recent\\";
            string[] recentFiles = System.IO.Directory.GetFiles(recentPath);
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@".*\.sln");

            List<string> slnFiles = new List<string>();
            foreach (var item in recentFiles)
            {
                if (regex.Match(item).Success)
                {
                    slnFiles.Add(item);
                }
            }

            return recentFiles;
        }

    }
}
