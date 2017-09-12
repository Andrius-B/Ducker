using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;

namespace liveDesktop
{
    class Program
    {


        static IntPtr workerw;
        static string wpPath;
        
        static void Main(string[] args)
        {
            Console.WriteLine("Initiazlizing..");
            wpPath = WinApi.GetCurrentWallpaperPath();
            workerw = HWndProvider.GetHandle();
            
            Window w = new Window();
            Console.WriteLine("GameWindow handle: {0:G}",w.WindowInfo.Handle);
            IntPtr gameWindowHandle = w.WindowInfo.Handle;
            w.WindowBorder = OpenTK.WindowBorder.Hidden;
            WinApi.SetParent(gameWindowHandle, workerw);
            w.Run();
            w.Visible = false;
            w.Dispose();
            Console.ReadKey();
            WinApi.SetWallpaper(wpPath);
            /*if (workerw != null) {
                deviceContext = WinApi.GetDC(workerw);
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
            }*/
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


    }
}
