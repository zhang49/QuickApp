using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QuickApp
{
    public static class Extension
    {

        /// <summary>
        /// 检查val 是否位于[baseVal - distance,baseVal + distance]区间内
        /// </summary>
        /// <param name="val"></param>
        /// <param name="baseVal"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static bool IsInDeviation(this int val, int baseVal, int distance)
        {
            if (val >= baseVal - distance && val <= baseVal + distance)
            {
                return true;
            }
            return false;
        }
        public static bool IsInRange(this int val, int low, int up)
        {
            if (val >= low && val <= up)
            {
                return true;
            }
            return false;
        }
        public static void Loop(this int targetVal, Action<int> action)
        {
            for (int i = 0; i < targetVal; i++)
            {
                action(i);
            }
        }

        public static void ControllerInvoke(this Control ctrl, Action action)
        {
            if (ctrl?.IsHandleCreated == true && !ctrl?.IsDisposed == true)
            {
                ctrl.Invoke(new ThreadStart(action));
            }
        }

        public static string GetFilePathTopestName(this string filePath)
        {
            int subIdx = int.MaxValue;
            int charIdx = -1;
            bool flag = false;
            for (int i = filePath.Length - 1; i >= 0; i--)
            {
                if (filePath[i] == '/' || filePath[i] == '\\')
                {
                    if (flag)
                    {
                        subIdx = i;
                        break;
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        subIdx = i - 1;
                    }
                    if (!flag)
                    {
                        charIdx = i;
                    }
                    flag = true;
                }
            }
            if (subIdx != int.MaxValue)
            {
                return filePath.Substring(subIdx + 1, charIdx + 1 - (subIdx + 1));
            }
            return "";
        }
        private static readonly DateTime utc_time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static uint ConvertTime(this in DateTime time)
        {
            return (uint)(Convert.ToInt64(time.Subtract(utc_time).TotalMilliseconds) & 0xffffffff);
        }

        private static readonly DateTimeOffset utc1970 = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static uint ConvertTime(this in DateTimeOffset time)
        {
            return (uint)(Convert.ToInt64(time.Subtract(utc1970).TotalMilliseconds) & 0xffffffff);
        }


        public static int StructSize<T>(this T st) where T : struct
        {
            return Marshal.SizeOf<T>();
        }
        public static byte[] ToBytes<T>(this T st) where T : struct
        {
            byte[] outData = new byte[Marshal.SizeOf<T>()];
            IntPtr intPtr = Marshal.UnsafeAddrOfPinnedArrayElement(outData, 0);
            Marshal.StructureToPtr(st, intPtr, false);
            return outData;
        }

        public static bool ToStruct<T>(this byte[] inBytes, out T st, int offset) where T : struct
        {
            if (inBytes.Length < Marshal.SizeOf<T>())
            {
                st = default(T);
                return false;
            }
            IntPtr intPtr = Marshal.UnsafeAddrOfPinnedArrayElement(inBytes, offset);
            st = Marshal.PtrToStructure<T>(intPtr);
            return true;
        }

        public static bool ToStruct<T>(this byte[] inBytes, out T st) where T : struct
        {
            return ToStruct(inBytes, out st, 0);
        }
    }
}
