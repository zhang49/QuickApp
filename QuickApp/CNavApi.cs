using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuickApp
{
    public class CNavApi
    {

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]

        public delegate void OnUpdatedDelegate(int w, int h, IntPtr data, int size);
        private const string DllName = "CNavApi.dll";

        //typedef void (* JoystickCb) (int axis, float value);
        //int API StartJoystick(JoystickCb joystickCb)
        // 接口定义
        [DllImport(DllName, CallingConvention = CallingConvention.StdCall)]
        private static extern int Init(int l, int t, int w, int h, int outputBits, int verticalFlip, OnUpdatedDelegate call);

        public static int Start(int l, int t, int w, int h, bool verticalFlip, OnUpdatedDelegate onUpdated)
        {
            return Init(l, t, w, h, 24, verticalFlip ? 1 : 0, onUpdated);
        }


    }
}
