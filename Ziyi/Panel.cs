using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Windows;
using System.IO; 

namespace Ziyi
{
    class Panel: Viewbox
    {
        #region Protected Data

        protected Canvas canvas = new Canvas();
        protected string menuName = "Panel";

        #endregion 

        #region Properties

        public string MenuName
        {
            get
            {
                return this.menuName;
            }
            set
            {
                this.menuName = value;
            }
        }
        public Canvas Canvas
        {
            get
            {
                return this.canvas;
            }
            private set
            {
                this.canvas = value;
                this.Child = this.canvas; 
            }
        }

        #endregion 

        #region Constructors

        public Panel()
            : base()
        {
            this.Stretch = System.Windows.Media.Stretch.Fill;
            this.Child = this.canvas;
        }

        public Panel(string XmlFragment)
            : this()
        {
            this.FromXml(XmlFragment);
        }

        public Panel(XmlNode panelNodes)
            : this()
        {
            XmlElement ele = panelNodes as XmlElement; 
            if (ele.HasAttribute("name"))
                this.menuName = panelNodes.Attributes["name"].Value;

            double w = 0, h = 0;

            if (ele.HasAttribute("width"))
                double.TryParse(panelNodes.Attributes["width"].Value, out w);
            this.Canvas.Width = w;

            if (ele.HasAttribute("height"))
                double.TryParse(panelNodes.Attributes["height"].Value, out h);
            this.Canvas.Height = h; 
        }

        #endregion 

        #region XML Read/Write

        protected virtual bool SetValue(string name, string value)
        {
            double d = 0;
            switch (name.ToLower())
            {
                case "name":
                    this.menuName = value;
                    break;

                case "visibility":
                    Visibility vis;
                    if (Enum.TryParse(value, true, out vis))
                        this.Visibility = vis; 
                    else
                        this.Visibility = Visibility.Visible;
                    break;

                case "width":
                    if (double.TryParse(value, out d))
                        this.Canvas.Width = d;
                    break;

                case "height":
                    if (double.TryParse(value, out d))
                        this.Canvas.Height = d;
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
                reader.Read(); 
                if (reader.HasAttributes)
                {
                    while (reader.MoveToNextAttribute())
                    {
                        this.SetValue(reader.LocalName, reader.Value); 
                    }
                }

                while (!reader.EOF)
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        KeyBase kb;
                        switch (reader.LocalName)
                        {
                            case "StandardKey":
                                kb = new StandardKey(reader.ReadOuterXml());
                                break;

                            case "LockedKey":
                                kb = new LockedKey(reader.ReadOuterXml());
                                break;

                            case "ShiftingKey":
                                kb = new ShiftingKey(reader.ReadOuterXml());
                                break;

                            case "MacroKey":
                                kb = new MacroKey(reader.ReadOuterXml());
                                break;

                            case "WordCompleteKey":
                                kb = new WordCompleteKey(reader.ReadOuterXml());
                                break;

                            case "CommandKey":
                                kb = new CommandKey(reader.ReadOuterXml());
                                break;

                            default:
                                reader.Read();
                                continue;
                        }
                        this.Canvas.Children.Add(kb); 
                    }
                    else
                    {
                        reader.Read();
                    }
                }
            }
        }

        public virtual XmlDocument ToXml()
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement root = xmlDoc.CreateElement(this.GetType().Name.ToLower());

            root.SetAttribute("visibility", this.Canvas.Visibility.ToString().ToLower());
            root.SetAttribute("width", this.Canvas.Width.ToString());
            root.SetAttribute("height", this.Canvas.Height.ToString());
            root.SetAttribute("name", this.MenuName);

            xmlDoc.AppendChild(root);
            Console.WriteLine(xmlDoc.OuterXml); 
            foreach (UIElement uie in this.Canvas.Children)
            {
                KeyBase kb = uie as KeyBase;
                if (kb != null)
                {
                    XmlDocument xd = kb.ToXml();
                    XmlDocumentFragment xfrag = xmlDoc.CreateDocumentFragment();

                    xfrag.InnerXml = xd.OuterXml; 
                    xmlDoc.DocumentElement.AppendChild(xfrag); 
                }
            }
 
            return xmlDoc;
        }

        #endregion
    }
}
