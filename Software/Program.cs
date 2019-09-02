using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace ProPresenter_Arduino_Listener
{
    internal class Program
    {
        private static Process process;

        /// <summary>
        ///     Compile an array of COM port names associated with given VID and PID
        /// </summary>
        /// <param name="VID"></param>
        /// <param name="PID"></param>
        /// <returns></returns>
        private static List<string> ComPortNames(string VID, string PID)
        {
            var pattern = string.Format("^VID_{0}.PID_{1}", VID, PID);
            var _rx = new Regex(pattern, RegexOptions.IgnoreCase);
            var comports = new List<string>();
            var rk1 = Registry.LocalMachine;
            var rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum");
            foreach (var s3 in rk2.GetSubKeyNames())
            {
                var rk3 = rk2.OpenSubKey(s3);
                foreach (var s in rk3.GetSubKeyNames())
                    if (_rx.Match(s).Success)
                    {
                        var rk4 = rk3.OpenSubKey(s);
                        foreach (var s2 in rk4.GetSubKeyNames())
                        {
                            var rk5 = rk4.OpenSubKey(s2);
                            var rk6 = rk5.OpenSubKey("Device Parameters");
                            comports.Add((string)rk6.GetValue("PortName"));
                        }
                    }
            }
            return comports;
        }

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        public static void sendKey(int key)
        {
            PostMessage(process.MainWindowHandle, 0x0100, key, 0);
        }

        private static void Main(string[] args)
        {
            process = Process.GetProcessesByName("ProPresenter").FirstOrDefault();
            if (process == null)
            {
                Console.WriteLine("ProPresenter is not running!?");
                Environment.Exit(12345);
            }

            var _comport = ComPortNames("1B4F", "9206");
            if (_comport.Count == 0)
            {
                Console.WriteLine("No COM ports found");
                Environment.Exit(-1);
            }
            var comport = "";
            foreach (var s in SerialPort.GetPortNames()) {
                if (_comport.Contains(s))
                    comport = s;
            }

            var serial = new SerialPort(comport, 9600);
            serial.Handshake = Handshake.None;
            serial.DtrEnable = true;
            serial.RtsEnable = true;
            serial.Open();
            serial.Write(new byte[] { 0xff }, 0, 1);
            while (true)
            {
                var s = serial.ReadExisting();
                if (s.Length != 0)
                {
                    Console.WriteLine(s);
                    s = s[s.Length - 1].ToString();
                    var keyCode = 0;
                    /*
                     * Serial | Description | Key | Value
                     * -------|-------------|-----|------
                     *   0    | Logo        | F6  | 0x75
                     *   1    | Next        | ->  | 0x27
                     *   2    | Prev        | <-  | 0x25
                     *   3    | Text        | F2  | 0x71
                     *   4    | All         | F1  | 0x70
                     * -------|-------------|-----|------
                     */
                    switch (s)
                    {
                        case "0":
                            keyCode = 0x75;
                            break;

                        case "1":
                            keyCode = 0x27;
                            break;

                        case "2":
                            keyCode = 0x25;
                            break;

                        case "3":
                            keyCode = 0x71;
                            break;

                        case "4":
                            keyCode = 0x70;
                            break;

                        default:
                            continue;
                    }
                    Console.WriteLine("keyCode");
                    Console.WriteLine(keyCode);
                    sendKey(keyCode);
                    serial.Write(new byte[] { 0xfe }, 0, 1);
                }
            }
        }

        private static void received(object sender, SerialDataReceivedEventArgs e)
        {
            var s = (SerialPort)sender;
            Console.WriteLine(s.ReadExisting());
        }
    }
}