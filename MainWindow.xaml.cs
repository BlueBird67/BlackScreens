using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Xml.Serialization;
using WindowsDisplayAPI.DisplayConfig;

namespace BlackScreensWPF
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private event EventHandler Resized;

        public MainWindow()
        {
            InitializeComponent();
            this.tbTitle.Text = "BlackScreens 1.05";
            loadUserConfigFile();
            this.DataContext = CommonData.dataInstance;
            CommonData.dataInstance.FParams = this;
            notifyIcon.TrayLeftMouseDown += NotifyIcon_TrayLeftMouseDown;
            refreshMainWindowCurrentScreen();
            updateScreenNames();
        }

        /// <summary>
        /// Refresh name of the current param window screen
        /// </summary>
        public void refreshMainWindowCurrentScreen()
        {
            System.Windows.Interop.WindowInteropHelper wih = new System.Windows.Interop.WindowInteropHelper(this);
            Screen currentScreen = Screen.FromHandle(wih.Handle);
            CommonData.dataInstance.ParamsScreenDeviceName = currentScreen.DeviceName;
        }

        private void NotifyIcon_TrayLeftMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                showWindow();
            }
            else
            {
                minimizeWindow();
            }
        }

        private void minimizeWindow()
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        /// <summary>
        /// Update all screens names on application launch, and handle visibility of information on params window
        /// </summary>
        private void updateScreenNames()
        {
            if (Screen.AllScreens.Length > 0)
            {
                PathDisplayTarget display = CommonData.dataInstance.findDisplayByScreenName(Screen.AllScreens[0].DeviceName);
                if (display != null)
                {
                    CommonData.dataInstance.Screen1Name = display.FriendlyName;
                    tbScreen1Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                }
            }
            if (Screen.AllScreens.Length > 1)
            {
                PathDisplayTarget display = CommonData.dataInstance.findDisplayByScreenName(Screen.AllScreens[1].DeviceName);
                if (display != null)
                {
                    CommonData.dataInstance.Screen2Name = display.FriendlyName;
                    tbScreen1Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                }
            }
            if (Screen.AllScreens.Length > 2)
            {
                PathDisplayTarget display = CommonData.dataInstance.findDisplayByScreenName(Screen.AllScreens[2].DeviceName);
                if (display != null)
                {
                    CommonData.dataInstance.Screen3Name = display.FriendlyName;
                    tbScreen1Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                }
            }
            if (Screen.AllScreens.Length > 3)
            {
                tbScreen4Name.Visibility = Visibility.Visible;
                PathDisplayTarget display = CommonData.dataInstance.findDisplayByScreenName(Screen.AllScreens[3].DeviceName);
                if (display != null)
                {
                    CommonData.dataInstance.Screen4Name = display.FriendlyName;
                    tbScreen1Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                }
            }
            if (Screen.AllScreens.Length > 4)
            {
                tbScreen4Name.Visibility = Visibility.Visible;
                PathDisplayTarget display = CommonData.dataInstance.findDisplayByScreenName(Screen.AllScreens[4].DeviceName);
                if (display != null)
                {
                    CommonData.dataInstance.Screen5Name = display.FriendlyName;
                    tbScreen1Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                }
            }
            if (Screen.AllScreens.Length > 5)
            {
                tbScreen4Name.Visibility = Visibility.Visible;
                PathDisplayTarget display = CommonData.dataInstance.findDisplayByScreenName(Screen.AllScreens[5].DeviceName);
                if (display != null)
                {
                    CommonData.dataInstance.Screen6Name = display.FriendlyName;
                    tbScreen1Name.GetBindingExpression(TextBlock.TextProperty).UpdateTarget();
                }
            }
            tbScreen2AltKeyInfo.Visibility = (Screen.AllScreens.Length > 1) ? Visibility.Visible : Visibility.Hidden;
            tbScreen3AltKeyInfo.Visibility = (Screen.AllScreens.Length > 2) ? Visibility.Visible : Visibility.Hidden;
            tbScreen4AltKeyInfo.Visibility = (Screen.AllScreens.Length > 3) ? Visibility.Visible : Visibility.Hidden;
            // Collapsed used to hide complete line if there is not at least 4 screens used (presume large majority of users cases!)
            tbScreen4Name.Visibility = (Screen.AllScreens.Length > 3) ? Visibility.Visible : Visibility.Collapsed;
            tbScreen5AltKeyInfo.Visibility = (Screen.AllScreens.Length > 4) ? Visibility.Visible : Visibility.Collapsed;
            tbScreen5Name.Visibility = (Screen.AllScreens.Length > 4) ? Visibility.Visible : Visibility.Collapsed;
            tbScreen6AltKeyInfo.Visibility = (Screen.AllScreens.Length > 5) ? Visibility.Visible : Visibility.Collapsed;
            tbScreen6Name.Visibility = (Screen.AllScreens.Length > 5) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void showWindow()
        {
            //updateScreenNames();
            this.Topmost = true;
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
        {
            minimizeWindow();
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void sOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            CommonData.dataInstance.updateAllBlackWindowParams();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.ShowInTaskbar = true;
                refreshMainWindowCurrentScreen();
            }
            else
                this.ShowInTaskbar = false;
        }

        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void bMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
        }

        void MainWindow_Resized(object sender, EventArgs e)
        {
            // In case param Window is moved by user along different screens
            refreshMainWindowCurrentScreen();
        }

        /// <summary>
        /// Windows system rezized used to handle EXIT MOVE by user, which is not handle by any WPF window native event
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //const int WM_ENTERSIZEMOVE = 0x0231;
            const int WM_EXITSIZEMOVE = 0x0232;

            if (msg == WM_EXITSIZEMOVE)
            {
                if (Resized != null)
                {
                    Resized(this, EventArgs.Empty);
                }
            }

            return IntPtr.Zero;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Hook for windows system resized by user only on EXIT MOVE
            // In order to have less event possible, compared to WPF native move event which launch nearly each mouse pixel move
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source.AddHook(new HwndSourceHook(WndProc));
            this.Resized += MainWindow_Resized;
        }

        private void notifyIcon_TrayRightMouseDown(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                miShowHideParameters.Header = "Show Parameters";
            }
            else
                miShowHideParameters.Header = "Hide Parameters";
        }

        public void toggleShowHideWindow()
        {
            if (this.WindowState == WindowState.Minimized)
            {
                showWindow();
            }
            else
                minimizeWindow();
        }

        private void miShowHideParameters_Click(object sender, RoutedEventArgs e)
        {
            toggleShowHideWindow();
        }

        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == Key.Down) || (e.Key == Key.Left))
            {
                sOpacity.Value--;
            }

            if ((e.Key == Key.Up) || (e.Key == Key.Right))
            {
                sOpacity.Value++;
            }
            e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            saveUserConfigFiles();
        }

        /// <summary>
        /// Load user config file by XmlSerializer
        /// </summary>
        private void loadUserConfigFile()
        {
            UserPreferences up;

            String exeLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));

            try { 
                FileStream myFileStream = new FileStream(exeLocation+"/BlacScreensPrefs.xml", FileMode.Open);
                up = (UserPreferences)mySerializer.Deserialize(myFileStream);
                CommonData.dataInstance.Opacity = up.Opacity;
                CommonData.dataInstance.HideTexts = !up.showTextsOnBlackScreens;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Save user config file by XmlSerializer
        /// </summary>
        private void saveUserConfigFiles()
        {
            UserPreferences up = new UserPreferences();
            up.Opacity = CommonData.dataInstance.Opacity;
            up.showTextsOnBlackScreens = !CommonData.dataInstance.HideTexts;

            String exeLocation = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            XmlSerializer mySerializer = new XmlSerializer(typeof(UserPreferences));
            try { 
                StreamWriter myWriter = new StreamWriter(exeLocation + "/BlacScreensPrefs.xml");
                mySerializer.Serialize(myWriter, up);
                myWriter.Close();
            }
            catch (Exception) { }
        }

        private String debugOneScreenToString(System.Drawing.Rectangle r, String screenName)
        {
            String txt = "";
            txt += "Screen 1 : " + screenName + Environment.NewLine;
            txt += "Left " + r.Left + " / X = " + r.X + Environment.NewLine;
            txt += "Top " + r.Top + " / Y = " + r.Y + Environment.NewLine;
            txt += "Right " + r.Right + " / Width = " + r.Width + Environment.NewLine;
            txt += "Bottom " + r.Bottom + " / Height = " + r.Height + Environment.NewLine;
            txt += "Size.Height " + r.Size.Height + " / Size.Width = " + r.Size.Width + Environment.NewLine;
            txt += "Location.X " + r.Location.X + " / Location.Y = " + r.Location.Y + Environment.NewLine + Environment.NewLine;

            return txt;
        }

        /// <summary>
        ///  Debug all screens positioning information in ClipBoard
        /// </summary>
        private void debugAllScreensInClipboard()
        {
            String debugTxt = "";
            if (Screen.AllScreens.Length > 0)
            {
                debugTxt += debugOneScreenToString(CommonData.dataInstance.Screen1TooltipData, CommonData.dataInstance.Screen1Name);
            }
            if (Screen.AllScreens.Length > 1)
            {
                debugTxt += debugOneScreenToString(CommonData.dataInstance.Screen2TooltipData, CommonData.dataInstance.Screen2Name);
                if (Screen.AllScreens.Length > 2)
                {
                    debugTxt += debugOneScreenToString(CommonData.dataInstance.Screen3TooltipData, CommonData.dataInstance.Screen3Name);
                    if (Screen.AllScreens.Length > 3)
                    {
                        debugTxt += debugOneScreenToString(CommonData.dataInstance.Screen4TooltipData, CommonData.dataInstance.Screen4Name);
                        if (Screen.AllScreens.Length > 4)
                        {
                            debugTxt += debugOneScreenToString(CommonData.dataInstance.Screen5TooltipData, CommonData.dataInstance.Screen5Name);
                            if (Screen.AllScreens.Length > 5)
                            {
                                debugTxt += debugOneScreenToString(CommonData.dataInstance.Screen6TooltipData, CommonData.dataInstance.Screen6Name);
                            }
                        }
                    }
                }
            }
            System.Windows.Clipboard.SetText(debugTxt);
        }

        private void miAbout_Click(object sender, RoutedEventArgs e)
        {
            // @TODO
            spUpdatesHistory.Visibility = Visibility.Visible;
        }

        private void tbScreen1Name_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            debugAllScreensInClipboard();
        }

        private void expUpdatesHistory_Collapsed(object sender, RoutedEventArgs e)
        {
            spUpdatesHistory.Visibility = Visibility.Collapsed;
            expUpdatesHistory.IsExpanded = true;
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            spUpdatesHistory.Visibility = spUpdatesHistory.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

         private void Hyperlink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Hyperlink hl = (Hyperlink)sender;
            System.Diagnostics.Process.Start(hl.NavigateUri.AbsoluteUri);
        }
    }
}
