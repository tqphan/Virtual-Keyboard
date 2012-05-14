using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Controls;
using System.Windows;
using System.IO;
using System.Windows.Data;

namespace Ziyi
{
    class Keyboard : StackPanel
    {
        #region Events 

        public event EventHandler OnKeyboardLayoutChange;
        protected virtual void RaiseOnKeyboardLayoutChangeEvent(EventArgs e)
        {
            if (OnKeyboardLayoutChange != null)
            {
                OnKeyboardLayoutChange(this, e);
            }
        }

        #endregion 

        #region Protected Data

        private List<WordCompleteKey> wordCompleteKeys = new List<WordCompleteKey>();
        private KeyboardListener listener = new KeyboardListener(); 
        private WordPrediction.WordPredictor dictionary = new WordPrediction.WordPredictor();
        private IntPtr keyboardLayoutHandle = IntPtr.Zero;

        #endregion 

        public IntPtr KeyboardLayoutHandle
        {
            get
            {
                return this.keyboardLayoutHandle;
            }
            set
            {
                if (value != IntPtr.Zero && value != this.keyboardLayoutHandle)
                {
                    this.keyboardLayoutHandle = value;
                    RaiseOnKeyboardLayoutChangeEvent(EventArgs.Empty);
                }
            }
        }

        public Keyboard()
        {
            this.Margin = new Thickness(0);
            this.Orientation = Orientation.Horizontal; 
            this.keyboardLayoutHandle = WindowsAPI.NativeMethods.GetKeyboardLayout(0);

            //Load default keyboard on first run; otherwise, try to load from last used keyboard 
            if (Properties.Settings.Default.KeyboardLastUsedFile == string.Empty ||
                Properties.Settings.Default.KeyboardLastUsedFile == null)
            {
                LoadDefaultKeyboard();
                Properties.Settings.Default.Save();
            }
            else
            {
                LoadKeyboardFile(Properties.Settings.Default.KeyboardLastUsedFile);
            }
        }

        ~Keyboard()
        {

        }

        public void SaveKeyboardXml(string fileName)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.AppendChild(xmlDoc.CreateElement("keyboard"));
                foreach (UIElement uie in this.Children)
                {
                    Panel p = uie as Panel;
                    if (p != null)
                    {
                        XmlDocument xd = p.ToXml();
                        XmlDocumentFragment xfrag = xmlDoc.CreateDocumentFragment();
                        xfrag.InnerXml = xd.OuterXml;
                        xmlDoc.DocumentElement.AppendChild(xfrag);
                    }
                }
                XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                xmlDoc.Save(writer);
            }
            catch
            {
                //TO-DO 
            }
        }

        protected void ProcessKeyboardFromXml(XmlDocument xmlDoc)
        {
            this.UnregisterEvents();
            this.RemoveBindings(); 
            this.Children.RemoveRange(0, this.Children.Count); 
            this.wordCompleteKeys.Clear();

            XmlNodeList panelNodes = xmlDoc.SelectNodes("/keyboard/panel");

            foreach (XmlNode xn in panelNodes)
            {
                Panel panel = new Panel(xn.OuterXml);
                this.Children.Add(panel);
            }

            this.RegisterEvents();
            this.ApplyStyle(); 
        }
        
        protected void ProcessKeyboardFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(fileName);
                    ProcessKeyboardFromXml(xmlDoc);
                    Properties.Settings.Default.KeyboardLastUsedFile = fileName;
                    Properties.Settings.Default.Save();
                }
                catch
                {
                    throw; 
                }
                
            }
            else
            {
                MessageBoxResult mr = MessageBox.Show("Unable to load the keyboard  file: " + 
                    fileName + ". Would you like to load the default keyboard?",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (mr == MessageBoxResult.Yes)
                {
                    LoadDefaultKeyboard();
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
        }

        protected void LoadDefaultKeyboard()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(Properties.Resources.default_kbd);
                ProcessKeyboardFromXml(xmlDoc);
                Properties.Settings.Default.KeyboardLastUsedFile = string.Empty;
                Properties.Settings.Default.Save();
            }
            catch
            {
                throw;
            }
        }

        public void LoadAnotherKeyboard()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "kbd"; // Default file name
            dlg.DefaultExt = ".xml"; // Default file extension
            dlg.Filter = "XML documents (.xml)|*.xml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                ProcessKeyboardFile(dlg.FileName); 
            }
        }

        public bool LoadKeyboardFile(string fileName)
        {
            ProcessKeyboardFile(fileName);

            this.dictionary = new WordPrediction.WordPredictor(this.wordCompleteKeys); 
            this.dictionary.LoadDictionary(Properties.Settings.Default.DefaultDictionaryFile);
            return true;
        }

        public bool SaveDictionaryToXML(string fileName)
        {
            return this.dictionary.SaveDictionary(fileName); 
        }



        protected void RegisterEvents()
        {
            this.RegisterEvents(true); 
        }

        protected void UnregisterEvents()
        {
            this.RegisterEvents(false); 
        }

        protected void RegisterEvents(bool register)
        {
            foreach (UIElement uie in this.Children)
            {
                Panel p = uie as Panel;
                if (p != null)
                {
                    foreach (UIElement el in p.Canvas.Children)
                    {
                        SingleIputKey sik = el as SingleIputKey;
                        if (sik != null)
                        {
                            if (register)
                            {
                                sik.OnSimulateKeyDown += new EventHandler(OnSingleIputKeySimulateKeyDown);
                                sik.OnSimulateKeyUp += new EventHandler(OnSingleIputKeySimulateKeyUp);

                                listener.KeyDown += new RawKeyEventHandler(sik.OnKeyboardListenerKeyDown);
                                listener.KeyUp += new RawKeyEventHandler(sik.OnKeyboardListenerKeyUp);
                            }
                            else
                            {
                                sik.OnSimulateKeyDown -= new EventHandler(OnSingleIputKeySimulateKeyDown);
                                sik.OnSimulateKeyUp -= new EventHandler(OnSingleIputKeySimulateKeyUp);

                                listener.KeyDown -= new RawKeyEventHandler(sik.OnKeyboardListenerKeyDown);
                                listener.KeyUp -= new RawKeyEventHandler(sik.OnKeyboardListenerKeyUp);
                            }
                        }
                    }
                }

            }
        }

        protected void OnWordCompleteSimulateText(object sender, EventArgs e)
        {
            WordCompleteKey wck = sender as WordCompleteKey;
            if (wck.Content is string)
                this.dictionary.WordSimulated(wck.Content as string);
            else
                this.dictionary.ResetAndPopulate(); 
        }

        protected void OnSingleIputKeySimulateKeyUp(object sender, EventArgs e)
        {
            //Key key = sender as Key;
            //Console.WriteLine(key.KeyInput.Characters); 

        }

        protected void OnSingleIputKeySimulateKeyDown(object sender, EventArgs e)
        {
            //if (sender is SingleIputKey)
            //{
            //    SingleIputKey key = sender as SingleIputKey;

            //    List<WordPrediction.Word> w = this.dictionary.Predict(key.GetCurrentThingie());
            //}

        }

        static public bool IsShiftingOrLockingVirtualKey(WindowsAPI.VirtualKey vk)
        {
            if (IsShiftingModifierVirtualKey(vk) || IsLockingModifierVirtualKey(vk))
                return true;
            else
                return false; 
        }

        static public bool IsShiftingModifierVirtualKey (WindowsAPI.VirtualKey vk)
        {
            switch (vk)
            {
                case WindowsAPI.VirtualKey.VK_LSHIFT: 
                case WindowsAPI.VirtualKey.VK_RSHIFT: 
                case WindowsAPI.VirtualKey.VK_LCONTROL: 
                case WindowsAPI.VirtualKey.VK_RCONTROL: 
                case WindowsAPI.VirtualKey.VK_LMENU: 
                case WindowsAPI.VirtualKey.VK_RMENU: 
                    return true; 
            }
            return false; 
        }

        static public bool IsLockingModifierVirtualKey(WindowsAPI.VirtualKey vk)
        {
            switch (vk)
            {
                case WindowsAPI.VirtualKey.VK_CAPITAL:
                case WindowsAPI.VirtualKey.VK_NUMLOCK:
                case WindowsAPI.VirtualKey.VK_SCROLL:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Release all keys currently pressed in the virtual keyboard 
        /// </summary>
        public void ReleasePressedKeys()
        {
            foreach (UIElement uie in this.Children)
            {
                Panel p = uie as Panel;
                if (p != null)
                {
                    foreach (UIElement el in p.Canvas.Children)
                    {
                        SingleIputKey sik = el as SingleIputKey;
                        if (sik != null)
                        {
                            
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Release any key that is currently pressed regardless if it is part of the virtual keyboard 
        /// </summary>
        public void ReleaseAllPressedKeys()
        {
            foreach (WindowsAPI.VirtualKey val in Enum.GetValues(typeof(WindowsAPI.VirtualKey)))
            {
                if (WindowsAPI.InputSimulator.IsKeyDown(val))
                {
                    WindowsAPI.InputSimulator.SimulateKeyUp(val); 
                }
            }
        }

        //static public bool IsPrintableCharacter(char character)
        //{
        //    char.
        //}

        public MenuItem PopulatePanelsMenuItems(MenuItem menu)
        {
            foreach (UIElement uie in this.Children)
            {
                if (uie is Panel)
                {
                    Panel panel = uie as Panel; 
                    MenuItem mi = new MenuItem();
                    mi.Header = panel.MenuName;
                    mi.IsCheckable = true;
                    mi.IsChecked = true;
                    mi.StaysOpenOnClick = true; 
                    menu.Items.Add(mi);

                    Binding binding = new Binding();
                    binding.Source = mi;
                    binding.Path = new PropertyPath("IsChecked");
                    binding.Converter = new BoolToVisibility();
                    binding.Mode = BindingMode.TwoWay;
                    panel.Canvas.SetBinding(Canvas.VisibilityProperty, binding);
                }
            }
            return menu; 
        }

        protected void RemoveBindings()
        {
            foreach (UIElement uie in this.Children)
            {
                Panel p = uie as Panel;
                if (p != null)
                {
                    BindingOperations.ClearBinding(p.Canvas, Canvas.VisibilityProperty);
                }
            }
        }

        public void ApplyStyle()
        {
            foreach (UIElement uie in this.Children)
            {
                Panel p = uie as Panel;
                if (p != null)
                {
                    foreach (UIElement el in p.Canvas.Children)
                    {
                        KeyBase kb = el as KeyBase;
                        if (kb != null)
                        {
                            kb.ApplyStyle(); 
                        }
                    }
                }
            }
        }
    }
}
