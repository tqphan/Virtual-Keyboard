using System;
using System.Runtime.InteropServices;
using System.Windows; 

namespace WindowsAPI
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public MARGINS(Thickness t)
        {
            cxLeftWidth = (int)t.Left;
            cxRightWidth = (int)t.Right;
            cyTopHeight = (int)t.Top;
            cyBottomHeight = (int)t.Bottom;
        }
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    };
}
