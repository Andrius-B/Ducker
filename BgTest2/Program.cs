using CommandLine;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

using conf = liveDesktop.Configuration.ConfigurationSingleton;

namespace liveDesktop
{
    class Program
    {


        [Verb("duck", HelpText = "Duck windows beneath the desktop.")]
        public class DuckOptions {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Value(0, HelpText = "Window titles")]
            public IEnumerable<string> ToDuck { get; set; }
        }

        [Verb("unduck", HelpText = "Move windows back above the desktop.")]
        public class UnDuckOptions {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Value(0, HelpText = "Window titles")]
            public IEnumerable<string> UnDuck { get; set; }
        }

        [Verb("clear", HelpText = "Forces desktop refresh.")]
        public class ClearOptions
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }
        }
        static IntPtr workerw;
        static IntPtr desktopWnd;
        static void Main(string[] args)
        {
            var parser = new Parser(config => config.HelpWriter = Console.Out);
            parser.ParseArguments<DuckOptions, UnDuckOptions, ClearOptions>(args)
                  .MapResult(
                  (DuckOptions dopts) => Duck(dopts),
                  (UnDuckOptions udopts) => UnDuck(udopts),
                  (ClearOptions copts) => Clear(copts),
                  errs => 1);
        }

        static int Duck(DuckOptions dopts) {
            conf.GetInstance().debug = dopts.Verbose;
            workerw = HWndProvider.GetWorkerWHandle();
            foreach (string title in dopts.ToDuck)
            {
                IntPtr childWindowHandle = WinApi.FindWindow(null, title);
                SetParentWindow(childWindowHandle, workerw);
            }
            return 0;
        }

        static int UnDuck(UnDuckOptions udopts)
        {
            conf.GetInstance().debug = udopts.Verbose;
            desktopWnd = WinApi.GetDesktopWindow();
            workerw = HWndProvider.GetWorkerWHandle();
            
            if (conf.GetInstance().debug)
            {
                Console.WriteLine("WorkerW pointer: {0:X}", workerw.ToInt64());
                Console.WriteLine("Desktop pointer: {0:X}", desktopWnd.ToInt64());
            }
            foreach (string title in udopts.UnDuck)
            {
                IntPtr childWindowHandle = WinApi.FindWindowEx(workerw, IntPtr.Zero, null, title);
                SetParentWindow(childWindowHandle, desktopWnd);
            }

            return 0;
        }

        static int Clear(ClearOptions copts) {
            conf.GetInstance().debug = copts.Verbose;
            string wpPath = WinApi.GetCurrentWallpaperPath();

            //WinApi.PaintDesktop(WinApi.GetDC(desktopWnd));

            uint SPI_SETDESKWALLPAPER = 20;
            int SPIF_UPDATEINIFILE = 0x01;
            int SPIF_SENDWININICHANGE = 0x02;
            //WinApi.SystemParametersInfo(SPI_SETDESKWALLPAPER,
            //                            0,
            //                            "",
            //                            SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);

            WinApi.SystemParametersInfo(SPI_SETDESKWALLPAPER,
                                        0,
                                        wpPath,
                                        SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            if (conf.GetInstance().debug)
            {
                Console.WriteLine("Desktop change request sent!");
            }
            return 1;
        }

        private static int SetParentWindow(IntPtr child, IntPtr parent) {
            Debug.WriteLine(child);
            if (child != IntPtr.Zero)
            {
                var res = WinApi.SetParent(child, parent);
                if (conf.GetInstance().debug)
                {
                    Console.WriteLine("Set parent result:{0:X}", res.ToInt64());
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Child with pointer {0:X} to parent {1:X}", child.ToInt64(), parent.ToInt64());
                    Console.ResetColor();
                }
                return 0;
            }
            else
            {
                if (conf.GetInstance().debug)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Window parent not set, because a window with the title provided could not be found!");
                    Console.ResetColor();
                }
                return -1;
            }
        }
    }
}
