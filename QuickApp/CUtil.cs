using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using System.Net;

using System.Collections;
using ICSharpCode.SharpZipLib.GZip;
using System.IO.Compression;
using Lzfse;
using System.Diagnostics;

namespace QuickApp
{
    public enum DrawRop
    {
        SRCCOPY = 0xCC0020,
    }
    public class CUtil
    {

        #region Win32 Api

        [DllImport(User32, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();
        [DllImport(User32, EntryPoint = "ShowCursor")]
        public extern static bool ShowCursor(bool show);
        [DllImport(User32, CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();   //WINAPI 获取当前活动窗体的句柄
        [DllImport(User32)]
        public static extern bool HideCaret(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImportAttribute(Gdi32)]
        public static extern bool BitBlt(
               HandleRef hdcDest,//目标设备的句柄
               int nXDest,//目标对象的左上角x坐标
               int nYDest,//目标对象的左上角Y坐标
               int nWidth,//目标对象的矩形宽度
               int nHeight,//目标对象的矩形长度
               HandleRef hdcSrc,//源设备的句柄
               int nXSrc,//源对象的左上角x坐标
               int nYSrc,//源对象的左上角y坐标
               System.Int32 dwRop//光栅的操作值
               );

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        //安装钩子
        [DllImport(User32, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);
        //卸载钩子
        [DllImport(User32, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);
        //调用下一个钩子
        [DllImport(User32, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport(User32, EntryPoint = "GetMessage")]
        public static extern int GetMessage(out tagMSG lpMsg, IntPtr hwnd, int wMsgFilterMin, int wMsgFilterMax);

        [DllImport(User32, EntryPoint = "DispatchMessage")]
        public static extern int DispatchMessage(ref tagMSG lpMsg);
        [DllImport(User32, EntryPoint = "TranslateMessage")]
        public static extern int TranslateMessage(ref tagMSG lpMsg);

        [DllImport(User32, SetLastError = true, EntryPoint = "GetScrollBarInfo")]
        public static extern int GetScrollBarInfo(IntPtr hWnd, uint idObject, ref Scrollbarinfo psbi);

        [DllImport(User32, SetLastError = true, EntryPoint = "SetScrollPos")]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        [DllImport(User32, SetLastError = true, EntryPoint = "GetScrollPos")]
        public static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport(User32, SetLastError = true, EntryPoint = "GetScrollRange")]
        public static extern int GetScrollRange(IntPtr hWnd, int nBar, ref int lpMinPos, ref int lpMaxPos);

        [DllImport(User32, SetLastError = true, EntryPoint = "SetScrollRange")]
        public static extern int SetScrollRange(IntPtr hWnd, int nBar, int lpMinPos, int lpMaxPos, bool bRedraw);

        [DllImport(User32, SetLastError = true, EntryPoint = "SendMessage")]
        public static extern bool SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport(User32, SetLastError = true, EntryPoint = "SendMessage")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport(User32, SetLastError = true, EntryPoint = "PostMessage")]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        [DllImport(User32, SetLastError = true, EntryPoint = "ShowScrollBar")]
        public static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        [DllImport(User32, SetLastError = true, EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);


        [DllImport("kernel32.dll")]
        public static extern ulong GetTickCount64();
        #endregion
        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int x;
            public int y;
        }
        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStructEx
        {
            public POINT pt;
            public int hwnd;
            public int wHitTestCode;
            public int dwExtraInfo;
            public UInt32 mouseData;
        }
        public class MouseWheelStruct
        {
            public int flage;
            public UInt16 zDelta;
            public UInt16 x;
            public UInt16 y;
        }

        public struct tagMSG
        {
            public int hwnd;
            public uint message;
            public int wParam;
            public long lParam;
            public uint time;
            public int pt;
        }



        #region 获取滚动条信息的相应结构体等

        public const uint OBJID_HSCROLL = 0xFFFFFFFA;  //水平滚动条
        public const uint OBJID_VSCROLL = 0xFFFFFFFB;  //垂直滚动条


        public struct Scrollbarinfo
        {
            public int cbSize;
            public RECT rcScrollBar;
            public int dxyLineButton;
            public int xyThumbTop;    //滑块的Top或Left坐标
            public int xyThumbBottom; //滑块的Bottom或Right坐标
            public int reserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        #endregion

        #region 枚举和常数

        public const string User32 = "user32.dll";
        public const string Gdi32 = "gdi32.dll";
        [Flags]
        public enum ShandowSide
        {
            Left = 1,
            top = 2,
            Right = 4,
            bottom = 8,
            All = 0xf,
        }
        #endregion


        #region Public 方法
        public static string AutoRegCom(string strCmd)
        {
            string rInfo;
            try
            {
                Process myProcess = new Process();
                ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("cmd.exe");
                myProcessStartInfo.UseShellExecute = false;
                myProcessStartInfo.CreateNoWindow = true;
                myProcessStartInfo.RedirectStandardOutput = true;
                myProcess.StartInfo = myProcessStartInfo;
                myProcessStartInfo.Arguments = "/c " + strCmd;
                myProcess.Start();
                StreamReader myStreamReader = myProcess.StandardOutput;
                rInfo = myStreamReader.ReadToEnd();
                myProcess.Close();
                rInfo = strCmd + "\r\n" + rInfo;
                return rInfo;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public static (byte[], int) LzfseCompress(byte[] inBytes, int offset, int length, bool twice)
        {
            byte[] outBytes = new byte[inBytes.Length];
            int outSize = Lzfse.LzfseCompressor.Compress(inBytes, offset, length, outBytes, 0, outBytes.Length);
            if (twice)
            {
                byte[] outBytesSecond = new byte[(int)(outSize * 1.5)];
                outSize = Lzfse.LzfseCompressor.Compress(outBytes, 0, outSize, outBytesSecond, 0, outBytesSecond.Length);
                outBytes = outBytesSecond;
            }
            return (outBytes, outSize);
        }
        public static (byte[], int) LzfseCompress(byte[] inBytes, bool twice)
        {
            return LzfseCompress(inBytes, 0, inBytes.Length, twice);
        }
        public static (byte[], int) LzfseDeCompress(byte[] inBytes, int offset, int outputExpectSize, bool twice)
        {
            byte[] outBytes = new byte[outputExpectSize];
            int outSize = Lzfse.LzfseCompressor.Decompress(inBytes, offset, inBytes.Length - offset, outBytes, 0, outBytes.Length);
            if (twice)
            {
                byte[] outBytesSecond = new byte[outputExpectSize];
                outSize = Lzfse.LzfseCompressor.Decompress(outBytes, 0, outSize, outBytesSecond, 0, outBytesSecond.Length);
                outBytes = outBytesSecond;
            }
            return (outBytes, outSize);
        }
        public static (byte[], int) LzfseDeCompress(byte[] inBytes, int outputExpectSize, bool twice)
        {
            return LzfseDeCompress(inBytes, 0, outputExpectSize, twice);
        }

        public static ulong GetCurTickMs()
        {
            return GetTickCount64();
        }

        public static byte[] Int2Bytes(int val)
        {
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.GetBytes(val).Reverse().ToArray();
            }
            return BitConverter.GetBytes(val);
        }
        public static int Bytes2Int(byte[] data, int offset)
        {
            var tmp = new byte[4];
            Array.Copy(data, offset, tmp, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                return BitConverter.ToInt32(tmp.Reverse().ToArray(), 0);
            }
            return BitConverter.ToInt32(data, 0);
        }

        public static byte[] CompressBytes(byte[] bytes)
        {
            return CompressBytes(bytes, 0, bytes.Length);
        }
        public static byte[] CompressBytes(byte[] bytes, int offset, int lenght)
        {
            using (MemoryStream compressStream = new MemoryStream())
            {
                using (var zipStream = new GZipStream(compressStream, CompressionMode.Compress))
                    zipStream.Write(bytes, offset, lenght);
                return compressStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] bytes)
        {
            return Decompress(bytes, 0, bytes.Length);
        }
        public static byte[] Decompress(byte[] bytes, int offset)
        {
            return Decompress(bytes, offset, bytes.Length - offset);
        }

        public static byte[] Decompress(byte[] bytes, int offset, int count)
        {
            using (var compressStream = new MemoryStream(bytes, offset, count))
            {
                using (var zipStream = new GZipStream(compressStream, CompressionMode.Decompress))
                {
                    using (var resultStream = new MemoryStream())
                    {
                        zipStream.CopyTo(resultStream);
                        return resultStream.ToArray();
                    }
                }
            }
        }


        public static string GetImageBase64(String filePath, Size defaultSize)
        {
            Bitmap bmp = null;
            try
            {
                var suffix = Path.GetExtension(filePath).ToLower();
                var suffixName = suffix == ".png" ? ImageFormat.Png
                    : suffix == ".jpg" || suffix == ".jpeg"
                        ? ImageFormat.Jpeg
                        : suffix == ".bmp"
                            ? ImageFormat.Bmp
                            : suffix == ".gif"
                                ? ImageFormat.Gif
                                : ImageFormat.Jpeg;

                string result = string.Empty;
                if (Directory.Exists(filePath))
                {
                    bmp = new Bitmap(filePath);
                }
                else
                {
                    bmp = CreateBitmap(defaultSize, Color.Black);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, suffixName);
                    byte[] arr = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(arr, 0, (int)ms.Length);
                    ms.Close();
                    result = Convert.ToBase64String(arr);
                }
                return result;
            }
            catch (Exception exp)
            {
                return null;
            }
            finally
            {
                if (bmp != null)
                    bmp.Dispose();
            }
        }

        public static Image GetImage(String filePath, Size defaultSize)
        {
            Image tempImg = null;
            FileStream stream = null;
            try
            {
                stream = File.Open(filePath, FileMode.Open);
                byte[] rBytes = new byte[stream.Length];
                stream.Read(rBytes, 0, (int)stream.Length);
                stream.Close();
                Stream sTemp = new MemoryStream(rBytes);
                tempImg = Image.FromStream(sTemp);
            }
            catch
            {
                tempImg?.Dispose();
                stream?.Close();
                tempImg = CreateBitmap(defaultSize, Color.White);
                Pen pen = new Pen(Color.Red, 2);
                using (Graphics g = Graphics.FromImage(tempImg))
                {
                    var rect = new Rectangle(0, 0, defaultSize.Width, defaultSize.Height);
                    var borderRect = rect;
                    borderRect.Inflate(-(int)pen.Width / 2, -(int)pen.Width / 2);
                    g.DrawRectangle(pen, borderRect);
                    g.DrawLine(pen, rect.Left, rect.Top, rect.Right, rect.Bottom);
                    g.DrawLine(pen, rect.Right, rect.Top, rect.Left, rect.Bottom);

                    //System.Drawing.Font font = new System.Drawing.Font("Georgia", 10);
                    //Size textHoldSize = g.MeasureString("Error", font, defaultSize.Width).ToSize();
                    //g.DrawString("Error", font, Brushes.Red, (defaultSize.Width - textHoldSize.Width) / 2,
                    //    (defaultSize.Height - textHoldSize.Height) / 2);
                }


            }
            finally
            {
                stream?.Close();
            }
            return tempImg;
        }


        public static Bitmap CreateBitmap(Size size, Color color)
        {
            return CreateBitmap(size.Width, size.Height, color);
        }

        public static Bitmap CreateBitmap(int width, int height, Color color)
        {
            Bitmap bmp = new Bitmap(width, height);
            SolidBrush solidBrush = new SolidBrush(color);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                graphics.FillRectangle(solidBrush, 0, 0, width, height);
            }
            return bmp;
        }

        public static Bitmap CreateBitmap(int width, int height, int a, int r, int g, int b)
        {
            Bitmap bmp = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(bmp))
            {
                SolidBrush solidBrush = new SolidBrush(Color.FromArgb(a, r, g, b));//这里修改颜色
                graphics.FillRectangle(solidBrush, 0, 0, width, height);
            }
            return bmp;
        }

        public static Bitmap CreateBitmap(int width, int height, PixelFormat pf, byte[] pixelData)
        {
            Bitmap bmp = new Bitmap(width, height, pf);
            Rectangle lockRect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bitmapdata = bmp.LockBits(lockRect, ImageLockMode.WriteOnly, pf);
            int channle = (int)pf >> 11;
            channle &= 0xf;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    byte r, g, b;
                    int pixelPos = channle * (i * bmp.Width + j);
                    Marshal.WriteByte(bitmapdata.Scan0, pixelPos + 0, pixelData[pixelPos + 0]);
                    Marshal.WriteByte(bitmapdata.Scan0, pixelPos + 1, pixelData[pixelPos + 1]);
                    Marshal.WriteByte(bitmapdata.Scan0, pixelPos + 2, pixelData[pixelPos + 2]);
                }
            }
            //bitmapdata.Scan0 = pixelData;
            bmp.UnlockBits(bitmapdata);
            return bmp;
        }

        /// <summary>
        /// bmp图片位深转换
        /// </summary>
        /// <param name="srcImg">srcImg==dstImg时，返回前释放srcImg</param>
        /// <param name="dstImg">返回前释放dstImg</param>
        /// <param name="dstPixelFormat"></param>
        public static void BmpDepthTranform(Image srcImg, ref Image dstImg, PixelFormat dstPixelFormat)
        {
            Image midImg = null;
            if (srcImg.PixelFormat == dstPixelFormat)
            {
                midImg = srcImg.Clone() as Image;
                goto DONE;
            }
            Int64 dstDepthVal = 24L;
            switch (dstPixelFormat)
            {
                case PixelFormat.Format1bppIndexed:
                    dstDepthVal = 1L;
                    break;
                case PixelFormat.Format8bppIndexed:
                    dstDepthVal = 8L;
                    break;
                case PixelFormat.Format24bppRgb:
                    dstDepthVal = 24L;
                    break;
                default:
                    midImg = null;
                    goto DONE;
            }
            if (srcImg.PixelFormat == PixelFormat.Format8bppIndexed && dstPixelFormat == PixelFormat.Format24bppRgb)
            {
                //8 - 24
                midImg = new Bitmap(srcImg.Width, srcImg.Height, dstPixelFormat);
                for (int i = 0; i < srcImg.Height; i++)
                {
                    for (int j = 0; j < srcImg.Width; j++)
                    {
                        ((Bitmap)midImg).SetPixel(j, i, ((Bitmap)srcImg).GetPixel(j, i));
                    }
                }
                goto DONE;
            }

            if ((srcImg.PixelFormat == PixelFormat.Format24bppRgb || srcImg.PixelFormat == PixelFormat.Format32bppArgb) && dstPixelFormat == PixelFormat.Format1bppIndexed)
            {
                //24/32 - 1
                midImg = Bmp24_32bitTo1bitUseBitmapData(srcImg);
                goto DONE;
            }

            if (srcImg.PixelFormat == PixelFormat.Format1bppIndexed && dstPixelFormat == PixelFormat.Format24bppRgb)
            {
                //1 - 24
                midImg = Bmp1bitTo24bitUseBitmapData(srcImg);
                goto DONE;
            }

            EncoderParameters myEncoderParameters = new EncoderParameters(2);
            ImageCodecInfo myImageCodecInfo = GetEncoderInfo("image/tiff");
            EncoderParameter myEncoderParameter0 = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100);
            EncoderParameter myEncoderParameter1 = new EncoderParameter(System.Drawing.Imaging.Encoder.ColorDepth, dstDepthVal);

            myEncoderParameters.Param[0] = myEncoderParameter0;
            myEncoderParameters.Param[1] = myEncoderParameter1;
            Stream outStream = new MemoryStream();
            srcImg.Save(outStream, myImageCodecInfo, myEncoderParameters);

            //outStream.Close();    //Close导致midImg内存溢出
            midImg = Image.FromStream(outStream);
            DONE:
            //src=dst && src!=mid 释放src, src!=dst 释放dst
            if (Object.ReferenceEquals(srcImg, dstImg))
            {
                srcImg?.Dispose();
            }
            else
            {
                dstImg?.Dispose();
            }
            dstImg = midImg;
        }


        /// <summary>
        /// 生成圆角
        /// </summary>
        /// <param name="rectF"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static GraphicsPath CreateRoundRect(RectangleF rectF, float radius)
        {
            return CreateRoundRect(rectF.X, rectF.Y, rectF.Width, rectF.Height, radius);
        }
        /// <summary>
        /// 生成圆角
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static GraphicsPath CreateRoundRect(float x, float y, float width, float height, float radius)
        {
            GraphicsPath gp = new GraphicsPath();
            gp.AddLine(x + radius, y, x + width - (radius * 2), y);
            gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
            gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
            gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90);
            gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
            gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
            gp.AddLine(x, y + height - (radius * 2), x, y + radius);
            gp.AddArc(x, y, radius * 2, radius * 2, 180, 90);
            gp.CloseFigure();
            return gp;
        }
        /// <summary>
        /// 生成圆角
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        public static GraphicsPath CreateRoundRect(Rectangle rect, float radius)
        {
            return CreateRoundRect(rect.X, rect.Y, rect.Width, rect.Height, radius);
        }
        /// <summary>
        /// 颜色混合，无透明度
        /// </summary>
        /// <param name="backgroundColor"></param>
        /// <param name="frontColor"></param>
        /// <param name="blend"></param>
        /// <returns></returns>
        public static Color BlendColor(Color backgroundColor, Color frontColor, double blend)
        {
            double ratio = blend / 255d;
            double invRatio = 1d - ratio;
            int r = (int)((backgroundColor.R * invRatio) + (frontColor.R * ratio));
            int g = (int)((backgroundColor.G * invRatio) + (frontColor.G * ratio));
            int b = (int)((backgroundColor.B * invRatio) + (frontColor.B * ratio));
            return Color.FromArgb(r, g, b);
        }
        /// <summary>
        /// 颜色混合，无透明度
        /// </summary>
        /// <param name="backgroundColor"></param>
        /// <param name="frontColor"></param>
        /// <returns></returns>
        public static Color BlendColor(Color backgroundColor, Color frontColor)
        {
            return BlendColor(backgroundColor, frontColor, frontColor.A);
        }

        /// <summary>
        /// 替换非透明的颜色为newRgb，用于鼠标悬浮图标变色
        /// </summary>
        /// <param name="inBitmap"></param>
        /// <returns></returns>
        public static Image CreateActivateImage(Bitmap inBitmap, Color newRgb)
        {
            var outBitmap = inBitmap.Clone() as Bitmap;
            var bitData = ((Bitmap)outBitmap).LockBits(new Rectangle(0, 0, outBitmap.Width, outBitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite,
                   outBitmap.PixelFormat);
            var ptr = bitData.Scan0;
            for (int row = 0; row < bitData.Height; row++)
            {
                for (int col = 0; col < bitData.Stride; col += 4)
                {
                    int pixelIdx = (row * bitData.Stride + col);
                    byte alpha = Marshal.ReadByte(ptr, pixelIdx + 3);
                    if (alpha > 0)
                    {
                        int val = newRgb.ToArgb() & 0xffffff;
                        val |= alpha << 24;
                        Marshal.WriteInt32(ptr, pixelIdx, val);
                    }
                }
            }
            outBitmap.UnlockBits(bitData);
            return outBitmap;
        }

        public static void DisplayPictureWithDialog(Image image)
        {

            //MaterialSkin.Controls.MaterialDialog cbd = new MaterialSkin.Controls.MaterialDialog(
            //    MaterialSkin.Controls.MaterialDialog.DialogDisplayType.OnlyConfirm, "Display Picture");
            //Size size = new Size(10, 10);
            //try
            //{
            //    size = image.Size;
            //    PictureBox pictureBox = new PictureBox();
            //    pictureBox.Image = image;
            //    pictureBox.Size = size;
            //    pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
            //    cbd.Controls.Add(pictureBox);
            //    cbd.ControlBox = false;
            //    cbd.Sizable = true;
            //    cbd.Size = new Size(cbd.Width + size.Width, cbd.Height + size.Height);
            //    pictureBox.Location = new Point((cbd.Width - pictureBox.Width) / 2, (cbd.Height - pictureBox.Height) / 2);
            //}
            //catch
            //{

            //}
            //cbd.ShowDialog();
        }

        /// <summary>
        /// 在大小为drawSize的矩形内画阴影
        /// </summary>
        /// <param name="g"></param>
        /// <param name="drawSize">阴影外围矩形大小</param>
        /// <param name="shandowWidth">阴影大小</param>
        /// <param name="radius">圆角大小</param>
        /// <param name="side">阴影环绕边</param>
        public static void DrawShawdow(Graphics g, Size drawSize, int shandowWidth, int radius, Color shandowColor, ShandowSide side)
        {
            var brushRect = new Rectangle(0, 0, drawSize.Width, drawSize.Height);
            if (brushRect.Width < brushRect.Height)
            {
                brushRect.Width = brushRect.Height;
            }
            else
            {
                brushRect.Height = brushRect.Width;
            }
            //生成一个正方形(圆角)渐变画刷,阴影在图像形内
            PathGradientBrush pgb = new PathGradientBrush(CreateRoundRect(0, 0, brushRect.Width, brushRect.Height, radius));
            //从外到内渐变颜色
            Color[] colors =
            {
                Color.FromArgb(0),
                shandowColor,
                Color.FromArgb(0),
                Color.FromArgb(0),
            };
            float[] relativePositions =
            {
                0f,
                (float)(shandowWidth*2.0/(float)brushRect.Height),
                (float)(shandowWidth*2.0/(float)brushRect.Height),
                1f,
            };

            ColorBlend colorBlend = new ColorBlend();
            colorBlend.Colors = colors;
            colorBlend.Positions = relativePositions;
            pgb.InterpolationColors = colorBlend;
            //g.SmoothingMode = SmoothingMode.HighQuality;  //当使用枚举指定Graphics.SmoothingMode属性时, 它不会影响由路径渐变画笔SmoothingMode填充的区域
            int curWidth = shandowWidth + radius;

            if (side.HasFlag(ShandowSide.Left))
            {
                g.FillRectangle(pgb, new Rectangle(0, curWidth, shandowWidth, drawSize.Height - 2 * curWidth));
            }
            if (side.HasFlag(ShandowSide.top))
            {
                g.FillRectangle(pgb, new Rectangle(curWidth, 0, drawSize.Width - 2 * curWidth, shandowWidth));
            }
            Point offsetOrigin = new Point(brushRect.Width - drawSize.Width, brushRect.Height - drawSize.Height);
            pgb.TranslateTransform(-offsetOrigin.X, -offsetOrigin.Y); //画刷偏移
            if (side.HasFlag(ShandowSide.Right))
            {
                g.FillRectangle(pgb, new Rectangle(drawSize.Width - shandowWidth, curWidth, shandowWidth, drawSize.Height - 2 * curWidth));
            }
            if (side.HasFlag(ShandowSide.bottom))
            {
                g.FillRectangle(pgb, new Rectangle(curWidth, drawSize.Height - shandowWidth, drawSize.Width - 2 * curWidth, shandowWidth));
            }
            pgb.ResetTransform();
            if (side.HasFlag(ShandowSide.Left) && side.HasFlag(ShandowSide.top)) //左上角
            {
                g.FillRectangle(pgb, new Rectangle(0, 0, curWidth, curWidth));
            }
            pgb.TranslateTransform(0, -offsetOrigin.Y);
            if (side.HasFlag(ShandowSide.Left) && side.HasFlag(ShandowSide.bottom))
            {
                g.FillRectangle(pgb, new Rectangle(0, drawSize.Height - curWidth, curWidth, curWidth));
            }
            pgb.ResetTransform();
            pgb.TranslateTransform(-offsetOrigin.X, 0);

            if (side.HasFlag(ShandowSide.Right) && side.HasFlag(ShandowSide.top))
            {
                g.FillRectangle(pgb, new Rectangle(drawSize.Width - curWidth, 0, curWidth, curWidth));
            }
            pgb.ResetTransform();
            pgb.TranslateTransform(-offsetOrigin.X, -offsetOrigin.Y);
            if (side.HasFlag(ShandowSide.Right) && side.HasFlag(ShandowSide.bottom))
            {
                g.FillRectangle(pgb, new Rectangle(drawSize.Width - curWidth, drawSize.Height - curWidth, curWidth, curWidth));
            }

        }
        /// <summary>
        /// DNS域名解析成Ip地址
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static List<string> GetHostIpAddress(string url)
        {
            List<string> ipList = new List<string>();
            try
            {
                IPHostEntry IPHost = Dns.GetHostEntry(url);
                IPAddress[] addr = IPHost.AddressList;
                foreach (var el in addr)
                {
                    ipList.Add(el.ToString());
                }
            }
            catch { }
            return ipList;
        }
        #endregion
        #region Private 方法

        /// <summary>
        /// 图片1位转24位
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        private static Image Bmp1bitTo24bitUseBitmapData(Image srcImg)
        {
            Image dstImg = new Bitmap(srcImg.Width, srcImg.Height, PixelFormat.Format24bppRgb);
            BitmapData srcBitmapData = (srcImg as Bitmap).LockBits(
                new Rectangle(0, 0, srcImg.Width, srcImg.Height), ImageLockMode.ReadOnly, srcImg.PixelFormat);
            BitmapData dstBitmapData = (dstImg as Bitmap).LockBits(
                new Rectangle(0, 0, dstImg.Width, dstImg.Height), ImageLockMode.ReadOnly, dstImg.PixelFormat);
            int a = srcBitmapData.Stride;
            int c = srcBitmapData.Width;
            //Stride行4字节对齐后的字节数
            for (int i = 0; i < srcImg.Height; i++)
            {
                for (int j = 0; j < srcImg.Width; j++)
                {
                    byte r, g, b;
                    r = g = b = 0;
                    if (srcImg.PixelFormat == PixelFormat.Format1bppIndexed)
                    {
                        int bitPos = i * (srcBitmapData.Stride * 8) + j;
                        byte byteVal = Marshal.ReadByte(srcBitmapData.Scan0, bitPos / 8);
                        r = ((byteVal >> (7 - bitPos % 8)) & 0x01) == 0 ? (byte)0x00 : (byte)0xff;
                        int writePos = i * dstBitmapData.Stride + j * 3;
                        Marshal.WriteByte(dstBitmapData.Scan0, writePos, b);
                        Marshal.WriteByte(dstBitmapData.Scan0, writePos + 1, g);
                        Marshal.WriteByte(dstBitmapData.Scan0, writePos + 2, r);
                    }
                }
            }

            (srcImg as Bitmap).UnlockBits(srcBitmapData);
            (dstImg as Bitmap).UnlockBits(dstBitmapData);
            return dstImg;
        }

        /// <summary>
        /// 图片 24/32位转1位
        /// </summary>
        /// <param name="srcImg"></param>
        /// <returns></returns>
        private static Image Bmp24_32bitTo1bitUseBitmapData(Image srcImg)
        {
            Image dstImg = new Bitmap(srcImg.Width, srcImg.Height, PixelFormat.Format1bppIndexed);
            BitmapData srcBitmapData = (srcImg as Bitmap).LockBits(
                new Rectangle(0, 0, srcImg.Width, srcImg.Height), ImageLockMode.ReadOnly, srcImg.PixelFormat);
            BitmapData dstBitmapData = (dstImg as Bitmap).LockBits(
                new Rectangle(0, 0, dstImg.Width, dstImg.Height), ImageLockMode.ReadOnly, dstImg.PixelFormat);
            int byteCount = 3;
            if (srcImg.PixelFormat == PixelFormat.Format32bppArgb)
            {
                byteCount = 4;
            }
            //Stride行4字节对齐后的字节数
            for (int i = 0; i < srcImg.Height; i++)
            {
                byte r, g, b, wVal;
                wVal = 0;
                for (int j = 0; j < srcImg.Width; j++)
                {
                    r = g = b = 0;
                    byte bitVal = 0;
                    b = Marshal.ReadByte(srcBitmapData.Scan0, i * srcBitmapData.Stride + j * byteCount);
                    g = Marshal.ReadByte(srcBitmapData.Scan0, i * srcBitmapData.Stride + j * byteCount + 1);
                    r = Marshal.ReadByte(srcBitmapData.Scan0, i * srcBitmapData.Stride + j * byteCount + 2);
                    if (r == 255 && g == 0 && b == 0)   //板的透明色为(r 255,g 0,b 0)
                    {
                        bitVal = 1;
                    }
                    wVal |= (byte)(bitVal << (7 - j % 8));
                    if ((j + 1) % 8 == 0)
                    {
                        Marshal.WriteByte(dstBitmapData.Scan0, i * dstBitmapData.Stride + j / 8, wVal);
                        wVal = 0;
                    }
                }
                if (srcImg.Width % 8 != 0)
                {
                    Marshal.WriteByte(dstBitmapData.Scan0, i * dstBitmapData.Stride + srcImg.Width / 8, wVal);
                }
            }
            (srcImg as Bitmap).UnlockBits(srcBitmapData);
            (dstImg as Bitmap).UnlockBits(dstBitmapData);
            ColorPalette palettes = dstImg.Palette;
            palettes.Entries[0] = Color.Black;
            palettes.Entries[1] = Color.Red;
            dstImg.Palette = palettes;
            return dstImg;
        }


        /// <summary>
        /// 获取指定的编解码器
        /// </summary>
        /// <param name="mimeType">图片mime类型</param>
        /// <returns></returns>
        private static ImageCodecInfo GetEncoderInfo(String mimeType)
        {
            int j;
            ImageCodecInfo[] encoders;
            encoders = ImageCodecInfo.GetImageEncoders();
            for (j = 0; j < encoders.Length; ++j)
            {
                if (encoders[j].MimeType == mimeType)
                    return encoders[j];
            }
            return null;
        }
        #endregion

    }




}
