using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Globalization;
using System.Threading; 
using WindowsAPI; 

namespace Ziyi
{
    public class SingleIputKey: KeyBase
    {
        #region Protected Data

        protected KeyInput keyInput = new KeyInput(0);
        protected bool repeating = false;
        protected bool isSimulating = false;
        protected RepeatTimer repeater = new RepeatTimer();
        protected ShiftState shiftState = ShiftState.None;

        #endregion 

        #region Properties 
        public KeyInput KeyInput
        {
            get
            {
                return this.keyInput;
            }
            set
            {
                this.keyInput = value;
            }
        }
        public bool Repeating
        {
            get
            {
                return this.repeating;
            }
            set
            {
                this.repeating = value;
            }
        }
        public bool IsSimulating
        {
            get
            {
                return this.isSimulating;
            }
            private set
            {
                this.isSimulating = value;
            }
        }

        public ShiftState ShiftState
        {
            get
            {
                return shiftState;
            }
        }

        #endregion

        #region Constructors 

        public SingleIputKey()
            : base()
        {
            this.repeater.Tick += new EventHandler(repeater_Tick);
        }

        public SingleIputKey(string XmlFragment)
            : this()
        {
            this.FromXml(XmlFragment); 
        }

        public SingleIputKey(XmlNodeList keyNodes)
            : base(keyNodes)
        {
            this.repeater.Tick += new EventHandler(repeater_Tick);
            for (int k = 0; k < keyNodes.Count; k++)
            {
                switch (keyNodes[k].Name)
                {
                    case "scancode":
                        uint sc = 0;
                        uint.TryParse(keyNodes[k].InnerText, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out sc);
                        this.keyInput = new KeyInput(sc);
                        if (this.label == null)
                            this.Content = this.keyInput.GetShiftState(ShiftState.None, false); 
                        break;
                    case "repeating":
                        if (keyNodes[k].InnerText == "yes")
                            this.Repeating = true;
                        else if (keyNodes[k].InnerText == "no")
                            this.Repeating = false;
                        break;
                }
            }
        }

        #endregion
        
        #region Events

        public event EventHandler OnSimulateKeyDown;
        public event EventHandler OnSimulateKeyUp;
        public event EventHandler OnSimulateKeyPress;

        protected virtual void RaiseOnSimulateKeyDownEvent(EventArgs e)
        {
            if (OnSimulateKeyDown != null)
            {
                OnSimulateKeyDown(this, e);
            }
        }

        protected virtual void RaiseOnSimulateKeyUpEvent(EventArgs e)
        {
            if (OnSimulateKeyUp != null)
            {
                OnSimulateKeyUp(this, e);
            }
        }

        protected virtual void RaiseOnSimulateKeyPressEvent(EventArgs e)
        {
            if (OnSimulateKeyPress != null)
            {
                OnSimulateKeyPress(this, e);
            }
        }

        #endregion

        #region Key Simulations 

        protected void SimulateKeyDown()
        {
            ThreadPool.QueueUserWorkItem(o => WindowsAPI.InputSimulator.SimulateKeyDown(this.keyInput.VirtualKey, (ushort)this.keyInput.Scancode));

            //(new Thread(() =>
            //{
            //    WindowsAPI.InputSimulator.SimulateKeyDown(this.keyInput.VirtualKey, (ushort)this.keyInput.Scancode);
            //})
            //{
            //    Name = string.Format("SimulateKeyDown - virtualkey: {0}, scancode: {1}",
            //    this.keyInput.VirtualKey, this.keyInput.Scancode),
            //    Priority = ThreadPriority.Normal
            //}).Start();
            
            RaiseOnSimulateKeyDownEvent(EventArgs.Empty);
        }

        protected void SimulateKeyUp()
        {
            //(new Thread(() =>
            //{
            //    WindowsAPI.InputSimulator.SimulateKeyUp(this.keyInput.VirtualKey, (ushort)this.keyInput.Scancode);
            //})
            //{
            //    Name = string.Format("SimulateKeyDown - virtualkey: {0}, scancode: {1}",
            //    this.keyInput.VirtualKey, this.keyInput.Scancode),
            //    Priority = ThreadPriority.Normal
            //}).Start();
            ThreadPool.QueueUserWorkItem(o => WindowsAPI.InputSimulator.SimulateKeyUp(this.keyInput.VirtualKey, (ushort)this.keyInput.Scancode));
            RaiseOnSimulateKeyUpEvent(EventArgs.Empty);
        }

        protected void SimulateKeyPress()
        {
            (new Thread(() =>
            {
                WindowsAPI.InputSimulator.SimulateKeyDown(this.keyInput.VirtualKey, (ushort)this.keyInput.Scancode);
                WindowsAPI.InputSimulator.SimulateKeyUp(this.keyInput.VirtualKey, (ushort)this.keyInput.Scancode);
            })
            {
                Name = string.Format("SimulateKeyDown - virtualkey: {0}, scancode: {1}",
                this.keyInput.VirtualKey, this.keyInput.Scancode),
                Priority = ThreadPriority.Normal
            }).Start();

            RaiseOnSimulateKeyDownEvent(EventArgs.Empty);
            RaiseOnSimulateKeyUpEvent(EventArgs.Empty);
            RaiseOnSimulateKeyPressEvent(EventArgs.Empty);
        }

        protected void StartRepeating()
        {
            if (this.IsSimulating == false)
            {
                this.repeater.Start();
                this.IsSimulating = true;
            }
        }

        protected void StopRepeating()
        {
            if (this.IsSimulating == true)
            {
                this.repeater.Stop();

                this.repeater.IsFirstTick = true;
                this.repeater.Interval = this.repeater.InitialDelay;
                this.IsSimulating = false;
            }
        }

        void repeater_Tick(object sender, EventArgs e)
        {
            RepeatTimer rt = sender as RepeatTimer;
            rt.Interval = rt.RepeatRate;
            this.SimulateKeyDown(); 
        }

        public virtual void Activate(bool doSimulation)
        {
            if (doSimulation)
            {
                this.SimulateKeyDown(); 
            }
            else
            {
                this.IsChecked = true;
            }
        }
        public virtual void Deactivate(bool doSimulation)
        {
            if (doSimulation)
            {
                if (this.isSimulating)
                    this.StopRepeating(); 
                this.SimulateKeyUp(); 
            }
            else
            {
                this.IsChecked = false;
            }
        }

        #endregion

        #region Keyboard Hooks Handlers

        public void OnKeyboardListenerKeyDown(object sender, RawKeyEventArgs args)
        {
            lock (this.IsChecked as object)
            {
                if (Keyboard.IsShiftingOrLockingVirtualKey((VirtualKey)args.VKCode))
                {
                    bool capsState = WindowsAPI.InputSimulator.IsTogglingKeyInEffect(WindowsAPI.VirtualKey.VK_CAPITAL);
                    this.SetShiftStateFlag((VirtualKey)args.VKCode, capsState);
                }

                if (args.VKCode == (int)this.KeyInput.VirtualKey)
                {
                    if (Keyboard.IsLockingModifierVirtualKey(this.KeyInput.VirtualKey))
                    {
                        //bool lockedStatus = WindowsAPI.InputSimulator.IsTogglingKeyInEffect(this.KeyInput.VirtualKey);
                        //if (this.IsChecked != lockedStatus)
                        //{
                        //    if (lockedStatus)
                        //        this.Activate(false);
                        //    else
                        //        this.Deactivate(false);
                        //}
                    }// if (Keyboard.IsShiftingModifierVirtualKey(this.KeyInput.VirtualKey))
                    else
                    {
                        if (this.IsChecked == false)
                            this.Activate(false);
                    }
                }
            }
        }

        public void OnKeyboardListenerKeyUp(object sender, RawKeyEventArgs args)
        {
            lock (this.IsChecked as object)
            {
                if (Keyboard.IsLockingModifierVirtualKey(this.KeyInput.VirtualKey))
                {
                    bool lockedStatus = WindowsAPI.InputSimulator.IsTogglingKeyInEffect(this.KeyInput.VirtualKey);

                    if (this.IsChecked != lockedStatus)
                    {
                        if (lockedStatus)
                            this.Activate(false);
                        else
                            this.Deactivate(false);
                    }
                }// if (Keyboard.IsShiftingModifierVirtualKey(this.KeyInput.VirtualKey))
                if (Keyboard.IsShiftingOrLockingVirtualKey((VirtualKey)args.VKCode))
                {
                    bool capsState = WindowsAPI.InputSimulator.IsTogglingKeyInEffect(WindowsAPI.VirtualKey.VK_CAPITAL);
                    this.RemoveShiftStateFlag((VirtualKey)args.VKCode, capsState);
                }

                if (args.VKCode == (int)this.KeyInput.VirtualKey)
                {

                    if (Keyboard.IsLockingModifierVirtualKey(this.KeyInput.VirtualKey))
                    {
                        //bool lockedStatus = WindowsAPI.InputSimulator.IsTogglingKeyInEffect(this.KeyInput.VirtualKey);

                        //if (this.IsChecked != lockedStatus)
                        //{
                        //    if (lockedStatus)
                        //        this.Activate(false);
                        //    else
                        //        this.Deactivate(false);
                        //}
                    }// if (Keyboard.IsShiftingModifierVirtualKey(this.KeyInput.VirtualKey))
                    else
                    {
                        if (this.IsChecked == true)
                            this.Deactivate(false);
                    }
                }
            }

        }

        #endregion 

        #region ShiftState Flag

        public void SetShiftStateFlag(VirtualKey virtualKey, bool capslock)
        {
            this.shiftState |= VirtualKeyToShiftState(virtualKey);
            ChangeLabel(capslock); 
        }

        public void RemoveShiftStateFlag(VirtualKey virtualKey, bool capslock)
        {
            this.shiftState ^= VirtualKeyToShiftState(virtualKey);
            ChangeLabel(capslock); 
        }

        protected void ChangeLabel(bool capslock)
        {
            string newLabel = this.KeyInput.GetShiftState(this.shiftState, capslock);
            if (((newLabel != "") && (newLabel != null)) && (this.label == null))
                this.Content = newLabel; 
        }

        public string GetCurrentThingie()
        {
            return this.keyInput.GetShiftState(this.shiftState, InputSimulator.IsTogglingKeyInEffect(VirtualKey.VK_CAPITAL)); 
        }

        #endregion 

        #region Static Members 

        static public ShiftState VirtualKeyToShiftState(VirtualKey virtualKey)
        {
            switch (virtualKey)
            {
                case VirtualKey.VK_LSHIFT:
                    return ShiftState.LShft;
                case VirtualKey.VK_RSHIFT: 
                    return ShiftState.RShft; 
                case VirtualKey.VK_LCONTROL:
                    return ShiftState.LCtrl;
                case VirtualKey.VK_RCONTROL:
                    return ShiftState.RCtrl;
                case VirtualKey.VK_LMENU:
                    return ShiftState.LMenu;
                case VirtualKey.VK_RMENU:
                    return ShiftState.RMenu; 
                default:
                    return ShiftState.None; 
            }
        }

        #endregion

        #region XML Read/Write Overrides
        protected override bool SetValue(string name, string value)
        {
            switch (name.ToLower())
            {
                case "scancode":
                    uint scancodeOut = 0;
                    uint.TryParse(value, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out scancodeOut); 
                    this.keyInput = new KeyInput(scancodeOut);
                    if (!this.keyInput.IsDeadKey(Ziyi.ShiftState.None, false))
                        this.Content = this.keyInput.GetShiftState(Ziyi.ShiftState.None, false); 
                    break;
                case "repeating":
                    if (value == "yes" || value == "true")
                        this.Repeating = true;
                    else if (value == "no" || value == "false")
                        this.Repeating = false;
                    break;
                default:
                    return base.SetValue(name, value);
            }
            return true;
        }

        public override XmlDocument ToXml()
        {
            XmlDocument xmlDoc = base.ToXml();

            XmlNode xn = xmlDoc.SelectSingleNode(this.GetType().Name);

            XmlElement input = xmlDoc.CreateElement("scancode");
            string hexOutput = String.Format("{0:X}", this.KeyInput.Scancode);
            input.InnerText = hexOutput;
            xn.AppendChild(input);

            XmlElement virtualkey = xmlDoc.CreateElement("virtualkey");
            virtualkey.InnerText = this.KeyInput.VirtualKey.ToString();
            xn.AppendChild(virtualkey);

            XmlElement repeating = xmlDoc.CreateElement("repeating");
            repeating.InnerText = this.Repeating.ToString().ToLower();
            xn.AppendChild(repeating);

            return xmlDoc;
        }
        #endregion 
    }
}
