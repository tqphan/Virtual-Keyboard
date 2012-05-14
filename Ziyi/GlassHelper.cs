using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows;
using System.Windows.Media; 
using WindowsAPI; 

namespace Ziyi
{
    public class GlassHelper 
    {
        
        public static bool ExtendGlassFrame(Window window, Thickness margin) 
        {
            if (!NativeMethods.DwmIsCompositionEnabled()) 
                return false;
            IntPtr hwnd = new WindowInteropHelper(window).Handle; 
            if (hwnd == IntPtr.Zero)
                throw new InvalidOperationException( "The Window must be shown before extending glass.");
            // Set the background to transparent from both the WPF and Win32 perspectives 
            window.Background = Brushes.Transparent; 
            HwndSource.FromHwnd(hwnd).CompositionTarget.BackgroundColor = Colors.Transparent;
            MARGINS margins = new MARGINS(margin);
            NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins); 
            return true;
        } 
    }
}
