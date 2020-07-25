using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace BlackScreensWPF
{
    /// <summary>
    /// Logique d'interaction pour BlackWindow.xaml
    /// </summary>
    public partial class BlackWindow : Window
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
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
        private Screen currentScreen;
        private string keyToUse;

        private Storyboard sbClickHelp;
        private Storyboard sbScreenDeviceName;
        private Storyboard sbKeyboardHelp;

        public string ScreenDeviceName { get => screenDeviceName; set => screenDeviceName = value; }
        public string KeyToUse { get => keyToUse; set => keyToUse = value; }

        public BlackWindow()
        {
            InitializeComponent();
            updateScreenDeviceName();
            this.DataContext = CommonData.dataInstance;
            createAllStoryBoards();
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
            setWindowsClickThrough(CommonData.dataInstance.ClickThrough);
        }

        /// <summary>
        /// Update the current device name (real screen name) of this window
        /// </summary>
        public void updateScreenDeviceName()
        {
            System.Windows.Interop.WindowInteropHelper wih = new System.Windows.Interop.WindowInteropHelper(this);
            this.currentScreen = Screen.FromHandle(wih.Handle);
            this.screenDeviceName = currentScreen.DeviceName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Set BlackWindow top most against all others Microsoft Windows windows
            System.Windows.Interop.WindowInteropHelper wih = new System.Windows.Interop.WindowInteropHelper(this);
            SetWindowPos(wih.Handle, HWND_TOPMOST, 100, 100, 300, 300, TOPMOST_FLAGS);
            setWindowsClickThrough(false);
            updateScreenDeviceName();
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
            updateScreenDeviceName();
            this.tbScreenDeviceName.Text = CommonData.dataInstance.findDisplayByScreenName(this.screenDeviceName).FriendlyName;
            Visibility vTexts = CommonData.dataInstance.HideTexts ? Visibility.Hidden : Visibility.Visible;
            tbScreenDeviceName.Visibility = vTexts;
            tbClickHelp.Visibility = vTexts;
            tbKeyboardHelp.Visibility = vTexts;
            this.tbKeyboardHelp.Text = this.KeyToUse;
            this.Show();
            launchInfoAnimation();
        }
    }
}
