using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls; 

namespace Ziyi
{
    class WordCompleteKey : KeyBase
    {

        public int SubstringIndex { get; set; }

        #region Constructors 

        public WordCompleteKey()
            : base()
        {
            this.SubstringIndex = 0;
        }

        public WordCompleteKey(string XmlFragment)
            : this()
        {
            this.FromXml(XmlFragment); 
        }

        public WordCompleteKey(XmlNodeList keyNodes)
            : base(keyNodes)
        {
            this.SubstringIndex = 0; 
        }

        #endregion 

        #region Events

        public event EventHandler OnSimulateText;
        protected virtual void RaiseOnSimulateTextEvent(EventArgs e)
        {
            if (OnSimulateText != null)
            {
                OnSimulateText(this, e);
            }
        }

        #endregion 

        protected override void OnChecked(RoutedEventArgs e)
        {
            //base.OnChecked(e);
            this.IsChecked = false; 
            if (this.Content is string )
            {
                string text = this.Content as string;
                if (text != "" && text != null)
                {                    
                    if (SubstringIndex <= text.Length && (SubstringIndex + (text.Length - SubstringIndex) <= text.Length))
                    {
                        text = text.Substring(SubstringIndex, text.Length - SubstringIndex);
                        if (Properties.Settings.Default.AddSpaceOnTextSimulation)
                            text = String.Concat(text, " ");
                        WindowsAPI.InputSimulator.SimulateUnicodeString(text);
                        RaiseOnSimulateTextEvent(EventArgs.Empty);
                    }
                }
            }
        }
    }
}
