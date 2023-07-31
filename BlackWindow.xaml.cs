using NLog.Targets.Wrappers;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WindowsDisplayAPI.DisplayConfig;

namespace BlackScreensWPF
{
    /// <summary>
    /// Logique d'interaction pour BlackWindow.xaml
    /// </summary>
    public partial class BlackWindow : Window
    {
        private int mouseCursorTimerMsCount = 0;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_ASYNCWINDOWPOS = 0x4000;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE | SWP_ASYNCWINDOWPOS;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        // Used to force black window on top of all others OS windows
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private string screenDeviceName = "";
        private int screenNumber = -1;
        private Screen currentScreen;
        private string keyToUse;
        private DispatcherTimer mouseCursorTimer = new DispatcherTimer();

        private Storyboard sbClickHelp;
        private Storyboard sbScreenDeviceName;
        private Storyboard sbKeyboardHelp;

        public int ScreenNumber { get => screenNumber; set => screenNumber = value; }
        public string ScreenDeviceName { get => screenDeviceName; set => screenDeviceName = value; }
        public string KeyToUse { get => keyToUse; set => keyToUse = value; }
        public System.Windows.DpiScale currentDpi { get => getScreenDPI(); }

        public BlackWindow()
        {
            InitializeComponent();
            updateScreenDeviceName();
            this.DataContext = CommonData.dataInstance;
            createAllStoryBoards();            
        }

        public System.Windows.DpiScale getScreenDPI()
        {
            System.Windows.DpiScale tmpDpi = VisualTreeHelper.GetDpi(this as Visual);
            return tmpDpi;
        }

        /// <summary>
        /// Reset opacity of all information texts
        /// </summary>
        /// <param name="o">1 for showing normal text, 0 to hide it</param>
        public void resetInfoOpacity(double o)
        {
            tbClickHelp.Opacity = o;
            tbScreenDeviceName.Opacity = o;
            tbKeyboardHelp.Opacity = o;
        }

        public void HideWindow()
        {
            this.Hide();
        }

        /// <summary>
        /// Update this BlackWindow Opacity parameter, level read in CommonData
        /// </summary>
        public void updateBlackWindowParams()
        {
            this.Opacity = (Double)(CommonData.dataInstance.Opacity) / 100;
            this.Activate();
            setWindowsClickThrough(CommonData.dataInstance.ClickThrough);
            string imageNameToUse = "";
            switch (this.screenNumber)
            {
                case 1: imageNameToUse = CommonData.dataInstance.ImageFileNameScreen1; break;
                case 2: imageNameToUse = CommonData.dataInstance.ImageFileNameScreen2; break;
                case 3: imageNameToUse = CommonData.dataInstance.ImageFileNameScreen3; break;
                case 4: imageNameToUse = CommonData.dataInstance.ImageFileNameScreen4; break;
                case 5: imageNameToUse = CommonData.dataInstance.ImageFileNameScreen5; break;
                case 6: imageNameToUse = CommonData.dataInstance.ImageFileNameScreen6; break;
            }
            if (!String.IsNullOrEmpty(imageNameToUse))
            {
                ImageBrush imageBrush = new ImageBrush(new BitmapImage(new Uri(imageNameToUse, UriKind.RelativeOrAbsolute)));
                this.Background = imageBrush;
            }
            else
            {
                this.Background = new SolidColorBrush(System.Windows.Media.Colors.Black);
            }
        }

        /// <summary>
        /// Update the current device name (real screen name) of this window
        /// </summary>
        public void updateScreenDeviceName()
        {
            System.Windows.Interop.WindowInteropHelper wih = new System.Windows.Interop.WindowInteropHelper(this);
            this.currentScreen = Screen.FromHandle(wih.Handle);
            this.screenDeviceName = currentScreen.DeviceName;

            if (ScreenNumber > -1) { 
                PathDisplayTarget display = CommonData.dataInstance.findDisplayByScreenName(Screen.AllScreens[ScreenNumber-1].DeviceName);
                this.screenDeviceName = Screen.AllScreens[ScreenNumber - 1].DeviceName;
            }
        }

        private void putWindowsOnTop()
        {
            CommonData.dataInstance.LogToFile.Debug("BlackWindow.putWindowsOnTop()");
            // Set BlackWindow top most against all others Microsoft Windows windows
            System.Windows.Interop.WindowInteropHelper wih = new System.Windows.Interop.WindowInteropHelper(this);
            bool res = SetWindowPos(wih.Handle, HWND_TOPMOST, 100, 100, 300, 300, TOPMOST_FLAGS);
            CommonData.dataInstance.LogToFile.Debug("BlackWindow.putWindowsOnTop().SetWindowsPos() result = " + res);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CommonData.dataInstance.LogToFile.Debug("BlackWindow.Window_Loaded()");
            setWindowsClickThrough(false);
            updateScreenDeviceName();
            mouseCursorTimer.Tick += mouseCursorTimerTick;
            mouseCursorTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            putWindowsOnTop();
        }

        private void mouseCursorTimerTick(object sender, EventArgs e)
        {
            mouseCursorTimerMsCount += 100;
            if (mouseCursorTimerMsCount > CommonData.dataInstance.MsDelayMouseCursorHide)
            {
                mouseCursorTimer.Stop();
                this.blackWindow.Cursor = System.Windows.Input.Cursors.None;
                mouseCursorTimerMsCount = 0;
            }
        }

        private void setWindowsClickThrough(bool clickThrough)
        {
            System.Windows.Interop.WindowInteropHelper wih = new System.Windows.Interop.WindowInteropHelper(this);
            var extendedStyle = GetWindowLong(wih.Handle, GWL_EXSTYLE);
            if (clickThrough) // Allow mouse clickthrough on this window
                SetWindowLong(wih.Handle, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            else // Cancel mouse clickthrough
                SetWindowLong(wih.Handle, GWL_EXSTYLE, 0);
        }

        private void spMain_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CommonData.dataInstance.FParams.toggleShowHideWindow();
        }

        /// <summary>
        /// Create all Story Boards for text information fading effect
        /// </summary>
        public void createAllStoryBoards()
        {
            sbClickHelp = buildStoryboard("tbClickHelp");
            sbScreenDeviceName = buildStoryboard("tbScreenDeviceName");
            sbKeyboardHelp = buildStoryboard("tbKeyboardHelp");

            sbClickHelp.Completed += (s, a) => tbClickHelp.Opacity = 0;
            sbScreenDeviceName.Completed += (s, a) => tbScreenDeviceName.Opacity = 0;
            sbKeyboardHelp.Completed += (s, a) => tbKeyboardHelp.Opacity = 0;
        }

        /// <summary>
        /// Launch text information fading effects
        /// </summary>
        public void launchInfoAnimation()
        {
            // Reset fading animation even if it's already launch
            // Only solution that worked, from https://docs.microsoft.com/fr-fr/dotnet/framework/wpf/graphics-multimedia/how-to-set-a-property-after-animating-it-with-a-storyboard
            sbClickHelp.Remove(tbClickHelp);
            sbScreenDeviceName.Remove(tbScreenDeviceName);
            sbKeyboardHelp.Remove(tbKeyboardHelp);

            resetInfoOpacity(1.0);
            createAllStoryBoards();

            sbClickHelp.Begin(tbClickHelp, true);
            sbScreenDeviceName.Begin(tbScreenDeviceName, true);
            sbKeyboardHelp.Begin(tbKeyboardHelp, true);
        }

        public Storyboard buildStoryboard(String targetName)
        {
            DoubleAnimation da = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(4.5)));
            Storyboard.SetTargetName(da, targetName);
            da.FillBehavior = FillBehavior.Stop;
            Storyboard.SetTargetProperty(da, new PropertyPath("Opacity"));
            Storyboard sbResult = new Storyboard();
            sbResult.Children.Add(da);

            return sbResult;
        }

        /// <summary>
        /// Set everything for showing proper information for this BlackWindows
        /// </summary>
        public void ShowWindow()
        {
            CommonData.dataInstance.LogToFile.Debug("BlackWindow.ShowWindow()");
            this.tbKeyboardHelp.Text = this.KeyToUse;
            updateScreenDeviceName();
            this.tbScreenDeviceName.Text = CommonData.dataInstance.findDisplayByScreenName(this.screenDeviceName).FriendlyName;
            Visibility vTexts = CommonData.dataInstance.HideTexts ? Visibility.Hidden : Visibility.Visible;
            tbScreenDeviceName.Visibility = vTexts;
            tbClickHelp.Visibility = vTexts;
            tbKeyboardHelp.Visibility = vTexts;
            this.Show();
            this.Activate();
            // Ensure that black Window is on top, even if Windows.Minized was used on main screen
            putWindowsOnTop();
            launchInfoAnimation();
            // Trying to correct Windows system DPI scale for screens, but not working
            // What worked is updating the app.manifest with 
            // <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">unaware</dpiAwareness>
            /*DpiScale currentDpi = this.getScreenDPI();
            this.Height = this.Height / currentDpi.DpiScaleY;
            this.Width = this.Width / currentDpi.DpiScaleX;*/
        }

        private void Window_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // If mouse delay equal zero, then mouse is always showing
            if (CommonData.dataInstance.MsDelayMouseCursorHide > 0)
            {
                this.blackWindow.Cursor = System.Windows.Input.Cursors.Arrow;
                mouseCursorTimerMsCount = 0;
                mouseCursorTimer.Stop();
                mouseCursorTimer.Start();
            }
        }
    }
}