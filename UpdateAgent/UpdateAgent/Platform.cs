using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using SharpCompress.Archives;
using SharpCompress.Readers;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpdateAgent
{
    public class Platform
    {
        [DllImport("kernel32.dll")]
        static extern uint SetThreadExecutionState(ExecutionFlag flags);

        [Flags]
        enum ExecutionFlag : uint
        {
            System = 0x00000001,
            Display = 0x00000002,
            Continus = 0x80000000,
        }

        protected Mutex mutex = null;
        protected string TestProgramName = "TestProgram.exe";
        protected string BurnInAgentName = "BurnInAgent.exe";
        protected string Ntp = "NTP.exe";
        protected string LightSensorUpdate = "STMicroSensorHubTool.exe";
        protected string BiosUpdate = "*.bin";
        protected string UpdateBat = "Update.bat";
        protected string Update = "Update.exe";
        protected string Install = "Install.bat";
        protected string MacUpdate = "iqvw32.sys";
        public bool CanBiosUpdate = false;
        public bool CanLightSensorUpdate = false;
        public bool CanTestProgramRunStartup = false;
        public bool CanNtp = false;
        public bool CanMacUpdate = false;
        public bool CanBurnInAgentRunStartup = false;

        public void PreventSleep()
        {
            SetThreadExecutionState(ExecutionFlag.System | ExecutionFlag.Display | ExecutionFlag.Continus);
        }

        public void UpdateMac()
        {
            try
            {
                if (!CanMacUpdate)
                    return;

                string ParentPath = string.Empty;
                var FilePath = FindFilePath(KnownFolders.Desktop.DefaultPath, MacUpdate);
                foreach (var v in FilePath)
                {
                    ParentPath = Directory.GetParent(v).FullName;
                }

                RunProcess(Path.Combine(ParentPath, Install));
                Thread.Sleep(200);
                RunProcess(Path.Combine(ParentPath, UpdateBat));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public void UpdateBios()
        {
            try
            {
                if (!CanBiosUpdate)
                    return;

                string ParentPath = string.Empty;
                var FilePath = FindFilePath(KnownFolders.Downloads.DefaultPath, BiosUpdate);
                foreach (var v in FilePath)
                {
                    ParentPath = Directory.GetParent(v).FullName;
                }

                if (FindFilePath(ParentPath, UpdateBat).Any())
                    RunProcess(Path.Combine(ParentPath, UpdateBat));
                else
                    RunProcess(Path.Combine(ParentPath, Update));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public void UpdateLightSensor()
        {
            try
            {
                if (!CanLightSensorUpdate)
                    return;

                string ParentPath = string.Empty;
                var FilePath = FindFilePath(KnownFolders.Downloads.DefaultPath, LightSensorUpdate);
                foreach (var v in FilePath)
                {
                    ParentPath = Directory.GetParent(v).FullName;
                }

                RunProcess(Path.Combine(ParentPath, UpdateBat));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public void Readini()
        {
            try
            {
                CanNtp = GetIniValue("Ntp") != "0";
                CanBiosUpdate = GetIniValue("BiosUpdate") != "0";
                CanLightSensorUpdate = GetIniValue("LightSensorUpdate") != "0";
                CanBurnInAgentRunStartup = GetIniValue("BurnInAgentRunStartup") != "0";
                CanTestProgramRunStartup = GetIniValue("TestProgramRunStartup") != "0";
                CanMacUpdate = GetIniValue("MacUpdate") != "0";
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public void Extract()
        {
            foreach (var v in FindFilePath(KnownFolders.Downloads.DefaultPath, "*.zip"))
            {
                Uncompress(v, KnownFolders.Downloads.DefaultPath);
            }
        }

        public void AlreadyRunning()
        {
            const string AppName = "UpdateAgent";
            bool createdNew;
            mutex = new Mutex(true, AppName, out createdNew);

            if (!createdNew)
            {
                Environment.Exit(0);
            }
        }

        public void NtpUpdate()
        {
            try
            {
                if (!CanNtp)
                    return;

                var FilePath = FindFilePath(KnownFolders.Desktop.DefaultPath, Ntp);
                foreach (var v in FilePath)
                {
                    RunProcess(v);
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public string GetIniValue(string Key)
        {
            try
            {
                var parser = new FileIniDataParser();
                parser.Parser.Configuration.CommentString = "#";
                var FilePath = Assembly.GetEntryAssembly().Location;
                IniData data = parser.ReadFile(Path.Combine(Directory.GetParent(FilePath).FullName, "config.ini"));
                var SectionName = data.Sections.FirstOrDefault().SectionName;
                var FormatKey = string.Format("{0}.{1}", SectionName, Key);
                Console.WriteLine("{0} {1}", Key, data.GetKey(FormatKey));
                return data.GetKey(FormatKey);
            }
            catch (Exception ex)
            {
                Error(ex);
                return string.Empty;
            }
        }

        public void Uncompress(string FileName, string WritePath)
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
                Error(ex);
            }
        }

        public List<string> FindFilePath(string directory, string FileName)
        {
            List<string> FilePath = null;
            try
            {
                FilePath = Directory.GetFiles(directory, FileName, SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                Error(ex);
            }
            return FilePath;
        }

        public void CreateStatupBat(string path)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(KnownFolders.Startup.DefaultPath, Path.GetFileNameWithoutExtension(path) + ".bat")))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine("start {0}", path);
                writer.WriteLine("exit");
            }
        }

        public void RunAtStartup()
        {
            try
            {
                if (!CanTestProgramRunStartup)
                    return;

                var FilePath = FindFilePath(KnownFolders.Desktop.DefaultPath, TestProgramName);

                if (!FilePath.Any())
                    throw new Exception("TestProgram not found");

                FilePath.Sort((x, y) =>
                {
                    var versionx = FileVersionInfo.GetVersionInfo(x).ProductVersion;
                    var versiony = FileVersionInfo.GetVersionInfo(y).ProductVersion;
                    var VersionX = new Version(versionx);
                    var VersionY = new Version(versiony);
                    return -VersionX.CompareTo(VersionY);
                });

                CreateStatupBat(FilePath.FirstOrDefault());
                Console.WriteLine("TestProgram RunAtStartup OK");

                if (!CanBurnInAgentRunStartup)
                    return;

                FilePath = FindFilePath(KnownFolders.Desktop.DefaultPath, BurnInAgentName);

                if (!FilePath.Any())
                    throw new Exception("BurnInAgent not found");

                FilePath.Sort((x, y) =>
                {
                    var versionx = FileVersionInfo.GetVersionInfo(x).ProductVersion;
                    var versiony = FileVersionInfo.GetVersionInfo(y).ProductVersion;
                    var VersionX = new Version(versionx);
                    var VersionY = new Version(versiony);
                    return -VersionX.CompareTo(VersionY);
                });

                CreateStatupBat(FilePath.FirstOrDefault());
                Console.WriteLine("BurnInAgent RunAtStartup OK");
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        protected void RunProcess(string path)
        {
            try
            {
                using (var p = new Process())
                {
                    Console.WriteLine(path);
                    p.StartInfo.FileName = path;
                    p.StartInfo.WorkingDirectory = Path.GetDirectoryName(path);
                    p.StartInfo.Arguments = "UpdateAgent";
                    p.StartInfo.CreateNoWindow = false;
                    p.StartInfo.UseShellExecute = false;
                    p.Start();
                    p.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public void Error(object msg)
        {
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
            MessageBox.Show(msg.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Environment.Exit(0);
        }
    }
}
