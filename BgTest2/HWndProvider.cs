using liveDesktop.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using conf = liveDesktop.Configuration.ConfigurationSingleton;

namespace liveDesktop
{
    static class HWndProvider
    {
        public static IntPtr GetWorkerWHandle() {
            if(conf.GetInstance().debug)Console.WriteLine("HWndProvider:GetHandle()");
            IntPtr hWnd = IntPtr.Zero;

            IntPtr progman = FindProgman();
            SendMagicMessegeToProgman(progman);
            hWnd = FindWorkerW();
            return hWnd;
        }



        private static void SendMagicMessegeToProgman(IntPtr progman) {
            if (conf.GetInstance().debug)Console.WriteLine("Sending messege to progman");
            WinApi.SendMessage(progman,
                        0x052C,
                        new IntPtr(0),
                        IntPtr.Zero);
        }

        private static IntPtr FindProgman() {
            return WinApi.FindWindow("ProgMan", null);
        }

        static IntPtr FindWorkerW()
        {
            if (conf.GetInstance().debug) Console.WriteLine("Searching for WorkerW");
                IntPtr workerw = IntPtr.Zero;
            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            WinApi.EnumWindows(new WinApi.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = WinApi.FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            null);

                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one -> WorkerW were after
                    workerw = WinApi.FindWindowEx(IntPtr.Zero,
                                               tophandle,
                                               "WorkerW",
                                               null);
                }

                return true;
            }), IntPtr.Zero);
            if (conf.GetInstance().debug && workerw != IntPtr.Zero)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("WorkerW Found");
                Console.ResetColor();
            } else if(conf.GetInstance().debug && workerw == IntPtr.Zero)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("WorkerW not found!");
                Console.ResetColor();
            }
            return workerw;
        }
    }
}
