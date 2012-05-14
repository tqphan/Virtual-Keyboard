using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Forms;

namespace WindowsAPI
{
    public static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int ToUnicodeEx(
            uint wVirtKey,
            uint wScanCode,
            Keys[] lpKeyState,
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags,
            IntPtr dwhkl);
        [DllImport("user32.dll")]
        public static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int ToUnicodeEx(
            uint wVirtKey,
            uint wScanCode,
            VirtualKey[] lpKeyState,
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags,
            IntPtr dwhkl);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern IntPtr GetKeyboardLayout(uint threadId);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern bool GetKeyboardState(VirtualKey[] keyStates);

        [DllImport("user32")]
        public static extern int GetKeyboardState(byte[] pbKeyState);

        [DllImport("user32.dll", ExactSpelling = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hwindow, out uint processId);

        public static uint MapVirtualKeyExWrapper(uint uCode, MapVirtualKeyMapTypes uMapType, IntPtr dwhkl)
        {
            //switch(uMapType)
            //{
            //    case MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC_EX:
            //        if ((VirtualKey)uCode == VirtualKey.VK_NUMLOCK)
            //            return MapVirtualKeyEx(uCode, uMapType, dwhkl) & 0xe000;
            //        break; 
            //    case MapVirtualKeyMapTypes.MAPVK_VSC_TO_VK_EX:
            //        if (uCode == 0xe045)
            //            return MapVirtualKeyEx(uCode ^ 0xe000, uMapType, dwhkl);
            //        break; 
            //}
            return MapVirtualKeyEx(uCode, uMapType, dwhkl); 
        }


        [DllImport("user32.dll")]
        public static extern uint MapVirtualKeyEx(uint uCode, MapVirtualKeyMapTypes uMapType, IntPtr dwhkl);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hwndInsertAfter, int x, int y, int width, int height, uint flags);

        [DllImport("user32.dll")]
        public static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern bool DwmIsCompositionEnabled();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        
    }
}
