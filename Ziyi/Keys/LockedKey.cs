using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Input;

namespace Ziyi
{
    class LockedKey : SingleIputKey
    {
        #region Constructors

        public LockedKey()
            : base()
        {

        }

        public LockedKey(string XmlFragment)
            : this()
        {
            this.FromXml(XmlFragment);
        }

        public LockedKey(XmlNodeList keyNodes)
            : base(keyNodes)
        {

        }

        #endregion 

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            e.Handled = true;
            this.CaptureMouse();

            if (e.ChangedButton == Properties.Settings.Default.PrimaryInputTrigger)
            {
                //Action beep = this.SimulateKeyPress;
                //beep.BeginInvoke((a) => { beep.EndInvoke(a); }, null);

                this.SimulateKeyPress();

            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            e.Handled = true;
            this.ReleaseMouseCapture();

            if (e.ChangedButton == Properties.Settings.Default.PrimaryInputTrigger)
            {

            }
        }
    }
}
