using Microsoft.Win32;
using NHotkey.WindowsForms;
using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp.WinForms;
using CefSharp;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using QuickApp.net;

namespace QuickApp
{

    public enum MouseAction
    {
        LeftDown,
        LeftUp,
        RightDown,
        RightUp,
        Middle,
    }
    public enum KeyboardAction
    {
        Down,
        Up,
    }

    public struct MouseOperatorRequest
    {
        MouseAction action;
        UInt16 x;
        UInt16 y;
        UInt16 deatl;
    }

    public struct KeyboardOperatorRequest
    {
        KeyboardAction action;
        byte code;
    }

    public partial class WindowsSreenDisplayForm : Form
    {
        private bool mRunning;
        //windwos screen
        private static readonly object imgLock = new object();
        private static byte[] imgBytes = new byte[1920 * 1080 * 4];
        private static Semaphore mImgSem = new Semaphore(0, int.MaxValue);

        public WindowsSreenDisplayForm()
        {
            mRunning = true;
            InitializeComponent();
            this.Location = new Point(0, 0);
            this.StartPosition = FormStartPosition.Manual;
            this.FormBorderStyle = FormBorderStyle.None;
            this.pbWindowsScreen.ImgDisplayType = WinScreenCtrl.DisplayType.TileBottomRight;
            this.TopMost = true;

            InitWindowCapture();
            ImageSendRecvTest();
        }

        private void ImageSendRecvTest()
        {
            ImageReceiver receiver = new ImageReceiver("192.168.0.100", 40001); //Server
            ImageSender sender = new ImageSender("192.168.0.100", 40001);   //client


            receiver.SetOnImageReceived((img) =>
            {
                pbWindowsScreen.ControllerInvoke(() =>
                {
                    pbWindowsScreen.Image = img;
                });
            });
            Thread sendTh = new Thread(() =>
            {
                Thread.Sleep(50);
                sender.PrepareSend(CloneWindowScreenImageBytes(), 1920, 1080);
                ulong bTick = CUtil.GetCurTickMs();
                const int fps = 60;
                const int intravelMs = 1000 / fps;
                while (mRunning)
                {
                    int useTime = (int)(CUtil.GetCurTickMs() - bTick);
                    if (useTime > intravelMs)
                    {
                        ulong cc_bTick = CUtil.GetCurTickMs();
                        bTick = CUtil.GetCurTickMs();
                        //if (ImageReceiver.SendRecvSem.WaitOne(50))
                        {
                            sender.SendNext(CloneWindowScreenImageBytes());
                            int cc_useTime = (int)(CUtil.GetCurTickMs() - cc_bTick);
                            //Console.WriteLine($"{useTime}");
                        }
                    }
                    else
                    {
                        Thread.Sleep(1);
                    }
                }
            });
            sendTh.Priority = ThreadPriority.Highest;
            sendTh.Start();
        }
        private void InitWindowCapture()
        {
            CNavApi.OnUpdatedDelegate updatedDelegate = new CNavApi.OnUpdatedDelegate(OnWindowsScreenUpdated);
            GCHandle.Alloc(updatedDelegate);
            CNavApi.Start(0, 0, 1920, 1080, false, updatedDelegate);
            new Thread(new ThreadStart(() =>
            {
                WindowsUpdateProcess();
            })).Start();

        }
        //24位
        private static void OnWindowsScreenUpdated(int w, int h, IntPtr data, int size)
        {
            lock (imgLock)
            {
                if (false)
                {
                    Bitmap img = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                    var bd = img.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, img.PixelFormat);
                    var tmp = new byte[w * h * 3];
                    Marshal.Copy(data, tmp, 0, w * h * 3);
                    Marshal.Copy(tmp, 0, bd.Scan0, w * h * 3);
                    img.UnlockBits(bd);
                    img.Save("D://ssss.jpg");
                }
                Marshal.Copy(data, imgBytes, 0, size);
            }
            mImgSem.Release();
        }

        private void WindowsUpdateProcess()
        {
            //Bitmap bitmap = new Bitmap(1920, 1080, PixelFormat.Format32bppArgb);
            while (mRunning)
            {
                //pixel tranform to image
                if (mImgSem.WaitOne(50))
                {
                    try
                    {
                        //pbWindowsScreen.ControllerInvoke(() =>
                        //{
                        //    pbWindowsScreen.Image = null;
                        //});


                        //BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        //    ImageLockMode.ReadWrite, bitmap.PixelFormat);
                        //lock (imgLock)
                        //{
                        //    Marshal.Copy(imgBytes, 0, bitmapData.Scan0, imgBytes.Length);
                        //}
                        //bitmap.UnlockBits(bitmapData);
                        //pbWindowsScreen.ControllerInvoke(() =>
                        //{
                        //    pbWindowsScreen.Image = bitmap;
                        //});
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
        }


        private byte[] CloneWindowScreenImageBytes()
        {
            byte[] mtmpBytes = null;
            lock (imgLock)
            {
                mtmpBytes = new byte[imgBytes.Length];
                imgBytes.CopyTo(mtmpBytes, 0);
            }
            return mtmpBytes;
        }


        private void Form1_Load(object sender, EventArgs e)
        {
        }


        private void WindowsSreenDisplayForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
