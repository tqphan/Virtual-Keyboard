using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Input;

namespace Ziyi
{
    class StandardKey : SingleIputKey
    {
        #region Constructors 

        public StandardKey()
            : base()
        {

        }

        public StandardKey(string XmlFragment)
            : this()
        {
            this.FromXml(XmlFragment);
        }

        public StandardKey(XmlNodeList keyNodes)
            : base(keyNodes)
        {

        }

        #endregion 

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            //base.OnPreviewMouseDown(e);
            e.Handled = true;
            this.CaptureMouse();

            if (e.ChangedButton == Properties.Settings.Default.PrimaryInputTrigger)
            {
                this.SimulateKeyDown();
                if (this.Repeating)
                    this.StartRepeating();
            }
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            //base.OnPreviewMouseDown(e);
            e.Handled = true;
            this.ReleaseMouseCapture();

            if (e.ChangedButton == Properties.Settings.Default.PrimaryInputTrigger)
            {
                if (this.Repeating)
                    this.StopRepeating();
                this.SimulateKeyUp();
            }
        }
    }
}
