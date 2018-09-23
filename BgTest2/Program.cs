using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;

using conf = liveDesktop.Configuration.ConfigurationSingleton;

namespace liveDesktop
{
    class Program
    {
        static IntPtr workerw;
        static void Main(string[] args)
        {
            workerw = HWndProvider.GetHandle();
            IntPtr gameWindowHandle = WinApi.FindWindow(null, args[0]);
            Console.WriteLine(gameWindowHandle);
            if (gameWindowHandle != IntPtr.Zero)
            {
                WinApi.SetParent(gameWindowHandle, workerw);
                if (conf.GetInstance().debug)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Window parent set!");
                    Console.ResetColor();
                }
            }
            else {
                if (conf.GetInstance().debug)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Window parent not set, because a window with the title provided could not be found!");
                    Console.ResetColor();
                }
            }
            if (conf.GetInstance().debug)Console.ReadKey();
        }
    }
}
