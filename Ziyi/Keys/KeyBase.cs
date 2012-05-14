using System;
using System.Windows.Controls;
using System.Windows;
using System.Xml;
using System.IO;
using System.Windows.Controls.Primitives; 

namespace Ziyi
{
    public class KeyBase : ToggleButton
    {
        #region Constructors

        public KeyBase(): base()
        {
            this.Focusable = false;
        }

        public KeyBase(string XmlFragment): this()
        {
            this.FromXml(XmlFragment); 
        }

        public KeyBase(XmlNodeList keyNodes) : this()
        {
            for (int k = 0; k < keyNodes.Count; k++)
            {
                double d = 0; 
                switch (keyNodes[k].Name)
                {
                    case "top":
                        double.TryParse(keyNodes[k].InnerText, out d);
                        this.Top = d; 
                        break;
                    case "left":
                        double.TryParse(keyNodes[k].InnerText, out d);
                        this.Left = d; 
                        break;
                    case "width":
                        double.TryParse(keyNodes[k].InnerText, out d);
                        this.Width = d; 
                        break;
                    case "height":
                        double.TryParse(keyNodes[k].InnerText, out d);
                        this.Height = d; 
                        break;
                    case "label":
                        this.label = keyNodes[k].InnerText;
                        if (this.label != null)
                            this.Content = this.label; 
                        break;
                    case "description":
                        this.description = keyNodes[k].InnerText;
                        break;
                }
            }
        }

        #endregion

        #region Protected Data

        protected string label = null;
        protected string description = "";

        #endregion 

        #region XML Read/Write 

        protected virtual bool SetValue(string name, string value)
        {
            double d = 0;
            switch (name.ToLower())
            {
                case "top":
                    if (double.TryParse(value, out d))
                        this.Top = d;
                    break;
                case "left":
                    if (double.TryParse(value, out d))
                        this.Left = d;
                    break;
                case "width":
                    if (double.TryParse(value, out d))
                        this.Width = d;
                    break;
                case "height":
                    if (double.TryParse(value, out d))
                        this.Height = d;
                    break;
                case "label":
                    if (value == string.Empty)
                    {
                        this.Label = null;
                    }
                    else
                    {
                        this.Label = value;
                        this.Content = this.Label;
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        public virtual void FromXml(string XmlFragment)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ConformanceLevel = ConformanceLevel.Fragment;

            using (XmlReader reader = XmlReader.Create(new StringReader(XmlFragment), settings))
            {
                string nodeName = reader.LocalName;

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        nodeName = reader.LocalName;                     
                    }
                    else if (reader.NodeType == XmlNodeType.Text)
                    {
                        this.SetValue(nodeName, reader.Value);
                    }
                }
            }
        }

        public virtual XmlDocument ToXml()
        {
            XmlDocument xmlDoc = new XmlDocument(); 

            XmlElement root = xmlDoc.CreateElement(this.GetType().Name);
 
            XmlElement top = xmlDoc.CreateElement("top");
            top.InnerText = this.Top.ToString();
            root.AppendChild(top); 

            XmlElement left = xmlDoc.CreateElement("left");
            left.InnerText = this.Left.ToString();
            root.AppendChild(left); 

            XmlElement width = xmlDoc.CreateElement("width");
            width.InnerText = this.Width.ToString();
            root.AppendChild(width); 

            XmlElement height = xmlDoc.CreateElement("height");
            height.InnerText = this.Height.ToString();
            root.AppendChild(height);

            XmlElement label = xmlDoc.CreateElement("label");
            if (this.label == null || this.label == string.Empty)
                label.InnerText = string.Empty;
            else
                label.InnerText = this.label;
            root.AppendChild(label);

            XmlElement description = xmlDoc.CreateElement("description");
            description.InnerText = this.description;
            root.AppendChild(description);

            xmlDoc.AppendChild(root);

            return xmlDoc; 
        }

        #endregion

        #region Overrides
        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
        }
        #endregion

        #region Properties
        public string Label
        {
            get
            {
                return this.label;
            }
            set
            {
                this.label = value;
            }
        }

        public double Top
        {
            get
            {
                return Canvas.GetTop(this);
            }
            set
            {
                Canvas.SetTop(this, value);
            }
        }

        public double Left
        {
            get
            {
                return Canvas.GetLeft(this);
            }
            set
            {
                Canvas.SetLeft(this, value);
            }
        }
        #endregion

        public virtual void ApplyStyle()
        {
            this.Style = new Style(GetType(), Application.Current.FindResource(typeof(ToggleButton)) as Style);
        }
    }
}
