using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateAgent
{
    class Bartec : Platform
    {
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
                    if (v.Contains("BTZ1"))
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
                    if (v.Contains("BTZ1"))
                    {
                        ParentPath = Directory.GetParent(v).FullName;
                    }
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
            }
            catch (Exception ex)
            {
                Error(ex);
            }
        }
    }

    class BartecPlatform : PlatformFactory
    {
        public Platform Create()
        {
            return new Bartec();
        }
    }
}
