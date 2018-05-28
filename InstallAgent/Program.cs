using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using SharpCompress.Archives;
using SharpCompress.Readers;
using Syroot.Windows.IO;

namespace InstallAgent
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetLocalTime(ref SYSTEMTIME st);

        static string Ntp = "NTP.exe";
        static string TestProgramName = "TestProgram.exe";
        static string LightSensorUpdate = "STMicroSensorHubTool.exe";
        static string BiosUpdate = "Update_.exe";
        static string Update = "Update.bat";
        static bool CanBiosUpdate;
        static bool CanLightSensorUpdate;
        static bool CanTestProgramRunStartup;
        static bool CanNtp;
        static Mutex mutex = null;

        static string GetIniValue(string Key)
        {
            try
            {
                var parser = new FileIniDataParser();
                parser.Parser.Configuration.CommentString = "#";
                IniData data = parser.ReadFile("config.ini");
                var SectionName = data.Sections.FirstOrDefault().SectionName;
                var FormatKey = string.Format("{0}.{1}", SectionName, Key);
                Console.WriteLine("{0} {1}", Key, data.GetKey(FormatKey));
                return data.GetKey(FormatKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return string.Empty;
            }
        }

        static void RunProcess(string path)
        {
            try
            {
                using (var p = new Process())
                {
                    Console.WriteLine(path);
                    p.StartInfo.FileName = path;
                    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
                    p.StartInfo.Arguments = "InstallAgent";
                    p.StartInfo.CreateNoWindow = false;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static List<string> FindFilePath(string directory, string FileName)
        {
            List<string> FilePath = null;
            try
            {
                FilePath = Directory.GetFiles(directory, FileName, SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return FilePath;
        }

        static void UpdateBios()
        {
            try
            {
                string ParentPath = string.Empty;
                var FilePath = FindFilePath(KnownFolders.Downloads.DefaultPath, BiosUpdate);
                foreach (var v in FilePath)
                {
                    if (v.Contains("BTZ1"))
                    {
                        Console.WriteLine(v);
                        ParentPath = Directory.GetParent(v).FullName;
                        Console.WriteLine(ParentPath);
                    }
                }

                Console.WriteLine(Path.Combine(ParentPath, Update));
                RunProcess(Path.Combine(ParentPath, Update));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void UpdateLightSensor()
        {
            try
            {
                string ParentPath = string.Empty;
                var FilePath = FindFilePath(KnownFolders.Downloads.DefaultPath, LightSensorUpdate);
                foreach (var v in FilePath)
                {
                    if (v.Contains("BTZ1"))
                    {
                        Console.WriteLine(v);
                        ParentPath = Directory.GetParent(v).FullName;
                        Console.WriteLine(ParentPath);
                    }
                }

                RunProcess(Path.Combine(ParentPath, Update));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void RunAtStartup()
        {
            var FilePath = FindFilePath(KnownFolders.Downloads.DefaultPath, TestProgramName);
            try
            {
                using (RegistryKey reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    reg.SetValue("TestProgram", "\"" + FilePath.FirstOrDefault() + "\"");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void Uncompress(string FileName, string WritePath)
        {
            try
            {
                var archive = ArchiveFactory.Open(FileName);

                foreach (var entry in archive.Entries)
                {
                    if (!entry.IsDirectory)
                    {
                        Console.WriteLine("unzip: " + entry.Key);
                        entry.WriteToDirectory(WritePath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });
                    }
                }
                archive.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void Init()
        {
            CanNtp = GetIniValue("Ntp") != "0";
            CanBiosUpdate = GetIniValue("BiosUpdate") != "0";
            CanLightSensorUpdate = GetIniValue("LightSensorUpdate") != "0";
            CanTestProgramRunStartup = GetIniValue("TestProgramRunStartup") != "0";
        }

        static void NtpUpdate()
        {
            try
            {
                var FilePath = FindFilePath(KnownFolders.Downloads.DefaultPath, Ntp);
                foreach (var v in FilePath)
                {
                    RunProcess(v);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        static void AlreadyRunning()
        {
            const string AppName = "InstallAgent";
            bool createdNew;
            mutex = new Mutex(true, AppName, out createdNew);

            if (!createdNew)
            {
                Environment.Exit(0);
            }
        }

        static void Main(string[] args)
        {
            AlreadyRunning();

            Init();

            #region Uncompress
            foreach (var v in FindFilePath(KnownFolders.Downloads.DefaultPath, "*.zip"))
            {
                if (!v.Contains("TestProgram"))
                    Uncompress(v, KnownFolders.Downloads.DefaultPath);
            }

            #endregion

            Thread.Sleep(100);

            #region NTP
            if (CanNtp)
                NtpUpdate();
            #endregion

            Thread.Sleep(100);

            #region RunStartup
            if (CanTestProgramRunStartup)
                RunAtStartup();
            #endregion

            Thread.Sleep(100);

            #region LightSensor
            if (CanLightSensorUpdate)
                UpdateLightSensor();
            #endregion

            Thread.Sleep(100);

            #region UpdateBios
            if (CanBiosUpdate)
                UpdateBios();
            #endregion

            Thread.Sleep(100);

            Console.WriteLine("Press Enter key to exit...");
            Console.ReadLine();
        }
    }
}
