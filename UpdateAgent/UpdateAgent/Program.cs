using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IniParser;
using IniParser.Model;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using SharpCompress.Archives;
using SharpCompress.Readers;
using Syroot.Windows.IO;

namespace UpdateAgent
{
    class Program
    {
        const string M101BProductName = "Agile_X";
        const string BartecProductName = "Agile_X_IS";
        static Platform platform = null;
        static Platform p = new Platform();

        static string GetPlatform()
        {
            string BIOSMainBoard = "";
            ManagementScope managementScope;
            ConnectionOptions connectionOptions;

            try
            {
                connectionOptions = new ConnectionOptions();
                connectionOptions.Impersonation = ImpersonationLevel.Impersonate;
                connectionOptions.Authentication = AuthenticationLevel.Default;
                connectionOptions.EnablePrivileges = true;

                managementScope = new ManagementScope();
                managementScope.Path = new ManagementPath(@"\\" + Environment.MachineName + @"\root\CIMV2");
                managementScope.Options = connectionOptions;

                SelectQuery selectQuery = new SelectQuery("SELECT * FROM Win32_ComputerSystemProduct");
                ManagementObjectSearcher managementObjectSearch = new ManagementObjectSearcher(managementScope, selectQuery);
                ManagementObjectCollection managementObjectCollection = managementObjectSearch.Get();

                foreach (ManagementObject managementObject in managementObjectCollection)
                {
                    BIOSMainBoard = (string)managementObject["Name"];
                }
            }
            catch (Exception ex)
            {
                p.Error(ex);
            }

            return BIOSMainBoard;
        }

        static void Init()
        {
            try
            {
                switch (GetPlatform())
                {
                    case M101BProductName:
                    case "M101B":
                        platform = new M101B();
                        Console.WriteLine("M101B");
                        break;
                    case BartecProductName:
                        platform = new Bartec();
                        Console.WriteLine("Bartec");
                        break;
                    default:
                        throw new Exception("Platform not support");
                        break;
                }
            }
            catch (Exception ex)
            {
                p.Error(ex);
            }

        }

        static void Main(string[] args)
        {

            #region Check Multi Open
            p.AlreadyRunning();
            #endregion

            #region Init
            Init();
            #endregion
            Thread.Sleep(100);

            #region Readini
            platform.Readini();
            #endregion
            Thread.Sleep(100);

            #region Extract
            platform.Extract();
            #endregion
            Thread.Sleep(100);

            #region UpdateMac
            platform.UpdateMac();
            #endregion
            Thread.Sleep(100);

            #region RunStartup
            platform.RunAtStartup();
            #endregion
            Thread.Sleep(100);

            #region LightSensor
            platform.UpdateLightSensor();
            #endregion
            Thread.Sleep(100);

            #region NTP
            platform.NtpUpdate();
            #endregion
            Thread.Sleep(100);

            #region UpdateBios
            platform.UpdateBios();
            #endregion
            Thread.Sleep(100);

            Console.WriteLine("Press Enter key to exit...");
            Console.ReadLine();
        }
    }
}
