using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Windows.Markup; 
using System.Runtime.InteropServices;
using System.Windows.Interop;
using WindowsAPI; 

namespace Ziyi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool inMovesize = false;
        private bool inMovesizeLoop = false;
        private Keyboard keyboard = new Keyboard();
        private Settings settingsWindow ; 
        private System.Windows.Threading.DispatcherTimer topmostTimer = new System.Windows.Threading.DispatcherTimer();
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private ActiveWindowMonitor activeWindowMonitor = new ActiveWindowMonitor(); 

        public MainWindow()
        {
            InitializeComponent();
            
            KeyboardViewbox.Child = this.keyboard;
            this.PopulateContextMenu();

        }

        void OnActiveWindowChanged(object sender, string windowHeader, IntPtr hwnd, IntPtr hkl)
        {
            if (Properties.Settings.Default.ForceTopmostOnActiveWindowChange)
            {
                ForceTopMost();
            }
        }

        private void PopulateContextMenu()
        {
            ContextMenu mainMenu = new ContextMenu();

            MenuItem open = new MenuItem();
            open.Header = "Open Keyboard";
            open.Click += new RoutedEventHandler(MenuHandler);
            mainMenu.Items.Add(open);

            MenuItem save = new MenuItem();
            save.Header = "Save Keyboard";
            save.Click += new RoutedEventHandler(MenuHandler);
            mainMenu.Items.Add(save);

            MenuItem settings = new MenuItem();
            settings.Header = "Settings";
            settings.Click += new RoutedEventHandler(MenuHandler);
            mainMenu.Items.Add(settings);

            MenuItem panels = new MenuItem();
            panels.Header = "Panels";
            panels = this.keyboard.PopulatePanelsMenuItems(panels);
            if (panels.HasItems)
                mainMenu.Items.Add(panels);

            MenuItem exit = new MenuItem();
            exit.Header = "Exit";
            exit.Click += new RoutedEventHandler(MenuHandler);
            mainMenu.Items.Add(exit);

            this.ContextMenu = mainMenu;
        }

        private void MenuHandler(object sender, RoutedEventArgs e)
        {

            MenuItem mi = e.Source as MenuItem;

            switch (mi.Header.ToString())
            {
                case "Exit":
                    {
                        Application.Current.Shutdown();
                        //settingsWindow.Close(); 
                        //this.Close();
                        break;
                    }
                case "Settings":
                    {
                        if (settingsWindow.Visibility == System.Windows.Visibility.Hidden)
                        {
                            settingsWindow.Visibility = System.Windows.Visibility.Visible;
                            settingsWindow.Show();
                        }
                        else
                            settingsWindow.Visibility = System.Windows.Visibility.Hidden;
                        break;
                    }
                case "Open Keyboard":
                    {
                        this.keyboard.LoadAnotherKeyboard();
                        this.PopulateContextMenu();
                        break;
                    }
                case "Save Keyboard":
                    {
                        this.keyboard.SaveKeyboardXml("test.xml");
                        break;
                    }
            }

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            settingsWindow = new Settings();

            LoadLastUsedTheme(); 

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += new EventHandler(notifyIcon_Click);
            notifyIcon.DoubleClick += new EventHandler(notifyIcon_DoubleClick);
            notifyIcon.Icon = new System.Drawing.Icon(Properties.Resources.Keyboard, 40, 40);
            notifyIcon.Visible = true;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //settings.Close(); 
            notifyIcon.Dispose();
            //this.notifier.Visible = false;
            //if (Properties.Settings.Default.SaveDictionaryOnExit)
            //    this.keyboard.SaveDictionaryToXML(Properties.Settings.Default.DefaultDictionaryFile); 
            Properties.Settings.Default.Save();
            base.OnClosing(e);
        }

        void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void notifyIcon_Click(object sender, EventArgs e)
        {
            ContextMenu.IsOpen = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (Properties.Settings.Default.EnableAeroGlass)
            {
                GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
            }

            //Set up topmost timer
            this.topmostTimer.Interval = TimeSpan.FromMilliseconds(200);
            this.topmostTimer.Tick += new EventHandler(topmostTimer_Tick);
             
            //this.topmostTimer.Start(); 

            //Set the window style to WS_EX_NOACTIVATE to prevent the window from getting focus on mouse interactions 
            WindowInteropHelper helper = new WindowInteropHelper(this);
            int newWinLongExStyle = NativeMethods.GetWindowLong(helper.Handle, (int)WindowsMessages.GWL_EXSTYLE);
            newWinLongExStyle |= (int)WindowsMessages.WS_EX_NOACTIVATE;
            NativeMethods.SetWindowLong(helper.Handle, (int)WindowsMessages.GWL_EXSTYLE, newWinLongExStyle);
            this.Topmost = true;

            //Setting the WS_DLGFRAME, WS_POPUP flags and removing WS_THICKFRAME will disable Aero Snap feature for the window
            //and still allowing Aero Glass
            int newWinLongStyle = 0;
            newWinLongStyle ^= (int)WindowsMessages.WS_THICKFRAME;
            newWinLongStyle |= (int)WindowsMessages.WS_POPUP | (int)WindowsMessages.WS_DLGFRAME;
            NativeMethods.SetWindowLong(helper.Handle, (int)WindowsMessages.GWL_STYLE, newWinLongStyle);

            //Set WndProc hook
            HwndSource src = HwndSource.FromHwnd(helper.Handle);
            src.AddHook(new HwndSourceHook(WndProc));

            activeWindowMonitor.OnActiveWindowChanged += new ActiveWindowMonitor.ActiveWindowChangedHandler(OnActiveWindowChanged);
            activeWindowMonitor.OnWindowRestored += new ActiveWindowMonitor.ActiveWindowChangedHandler(OnActiveWindowChanged);
            activeWindowMonitor.OnWindowMinimized += new ActiveWindowMonitor.ActiveWindowChangedHandler(OnActiveWindowChanged);
            activeWindowMonitor.Start();
        }

        void topmostTimer_Tick(object sender, EventArgs e)
        {
            this.ForceTopMost();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            this.Cursor = Cursors.SizeAll;
            DragMove();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            this.Cursor = Cursors.Arrow;
        }

        //protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        //{
        //    base.OnMouseLeftButtonDown(e);
        //    if (e.Source != this)
        //        e.Handled = true; 
        //}

        //protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
        //{
        //    base.OnMouseLeftButtonUp(e);
        //    if (e.Source != this)
        //        e.Handled = true; 
        //}

        protected override void OnActivated(EventArgs e)
        {
            //base.OnActivated(e);
            //Set the window style to noactivate.
            WindowInteropHelper helper = new WindowInteropHelper(this);
            NativeMethods.SetWindowLong(helper.Handle, (int)WindowsMessages.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(helper.Handle, (int)WindowsMessages.GWL_EXSTYLE) |
                (int)WindowsMessages.WS_EX_NOACTIVATE);
        }

        protected void ForceTopMost()
        {
            IntPtr windowHandle = (new WindowInteropHelper(this)).Handle;
            uint flags = (uint)(WindowsMessages.SWP_NOMOVE | WindowsMessages.SWP_NOSIZE |
                WindowsMessages.SWP_SHOWWINDOW | WindowsMessages.SWP_NOACTIVATE);
            NativeMethods.SetWindowPos(windowHandle, IntPtr.Zero, 0, 0, 0, 0, flags);
        }

        public void LoadAnotherTheme()
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".xaml"; // Default file extension
            dlg.Filter = "Text documents (.xaml)|*.xaml"; // Filter files by extension

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                LoadThemeFromFile(dlg.FileName);
            }
        }

        public void LoadDefaultTheme()
        {
            LoadThemeFromFile(Properties.Settings.Default.ThemeLastUsedFile); 
        }

        public void LoadLastUsedTheme()
        {
            LoadThemeFromFile(Properties.Settings.Default.ThemeLastUsedFile);
        }

        private void LoadThemeFromFile(string fileName)
        {
            try
            {
                ResourceDictionary skin = new ResourceDictionary();
                //skin.Source = new Uri("", UriKind.RelativeOrAbsolute);
                using (XmlReader reader = System.Xml.XmlReader.Create(fileName))
                {
                    skin = (ResourceDictionary)XamlReader.Load(reader);
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(skin);
                }
                Properties.Settings.Default.ThemeLastUsedFile = fileName; 
            }
            catch
            {
                Application.Current.Resources.MergedDictionaries.Clear();
            }
            finally
            {
                this.keyboard.ApplyStyle(); 
            }

        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch ((WindowsMessages)msg)
            {
                // Reenable Aero glass: 
                case WindowsMessages.WM_DWMCOMPOSITIONCHANGED:
                    if (Properties.Settings.Default.EnableAeroGlass)
                    {
                        GlassHelper.ExtendGlassFrame(this, new Thickness(-1));
                        handled = true;
                    }
                    break;

                //By handling this message we can make an active window's frame appear to be inactive and vice versa. 
                //Never activate the main window but still allow children windows to behave normally 
                //See http://mojoware.org/win32/wm_ncactivate.html
                case WindowsMessages.WM_NCACTIVATE:
                    if (wParam.ToInt32() == 1)
                    {
                        handled = true;
                        return (IntPtr.Zero);
                    }
                    break;

                // Allow the window to stay in the negative Y coordinate without being moved down by default 
                case WindowsMessages.WM_ENTERSIZEMOVE:
                    inMovesize = true;
                    inMovesizeLoop = false;
                    break;

                case WindowsMessages.WM_EXITSIZEMOVE:
                    inMovesize = false;
                    inMovesizeLoop = false;
                    break;

                case WindowsMessages.WM_CAPTURECHANGED:
                    inMovesizeLoop = inMovesize;
                    break;

                case WindowsMessages.WM_WINDOWPOSCHANGING:
                    if (inMovesizeLoop)
                    {
                        WINDOWPOS wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
                        wp.flags |= (int)WindowsMessages.SWP_NOMOVE;
                        Marshal.StructureToPtr(wp, lParam, true);
                    }
                    break;
                //case 0x0007: // WM_SETFOCUS
                //    {
                //        SetActiveWindow(wParam);
                //    }
                //    break; 
                //case WindowsMessages.WM_ACTIVATEAPP:
                //    {
                //        if ((int)wParam == 1)
                //        {
                //            Functions.SetActiveWindow(lParam);
                //        }
                //    }
                //    break;

                //case WindowsMessages.WM_ACTIVATE:
                //    {
                //        if (((int)wParam & 0x00FF) != 0)
                //        {
                //            handled = true;
                //            //SetActiveWindow(lParam);
                //            //new Thread(() => Console.Beep()).Start();
                //            Console.WriteLine("{0}, {1}, {2}", (int)wParam & 0x00FF, (int)wParam >> 16, (int)lParam);
                //            int wp = (int)wParam & 0x00FF;
                //            if (lParam != IntPtr.Zero && (wp == 1 || wp == 2))
                //            {
                //                SetActiveWindow(lParam);

                //            }
                //            //else
                //            //{
                //            //    // Could not find sender, just in-activate it.
                //            //    SetActiveWindow(IntPtr.Zero);
                //            //}
                //        }

                //    }
                //    break;

                // Fix for a Windows bug where the contents of the window is not shown when the window has WS_EX_NOACTIVATE flag
                case WindowsMessages.WM_MOVING:
                case WindowsMessages.WM_SIZING:
                    {
                        RECT rect;
                        rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                        NativeMethods.SetWindowPos(hwnd, IntPtr.Zero, rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top, 0);
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}
