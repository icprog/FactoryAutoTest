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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpdateAgent
{
    public class Platform
    {
        protected Mutex mutex = null;
        protected string TestProgramName = "TestProgram.exe";
        protected string Ntp = "NTP.exe";
        protected string LightSensorUpdate = "STMicroSensorHubTool.exe";
        protected string BiosUpdate = "*.bin";
        protected string Update = "Update.bat";
        public bool CanBiosUpdate = false;
        public bool CanLightSensorUpdate = false;
        public bool CanTestProgramRunStartup = false;
        public bool CanNtp = false;
        public bool CanMacUpdate = false;

        public virtual void UpdateMac()
        {

        }

        public virtual void UpdateBios()
        {

        }

        public virtual void UpdateLightSensor()
        {

        }

        public virtual void Readini()
        {

        }

        public virtual void Extract()
        {

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
                IniData data = parser.ReadFile(Path.Combine(Directory.GetParent(FilePath).FullName,"config.ini"));
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

                using (RegistryKey reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    reg.SetValue("TestProgram", "\"" + FilePath.FirstOrDefault() + "\"");
                }

                using (RegistryKey reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    reg.SetValue("TestProgram", "\"" + FilePath.FirstOrDefault() + "\"");
                }
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
