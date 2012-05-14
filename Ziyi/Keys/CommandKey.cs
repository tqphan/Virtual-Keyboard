using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Windows;

namespace Ziyi
{
    public class CommandKey: KeyBase
    {
        protected string command = "";

        #region Constructors 
        public CommandKey()
            : base()
        {

        }

        public CommandKey(string XmlFragment)
            : this()
        {
            this.FromXml(XmlFragment); 
        }

        public CommandKey(XmlNodeList keyNodes)
            : base(keyNodes)
        {
             for (int k = 0; k < keyNodes.Count; k++)
            {
                switch (keyNodes[k].Name)
                {
                    case "command":
                        this.command = keyNodes[k].InnerText; 
                        break;
                }
            }
        }

        #endregion 

        #region XML Overrides Read/Write

        protected override bool SetValue(string name, string value)
        {
            switch (name.ToLower())
            {
                case "command":
                    this.command = value;
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

            XmlElement input = xmlDoc.CreateElement("command");
            input.InnerText = this.command;
            xn.AppendChild(input);

            return xmlDoc;
        }
        #endregion 


        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            this.IsChecked = false;
            switch (this.command)
            {
                case "exit":
                    Application.Current.Shutdown(); 
                    break;
                case "minimized":
                    Application.Current.MainWindow.WindowState = WindowState.Minimized; 
                    break;
                case "menu":
                    //if (Application.Current.MainWindow.ContextMenu == null)
                    //    Console.Beep(); minimized
                    Application.Current.MainWindow.ContextMenu.IsOpen = true; 
                    break;
            }
        }
    }
}
