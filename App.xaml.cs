using BlackScreenAppWPF;
using BlackScreens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Navigation;
using WindowsDisplayAPI.DisplayConfig;

namespace BlackScreensWPF
{
    public partial class App : System.Windows.Application
    {
        public KeyboardHook kh;

        void App_Startup(object sender, StartupEventArgs e)
        {
            // Check for duplicate launch of application
            String thisprocessname = Process.GetCurrentProcess().ProcessName;
            if (Process.GetProcesses().Count(p => p.ProcessName == thisprocessname) > 1)
                App.Current.Shutdown();

            // Initiating each screen tooltip
            CommonData.dataInstance.Screen1TooltipData = Screen.AllScreens[0].Bounds;
            CommonData.dataInstance.Screen2TooltipData = (Screen.AllScreens.Length > 1) ? Screen.AllScreens[1].Bounds : new Rectangle();
            CommonData.dataInstance.Screen3TooltipData = (Screen.AllScreens.Length > 2) ? Screen.AllScreens[2].Bounds : new Rectangle();
            CommonData.dataInstance.Screen4TooltipData = (Screen.AllScreens.Length > 3) ? Screen.AllScreens[3].Bounds : new Rectangle();
            CommonData.dataInstance.Screen5TooltipData = (Screen.AllScreens.Length > 4) ? Screen.AllScreens[4].Bounds : new Rectangle();
            CommonData.dataInstance.Screen6TooltipData = (Screen.AllScreens.Length > 5) ? Screen.AllScreens[5].Bounds : new Rectangle();

            //bool result = System.Windows.UI.ViewManagement.ApplicationViewScaling.TrySetDisableLayoutScaling(true);

            // Hook for listen to Windows Keys
            kh = new KeyboardHook(false);
            kh.KeyDown += Kh_KeyDown;
            kh.hook();

            try
            {
                CommonData.dataInstance.Displays = PathDisplayTarget.GetDisplayTargets();
            }
            catch (Exception)
            {

            }

            CommonData.dataInstance.FParams = new MainWindow();
            CommonData.dataInstance.loadUserConfigFile(CommonData.dataInstance.FParams);
            CommonData.dataInstance.LogToFile.Debug("App/xaml.cs.App_Startup() FirstAppLaunch = "+ CommonData.dataInstance.FirstAppLaunch);
            CommonData.dataInstance.LogToFile.Debug("App/xaml.cs.App_Startup() ReduceAppOnLaunch = " + CommonData.dataInstance.ReduceAppOnLaunch);
            if ((CommonData.dataInstance.FirstAppLaunch == true) || (CommonData.dataInstance.ReduceAppOnLaunch == false))
            {
                CommonData.dataInstance.FParams.showWindow();
            }
            else
            {
                CommonData.dataInstance.FParams.updateScreenNames();
                CommonData.dataInstance.FParams.minimizeWindow();
            }
            CommonData.dataInstance.FirstAppLaunch = false;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

        }

        /// <summary>
        /// Return number of active BlackScreen, meaning count of BlackWindow which are currently visible
        /// </summary>
        /// <returns></returns>
        private int numberOfActiveBlackScreen(BlackWindow[] bwArray)
        {
            int nbActiveBlackScreens = 0;
            foreach (BlackWindow bw in bwArray)
            {
                if ((bw != null) && (bw.IsVisible))
                {
                    nbActiveBlackScreens++;
                }
            }
            return nbActiveBlackScreens;
        }

        /// <summary>
        /// Init a new BlackWindow
        /// </summary>
        /// <returns></returns>
        private BlackWindow initNewBlackWindow(int screenNumber)
        {
            // Creating first time BlackWindows for this screen
            BlackWindow bw = new BlackWindow();
            bw.ScreenNumber = screenNumber;
            bw.updateBlackWindowParams();
            return bw;
        }

        /// <summary>
        /// Hide this specific BlackWindow
        /// </summary>
        /// <param name="bsTohide"></param>
        private void hideBlackWindow(BlackWindow bsTohide)
        {
            if (bsTohide != null)
            {
                bsTohide.HideWindow();
                CommonData.dataInstance.FParams.Topmost = false;
            }
        }

        /// <summary>
        /// Show a specific BlackWindow on associated screen
        /// </summary>
        /// <param name="bsToShow">BlackWindow to show</param>
        /// <param name="screen">Associated screen boundaries</param>
        /// <param name="bwNumber">BlackWindow number (1 to 6)</param>
        private void showBlackWindow(BlackWindow bwToShow, Screen screen, int bwNumber)
        {
            String textAltToShow = "ALT + " + bwNumber;
            Rectangle bounds = screen.Bounds;
            //currentBlackWindow.Left = bounds.X;
            //currentBlackWindow.Top = bounds.Y;
            bwToShow.Left = bounds.Left;
            bwToShow.Top = bounds.Top;
            bwToShow.Height = bounds.Height;
            bwToShow.Width = bounds.Width;
            bwToShow.KeyToUse = "Use key " + textAltToShow + " to switch";
            bwToShow.ScreenNumber = bwNumber;
            bwToShow.ShowWindow();
            // Hidding parameters window if it's on the same black screen to switch visible
            if (CommonData.dataInstance.ParamsScreenDeviceName == bwToShow.ScreenDeviceName)
            {
                //CommonData.dataInstance.FParams.WindowState = WindowState.Minimized;
                CommonData.dataInstance.FParams.minimizeWindow();
            }
        }

        /// <summary>
        /// Show all BlackWindows for all existing screens
        /// </summary>
        /// <param name="blackWindows">BlackWindow Array</param>
        private void showAllBlackWindows(BlackWindow[] blackWindows)
        {
            for (int iBw=0; iBw < blackWindows.Length; iBw++)
            {
                if (blackWindows[iBw] == null)
                    blackWindows[iBw] = initNewBlackWindow(iBw+1);
                showBlackWindow(blackWindows[iBw], Screen.AllScreens[iBw], iBw+1);
            }
        }

        /// <summary>
        /// Hide all black Windows for all existing screens
        /// </summary>
        /// <param name="blackWindows"></param>
        private void hideAllBlackWindows(BlackWindow[] blackWindows)
        {
            for (int iBw = 0; iBw < blackWindows.Length; iBw++)
            {
                hideBlackWindow(blackWindows[iBw]);
            }
        }

        private bool Kh_KeyDown(int wParam, KeyboardHookData lParam)
        {
            bool keyHandled = false;
            Keys keyData = (Keys)lParam.vkCode;
            // Testing alt+x key pressed
            if (kh.AltHeld)
            {
                int screenNumKey = -1;
                if (keyData == Keys.D0) {
                    keyHandled = true;
                    int nbOfActiveScreen = numberOfActiveBlackScreen(CommonData.dataInstance.BlackWindows);
                    // Showing all black screens if number of screens actually shows is less than half of existing screens
                    if (nbOfActiveScreen <= ((Double)CommonData.dataInstance.BlackWindows.Length/2))
                    {
                        showAllBlackWindows(CommonData.dataInstance.BlackWindows);
                    }
                    else // Hide all screens
                    if (nbOfActiveScreen > (CommonData.dataInstance.BlackWindows.Length / 2))
                    {
                        hideAllBlackWindows(CommonData.dataInstance.BlackWindows);
                    }
                } else
                if (keyData == Keys.D1)
                {
                    screenNumKey = 1;
                    keyHandled = true;
                }
                else
                if ((keyData == Keys.D2) && (Screen.AllScreens.Length > 1))
                {
                    screenNumKey = 2;
                    keyHandled = true;
                }
                else
                if (keyData == Keys.D3 && (Screen.AllScreens.Length > 2))
                {
                    screenNumKey = 3;
                    keyHandled = true;
                }
                else
                if (keyData == Keys.D4 && (Screen.AllScreens.Length > 3))
                {
                    screenNumKey = 4;
                    keyHandled = true;
                }
                else
                if (keyData == Keys.D5 && (Screen.AllScreens.Length > 4))
                {
                    screenNumKey = 5;
                    keyHandled = true;
                }
                else
                if (keyData == Keys.D6 && (Screen.AllScreens.Length > 5))
                {
                    screenNumKey = 6;
                    keyHandled = true;
                }
                // If any valide alt+X key, and screen associated
                if ((screenNumKey > 0) && (screenNumKey <= Screen.AllScreens.Length))
                {
                    if (CommonData.dataInstance.BlackWindows[screenNumKey - 1] == null)
                    { // Creating first time BlackWindows for this screen
                        CommonData.dataInstance.BlackWindows[screenNumKey - 1] = initNewBlackWindow(screenNumKey);
                    }
                    BlackWindow currentBlackWindow = CommonData.dataInstance.BlackWindows[screenNumKey - 1];
                    if (currentBlackWindow.IsVisible)
                    { // Hidding black screen window if already visible
                        hideBlackWindow(currentBlackWindow);
                    }
                    else
                    { // Showing Blackscreen window if not visible
                        showBlackWindow(currentBlackWindow, Screen.AllScreens[screenNumKey - 1], screenNumKey);
                    }
                }
            }
            // If keyHandled == true, current keypressed will not be handled anymore by Windows
            // (in certain cases, let it continue to be handled by Microsoft Windows launch Windows system beep sound each time alt+x key was pressed...)
            return keyHandled;
        }

        void App_Exit(object sender, ExitEventArgs e)
        {
            kh.unhook();
        }
    }
}