using System;
using System.Runtime.InteropServices;

namespace WindowsAPI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWPOS
    {
        public IntPtr hwnd;
        public IntPtr hwndInsertAfter;
        public int x;
        public int y;
        public int cx;
        public int cy;
        public UInt32 flags;
    };
}
