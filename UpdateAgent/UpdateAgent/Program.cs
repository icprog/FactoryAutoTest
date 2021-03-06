﻿using System;
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
        static Platform platform = null;
        static Platform p = new Platform();

        static void Init()
        {
            try
            {
                platform = new Platform();
                //switch (p.GetIniValue("Model"))
                //{
                //    case "M101B":
                //        platform = new M101B();
                //        Console.WriteLine("M101B");
                //        break;
                //    case "Bartec":
                //        platform = new Bartec();
                //        Console.WriteLine("Bartec");
                //        break;
                //    default:
                //        throw new Exception("Platform not support");
                //}
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

            #region Prevent to sleep
            platform.PreventSleep();
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
