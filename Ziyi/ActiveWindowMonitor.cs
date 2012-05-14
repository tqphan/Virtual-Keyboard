using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Ziyi
{
    public class ActiveWindowMonitor
    {
        public delegate void ActiveWindowChangedHandler(object sender, String windowHeader, IntPtr hwnd, IntPtr hkl);

        public event ActiveWindowChangedHandler OnActiveWindowChanged;
        public event ActiveWindowChangedHandler OnWindowMinimized;
        public event ActiveWindowChangedHandler OnWindowRestored;

        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread,
            uint dwmsEventTime);

        const uint EVENT_SYSTEM_MINIMIZESTART = 0x16;
        const uint EVENT_SYSTEM_MINIMIZEEND = 0x17; 
        const uint WINEVENT_OUTOFCONTEXT = 0;
        const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax,
            IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc,
            uint idProcess, uint idThread, uint dwFlags);

        IntPtr m_hhook = IntPtr.Zero;
        private WinEventDelegate _winEventProc;

        public ActiveWindowMonitor()
        {
            _winEventProc = new WinEventDelegate(WinEventProc);
        }

        public ActiveWindowMonitor(bool start): this()
        {
            if (start)
                Start(); 
        }

        public void Start()
        {
            if (m_hhook == IntPtr.Zero)
            {
                m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND,
                    EVENT_SYSTEM_MINIMIZEEND, IntPtr.Zero, _winEventProc,
                    0, 0, WINEVENT_OUTOFCONTEXT);
            }
        }

        public void Stop()
        {
            if (m_hhook != IntPtr.Zero)
            {
                UnhookWinEvent(m_hhook);
                m_hhook = IntPtr.Zero;
            }
        }

        void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd,
            int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == EVENT_SYSTEM_FOREGROUND)
            {
                if (OnActiveWindowChanged != null)
                    OnActiveWindowChanged(this, GetActiveWindowTitle(hwnd), hwnd, GetKeyboardLayoutHandle(hwnd));
            }
            else if (eventType == EVENT_SYSTEM_MINIMIZEEND)
            {
                if (OnWindowRestored != null)
                    OnWindowRestored(this, GetActiveWindowTitle(hwnd), hwnd, GetKeyboardLayoutHandle(hwnd));
            }
            else if (eventType == EVENT_SYSTEM_MINIMIZESTART)
            {
                if (OnWindowMinimized != null)
                    OnWindowMinimized(this, GetActiveWindowTitle(hwnd), hwnd, GetKeyboardLayoutHandle(hwnd));
            }
        }

        private IntPtr GetKeyboardLayoutHandle(IntPtr hwnd)
        {
            uint currentProcessID;
            uint currentWindowThreadID = WindowsAPI.NativeMethods.GetWindowThreadProcessId(hwnd, out currentProcessID);
            return WindowsAPI.NativeMethods.GetKeyboardLayout(currentWindowThreadID); 
        }

        private string GetActiveWindowTitle(IntPtr hwnd)
        {
            StringBuilder Buff = new StringBuilder(500);
            WindowsAPI.NativeMethods.GetWindowText(hwnd, Buff, Buff.Capacity);
            return Buff.ToString();
        }

        ~ActiveWindowMonitor()
        {
            UnhookWinEvent(m_hhook);
        }
    }
}
