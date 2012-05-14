using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows;
using System.Windows.Controls;

namespace Ziyi
{
    class MacroKey: KeyBase
    {
        protected string simulationTexInput;

        #region Constructors

        public MacroKey()
            : base()
        {

        }

        public MacroKey(string XmlFragment)
            : this()
        {
            this.FromXml(XmlFragment); 
        }

        public MacroKey(XmlNodeList keyNodes): base(keyNodes)
        {
            ContextMenu cm = new ContextMenu(); 
            MenuItem mi = new MenuItem(); 
            mi.Header = "Testing";
            cm.Items.Add(mi); 

            this.ContextMenu = cm;

            for (int k = 0; k < keyNodes.Count; k++)
            {
                switch (keyNodes[k].Name)
                {
                    case "input":
                        this.simulationTexInput = keyNodes[k].InnerText;
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
                case "input":
                    this.simulationTexInput = value; 
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

            XmlElement input = xmlDoc.CreateElement("input");
            input.InnerText = this.simulationTexInput;
            xn.AppendChild(input);

            return xmlDoc; 
        }
        #endregion 

        protected override void OnChecked(RoutedEventArgs e)
        {
            base.OnChecked(e);
            this.IsChecked = false;


            if (this.simulationTexInput != "" && this.simulationTexInput != null)
            {
                WindowsAPI.InputSimulator.SimulateUnicodeString(this.simulationTexInput);
            }

        }
    }
}
