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
using System.Diagnostics;

namespace BgTest2
{
    class Program
    {

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDWININICHANGE = 0x02;
        const int FRAMERATE = 60;
        const int RUNTIME = 20000;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(
        int uAction, int uParam, string lpvParam, int fuWinIni);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int RedrawWindow(
        IntPtr hWnd, RectangleF lprcUpdate, IntPtr hrgnUpdate, WinApi.User32.RedrawWindowFlags fuWinIni);

        /*[DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int RedrawWindow(
        IntPtr hWnd, Rectangle lprcUpdate, string lpvParam, int fuWinIni);*/

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
                        RectangleF bonds = g.VisibleClipBounds;
                        Console.WriteLine("Size: {0}x{1}", bonds.Width, bonds.Height);
                        Console.WriteLine("hWnd of WorkerW: {0,10:X}", workerw);

                        BallData b = new BallData() {
                            x = 0,
                            y = 0,
                            w = 100,
                            h = 100,
                            xVel = 10,
                            yVel = 15
                        };
                        Console.WriteLine("Filling Rect");
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        long frameTime = 0;
                        long lastTime = 0;
                        while (stopwatch.ElapsedMilliseconds < RUNTIME)
                        {
                            frameTime = stopwatch.ElapsedMilliseconds - lastTime;
                            if (frameTime > (1000 / FRAMERATE))
                            {
                                RenderLoop(g, ref b);
                                lastTime = stopwatch.ElapsedMilliseconds;
                            }
                        }
                        g.Dispose();

                    }
                    
                }
            }
        }

        class BallData {
            public int x, y, w, h, xVel, yVel;
        }

        static void RenderLoop(Graphics g, ref BallData ball) {
            RectangleF r = g.VisibleClipBounds;
            if (ball.x + ball.xVel + ball.w > r.Right || ball.x + ball.xVel < r.Left) {
                ball.xVel = -ball.xVel;
            }
            if (ball.y + ball.yVel + ball.h > r.Bottom || ball.y + ball.yVel < r.Top)
            {
                ball.yVel = -ball.yVel;
            }
            ball.x += ball.xVel;
            ball.y += ball.yVel;
            g.Clear(Color.Gray);
            g.DrawEllipse(new Pen(Color.WhiteSmoke), ball.x, ball.y, ball.w, ball.h);
            g.Flush();
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
            return FindWindow("ProgMan", null);
        }
    }
}
