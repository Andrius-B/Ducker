using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Security.Permissions;
using static WinApi.User32.User32Methods;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Threading;

namespace BgTest2
{
    class Program
    {

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(
        int uAction, int uParam, string lpvParam, int fuWinIni);


        private static string GetCurrentWallpaperPath()
        {
            RegistryKey wallPaper = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", false);
            string WallpaperPath = wallPaper.GetValue("WallPaper").ToString();
            wallPaper.Close();
            return WallpaperPath;
        }
        static string wpPath;
        static IntPtr workerw;
        static IntPtr deviceContext;

        static void Main(string[] args)
        {
            Console.WriteLine("Initiazlizing..");
            //Attach an exit event handler
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            //get the current wallpaper path, to clear the screen after app exit
            wpPath = GetCurrentWallpaperPath();
            workerw = IntPtr.Zero;

            IntPtr progman = findProgman();
            IntPtr result = IntPtr.Zero;

            // Send 0x052C to Progman. This message directs Progman to spawn a 
            // WorkerW behind the desktop icons. If it is already there, nothing 
            // happens.
            Console.WriteLine("Sending messege to progman");
            SendMessage(progman,
                        0x052C,
                        new IntPtr(0),
                        IntPtr.Zero);

            Console.WriteLine("Searching for correct WorkerW");
            while (!findWorkerW()) {
                Console.WriteLine("WorkerW not found, trying again!");
            }
            Console.WriteLine("Correct WorkerW found!");

            if (workerw != null) {
                deviceContext = GetDCEx(workerw, IntPtr.Zero, (WinApi.User32.DeviceContextFlags)0x403);
                if (deviceContext != IntPtr.Zero)
                {

                    // Create a Graphics instance from the Device Context
                    using (Graphics g = Graphics.FromHdc(deviceContext))
                    {
                        System.Drawing.Drawing2D.GraphicsState rsState = g.Save();
                        
                        Console.WriteLine("Filling Rect");
                        g.FillRectangle(new SolidBrush(Color.Red), 100, 100, 500, 500);
                        g.FillRectangle(new SolidBrush(Color.Green), 200, 200, 500, 500);
                        g.FillRectangle(new SolidBrush(Color.Blue), 300, 300, 500, 500);
                        g.Flush();

                        g.Dispose();
                    }
                    
                }
            }
            Thread.Sleep(10000);
        }

        static bool findWorkerW() {
            // We enumerate all Windows, until we find one, that has the SHELLDLL_DefView 
            // as a child. 
            // If we found that window, we take its next sibling and assign it to workerw.
            EnumWindows(new WinApi.User32.EnumWindowsProc((tophandle, topparamhandle) =>
            {
                IntPtr p = FindWindowEx(tophandle,
                                            IntPtr.Zero,
                                            "SHELLDLL_DefView",
                                            null);

                if (p != IntPtr.Zero)
                {
                    // Gets the WorkerW Window after the current one -> WorkerW were after
                    workerw = FindWindowEx(IntPtr.Zero,
                                               tophandle,
                                               "WorkerW",
                                               null);
                }

                return true;
            }), IntPtr.Zero);
            return workerw != null;
        }


        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Resetting wallpaper");
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wpPath, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            Console.WriteLine("Releasing workerw context");
            ReleaseDC(workerw, deviceContext);
            Console.WriteLine("workerw released");
            Console.WriteLine("Program finished");
        }

        static private IntPtr findProgman() {
            return WinApi.User32.User32Methods.FindWindow("ProgMan", null);
        }
    }
}
