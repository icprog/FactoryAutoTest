using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateAgent
{
    class M101B : Platform
    {
        string Install = "Install.bat";
        string MacUpdate = "iqvw32.sys";

        public override void Extract()
        {
            foreach (var v in FindFilePath(KnownFolders.Downloads.DefaultPath, "*.zip"))
            {
                Uncompress(v, KnownFolders.Downloads.DefaultPath);
            }
        }

        public override void UpdateBios()
        {
            try
            {
                if (!CanBiosUpdate)
                    return;

                string ParentPath = string.Empty;
                var FilePath = FindFilePath(KnownFolders.Downloads.DefaultPath, BiosUpdate);
                foreach (var v in FilePath)
                {
                    if (v.Contains("M101B"))
                        ParentPath = Directory.GetParent(v).FullName;
                }

                RunProcess(Path.Combine(ParentPath, Update));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public override void UpdateLightSensor()
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

                RunProcess(Path.Combine(ParentPath, Update));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public override void Readini()
        {
            try
            {
                CanNtp = GetIniValue("Ntp") != "0";
                CanBiosUpdate = GetIniValue("BiosUpdate") != "0";
                CanLightSensorUpdate = GetIniValue("LightSensorUpdate") != "0";
                CanTestProgramRunStartup = GetIniValue("TestProgramRunStartup") != "0";
                CanBurnInAgentRunStartup = GetIniValue("BurnInAgentRunStartup") != "0";
                CanMacUpdate = GetIniValue("MacUpdate") != "0";
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }

        public override void UpdateMac()
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
                RunProcess(Path.Combine(ParentPath, Update));
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
    }

    class M101BPlatform : PlatformFactory
    {
        public Platform Create()
        {
            return new M101B();
        }
    }
}
