using IniParser;
using IniParser.Model;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NTP
{
    class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetLocalTime(ref SYSTEMTIME st);

        public static DateTime GetNetworkTime()
        {
            string ntpServer = GetIniValue("NtpServer");

            var ntpData = new byte[48];

            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            var ipEndPoint = new IPEndPoint(addresses[0], 123);

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            const byte serverReplyTime = 40;

            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime.ToLocalTime();
        }

        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }

        static void SetLocalTime(DateTime DateTime)
        {
            SYSTEMTIME st = new SYSTEMTIME();
            st.wYear = (ushort)DateTime.Year;
            st.wMonth = (ushort)DateTime.Month;
            st.wDay = (ushort)DateTime.Day;
            st.wHour = (ushort)DateTime.Hour;
            st.wMinute = (ushort)DateTime.Minute;
            st.wSecond = (ushort)DateTime.Second;
            SetLocalTime(ref st);
        }

        static string GetIniValue(string Key)
        {
            try
            {
                var parser = new FileIniDataParser();
                parser.Parser.Configuration.CommentString = "#";
                IniData data = parser.ReadFile("config.ini");
                var SectionName = data.Sections.FirstOrDefault().SectionName;
                var FormatKey = string.Format("{0}.{1}", SectionName, Key);
                Console.WriteLine(data.GetKey(FormatKey));
                return data.GetKey(FormatKey);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return string.Empty;
            }

        }

        static void Main(string[] args)
        {
            //Console.WriteLine("5566");
            #region LocalTime
            try
            {
                var DateTime = GetNetworkTime();
                SetLocalTime(DateTime);
                Console.WriteLine(DateTime);
                Console.WriteLine("Update ok");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                //Environment.Exit(0);
            }
            #endregion

            if (args.Length == 0)
            {
                Console.WriteLine("Press Enter key to exit...");
                Console.ReadLine();
            }
        }
    }
}
