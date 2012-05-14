using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Interop; 
using System.ComponentModel;
using WindowsAPI;

namespace Ziyi
{
    public class KeyInput
    {
        private IntPtr m_hkl;
        private uint m_vk;
        private uint m_sc;
        private bool[,] m_rgfDeadKey = new bool[Enum.GetValues(typeof(ShiftState)).Cast<int>().Max() * 2, 2];
        private string[,] m_rgss = new string[Enum.GetValues(typeof(ShiftState)).Cast<int>().Max() * 2, 2];

        public KeyInput(IntPtr hkl, VirtualKey virtualKey)
        {
            this.m_sc = NativeMethods.MapVirtualKeyExWrapper((uint)virtualKey, MapVirtualKeyMapTypes.MAPVK_VK_TO_VSC_EX, hkl);
            this.m_hkl = hkl;
            this.m_vk = (uint)virtualKey;
            InterrogateKeyboardLayout(this.m_hkl, this.m_vk, this.m_sc);
        }

        public KeyInput(IntPtr hkl, uint scanCode)
        {
            this.m_vk = NativeMethods.MapVirtualKeyExWrapper(scanCode, MapVirtualKeyMapTypes.MAPVK_VSC_TO_VK_EX, hkl);
            this.m_hkl = hkl;
            this.m_sc = scanCode;
            InterrogateKeyboardLayout(this.m_hkl, this.m_vk, this.m_sc);
        }

        public KeyInput(uint scanCode)
        {
            this.m_hkl = NativeMethods.GetKeyboardLayout(0);
            this.m_vk = NativeMethods.MapVirtualKeyExWrapper(scanCode, MapVirtualKeyMapTypes.MAPVK_VSC_TO_VK_EX, this.m_hkl);
            this.m_sc = scanCode;
            InterrogateKeyboardLayout(this.m_hkl, this.m_vk, this.m_sc);
        }

        public VirtualKey VirtualKey
        {
            get { return (VirtualKey)this.m_vk; }
        }

        public uint Scancode
        {
            get { return this.m_sc; }
        }

        public string GetShiftState(ShiftState shiftState, bool capsLock)
        {
            if (this.m_rgss[(uint)shiftState, (capsLock ? 1 : 0)] == null)
            {
                return ("");
            }

            return (this.m_rgss[(uint)shiftState, (capsLock ? 1 : 0)]);
        }

        public string GetShiftState(VirtualKey[] lpKeyState)
        {
            ShiftState ss;
            bool capsLock;
            FillShiftState(lpKeyState, out ss, out capsLock);
            return GetShiftState(ss, capsLock);
        }

        public void SetShiftState(ShiftState shiftState, string value, bool isDeadKey, bool capsLock)
        {
            this.m_rgfDeadKey[(uint)shiftState, (capsLock ? 1 : 0)] = isDeadKey;
            this.m_rgss[(uint)shiftState, (capsLock ? 1 : 0)] = value;
        }

        public bool IsDeadKey(ShiftState ss, bool capsLock)
        {
            return m_rgfDeadKey[(int)ss, (capsLock ? 1 : 0)];
        }

        public bool IsEmpty
        {
            get
            {
                for (int i = 0; i < this.m_rgss.GetUpperBound(0); i++)
                {
                    for (int j = 0; j <= 1; j++)
                    {
                        if (this.GetShiftState((ShiftState)i, (j == 1)).Length > 0)
                        {
                            return (false);
                        }
                    }
                }
                return true;
            }
        }

        // 0x53 is the scancode for VirtualKey.VK_DECIMAL
        private void ClearKeyboardBuffer(IntPtr hkl, uint vk = (int)VirtualKey.VK_DECIMAL, uint sc = 0x53)
        {
            
            StringBuilder sb = new StringBuilder(10);
            int rc;
            byte[] lpKeyStateNull = new byte[256];
            do
            {
                rc = NativeMethods.ToUnicodeEx(vk, sc, lpKeyStateNull, sb, sb.Capacity, 0, hkl);
            } while (rc < 0);
        }

        private void FillShiftState(VirtualKey[] lpKeyState, out ShiftState ss, out bool caps)
        {
            ss = ShiftState.None;
            if ((((int)lpKeyState[(int)VirtualKey.VK_LSHIFT] & 0x80) == 0x80)
                && (((int)lpKeyState[(int)VirtualKey.VK_SHIFT] & 0x80) == 0x80))
            {
                ss |= ShiftState.LShft;
            }
            if ((((int)lpKeyState[(int)VirtualKey.VK_RSHIFT] & 0x80) == 0x80)
                && (((int)lpKeyState[(int)VirtualKey.VK_SHIFT] & 0x80) == 0x80))
            {
                ss |= ShiftState.RShft;
            }
            if ((((int)lpKeyState[(int)VirtualKey.VK_LCONTROL] & 0x80) == 0x80)
                && (((int)lpKeyState[(int)VirtualKey.VK_CONTROL] & 0x80) == 0x80))
            {
                ss |= ShiftState.LCtrl;
            }
            if ((((int)lpKeyState[(int)VirtualKey.VK_RCONTROL] & 0x80) == 0x80)
                && (((int)lpKeyState[(int)VirtualKey.VK_CONTROL] & 0x80) == 0x80))
            {
                ss |= ShiftState.RCtrl;
            }
            if ((((int)lpKeyState[(int)VirtualKey.VK_LMENU] & 0x80) == 0x80)
                && (((int)lpKeyState[(int)VirtualKey.VK_MENU] & 0x80) == 0x80))
            {
                ss |= ShiftState.LMenu;
            }
            if ((((int)lpKeyState[(int)VirtualKey.VK_RMENU] & 0x80) == 0x80)
                && (((int)lpKeyState[(int)VirtualKey.VK_MENU] & 0x80) == 0x80))
            {
                ss |= ShiftState.RMenu;
            }
            if (((int)lpKeyState[(int)VirtualKey.VK_CAPITAL] & 0x01) == 0x01)
                caps = true;
            else
                caps = false;
        }

        private void FillKeyState(VirtualKey[] lpKeyState, ShiftState ss, bool fCapsLock)
        {
            if ((ss & ShiftState.LShft) == ShiftState.LShft)
            {
                lpKeyState[(int)VirtualKey.VK_SHIFT] = lpKeyState[(int)VirtualKey.VK_LSHIFT] = (VirtualKey)0x80; 
            }

            if ((ss & ShiftState.RShft) == ShiftState.RShft)
            {
                lpKeyState[(int)VirtualKey.VK_SHIFT] = lpKeyState[(int)VirtualKey.VK_RSHIFT] = (VirtualKey)0x80; 
            }

            if ((ss & ShiftState.LCtrl) == ShiftState.LCtrl)
            {
                lpKeyState[(int)VirtualKey.VK_CONTROL] = lpKeyState[(int)VirtualKey.VK_LCONTROL] = (VirtualKey)0x80;
            }

            if ((ss & ShiftState.RCtrl) == ShiftState.RCtrl)
            {
                lpKeyState[(int)VirtualKey.VK_CONTROL] = lpKeyState[(int)VirtualKey.VK_RCONTROL] = (VirtualKey)0x80;
            }

            if ((ss & ShiftState.LMenu) == ShiftState.LMenu)
            {
                lpKeyState[(int)VirtualKey.VK_MENU] = lpKeyState[(int)VirtualKey.VK_LMENU] = (VirtualKey)0x80;
            }

            if ((ss & ShiftState.RMenu) == ShiftState.RMenu)
            {
                lpKeyState[(int)VirtualKey.VK_MENU] = lpKeyState[(int)VirtualKey.VK_RMENU] = (VirtualKey)0x80;
            }
            
            lpKeyState[(int)VirtualKey.VK_CAPITAL] = (fCapsLock ? (VirtualKey)0x01 : (VirtualKey)0x00);
        }

        //need a better way to do this ......
        static bool IsAltOrShiftAlt(ShiftState ss)
        {
            switch (ss)
            {
                case ShiftState.LMenu: 
                case ShiftState.RMenu:
                case (ShiftState.LMenu | ShiftState.RMenu):
                case (ShiftState.LMenu | ShiftState.LShft):
                case (ShiftState.LMenu | ShiftState.RShft):
                case (ShiftState.RMenu | ShiftState.LShft):
                case (ShiftState.RMenu | ShiftState.RShft):
                case (ShiftState.LMenu | ShiftState.RShft | ShiftState.LShft):
                case (ShiftState.RMenu | ShiftState.RShft | ShiftState.LShft):
                case (ShiftState.LMenu | ShiftState.RMenu | ShiftState.LShft):
                case (ShiftState.LMenu | ShiftState.RMenu | ShiftState.RShft):
                case (ShiftState.LMenu | ShiftState.RMenu | ShiftState.RShft | ShiftState.LShft):
                    return true; 
                default:
                    return false; 
            }
        }

        //need a better way to do this ......
        static bool IsCtrlOrShiftCtrl(ShiftState ss)
        {
            switch (ss)
            {
                case ShiftState.LCtrl:
                case ShiftState.RCtrl:
                case (ShiftState.LCtrl | ShiftState.RCtrl):
                case (ShiftState.LCtrl | ShiftState.LShft):
                case (ShiftState.LCtrl | ShiftState.RShft):
                case (ShiftState.RCtrl | ShiftState.LShft):
                case (ShiftState.RCtrl | ShiftState.RShft):
                case (ShiftState.LCtrl | ShiftState.RShft | ShiftState.LShft):
                case (ShiftState.RCtrl | ShiftState.RShft | ShiftState.LShft):
                case (ShiftState.LCtrl | ShiftState.RCtrl | ShiftState.LShft):
                case (ShiftState.LCtrl | ShiftState.RCtrl | ShiftState.RShft):
                case (ShiftState.LCtrl | ShiftState.RCtrl | ShiftState.RShft | ShiftState.LShft):
                    return true;
                default:
                    return false;
            }
        }

        private void InterrogateKeyboardLayout(IntPtr hkl, uint vk, uint sc)
        {
            if (hkl != IntPtr.Zero)
            {
                if (sc != 0 && (VirtualKey)vk != VirtualKey.None)
                {
                    StringBuilder sbBuffer;     // Scratchpad we use many places

                    //List<DeadKey> alDead = new List<DeadKey>();

                    int maxEnum = Enum.GetValues(typeof(ShiftState)).Cast<int>().Max() * 2; 

                    //loop through all ShiftState enum
                    for (int ss = 0; ss < maxEnum; ss++)
                    {

                        if (IsAltOrShiftAlt((ShiftState)ss))
                        {
                            // Alt and Shift+Alt don't work, so skip them
                            continue;
                        }

                        //loop through each caps state
                        for (int caps = 0; caps <= 1; caps++)
                        {
                            VirtualKey[] lpKeyState = new VirtualKey[256];
                            ClearKeyboardBuffer(hkl);
                            FillKeyState(lpKeyState, (ShiftState)ss, (caps != 0));
                            sbBuffer = new StringBuilder(10);

                            int rc = NativeMethods.ToUnicodeEx(vk, sc,
                                lpKeyState, sbBuffer, sbBuffer.Capacity, 0, hkl);

                            if (rc > 0)
                            {
                                if (sbBuffer.Length == 0)
                                {
                                    // Someone defined NULL on the keyboard; let's coddle them"\u0000"
                                    this.SetShiftState((ShiftState)ss, null, false, (caps != 0));
                                }
                                else
                                {
                                    if ((rc == 1) &&
                                        (IsCtrlOrShiftCtrl((ShiftState)ss)) &&
                                        ((uint)vk == ((uint)sbBuffer[0] + 0x40)))
                                    {
                                        // ToUnicodeEx has an internal knowledge about those
                                        // VK_A ~ VK_Z keys to produce the control characters,
                                        // when the conversion rule is not provided in keyboard
                                        // layout files
                                        continue;
                                    }
                                    this.SetShiftState((ShiftState)ss, sbBuffer.ToString().Substring(0, rc), false, (caps != 0));
                                }
                            }
                            else if (rc < 0)
                            {
                                this.SetShiftState((ShiftState)ss, sbBuffer.ToString().Substring(0, 1), true, (caps != 0));

                                // It's a dead key; let's flush out whats stored in the keyboard state.
                                ClearKeyboardBuffer(hkl);
                                //DeadKey dk = null;
                                //for (int iDead = 0; iDead < alDead.Count; iDead++)
                                //{
                                //    dk = alDead[iDead];
                                //    if (dk.DeadCharacter == kld.GetShiftState(ss, caps != 0)[0])
                                //    {
                                //        break;
                                //    }
                                //    dk = null;
                                //}
                                //if (dk == null)
                                //{
                                //    alDead.Add(ProcessDeadKey(ss, lpKeyState, kld, caps == 1, hkl));
                                //}
                            }
                        }
                    }
                }
            }
        }
    }
}
