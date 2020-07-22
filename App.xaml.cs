using BlackScreenAppWPF;
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
        }

        private bool Kh_KeyDown(int wParam, KeyboardHookData lParam)
        {
            String keyToUseTmp = "";
            bool keyHandled = false;
            Keys keyData = (Keys)lParam.vkCode;
            // Testing alt+x key pressed
            if (kh.AltHeld)
            {
                int screenNumKey = -1;
                if (keyData == Keys.D1)
                {
                    screenNumKey = 1;
                    keyToUseTmp = "ALT + 1";
                    keyHandled = true;
                }
                else
                if ((keyData == Keys.D2) && (Screen.AllScreens.Length > 1))
                {
                    screenNumKey = 2;
                    keyToUseTmp = "ALT + 2";
                    keyHandled = true;
                }
                else
                if (keyData == Keys.D3 && (Screen.AllScreens.Length > 2))
                {
                    screenNumKey = 3;
                    keyToUseTmp = "ALT + 3";
                    keyHandled = true;
                }
                else
                if (keyData == Keys.D4 && (Screen.AllScreens.Length > 3))
                {
                    screenNumKey = 4;
                    keyToUseTmp = "ALT + 4";
                    keyHandled = true;
                }
                else
                if (keyData == Keys.D5 && (Screen.AllScreens.Length > 4))
                {
                    screenNumKey = 5;
                    keyToUseTmp = "ALT + 5";
                    keyHandled = true;
                }
                else
                if (keyData == Keys.D6 && (Screen.AllScreens.Length > 5))
                {
                    screenNumKey = 6;
                    keyToUseTmp = "ALT + 6";
                    keyHandled = true;
                }
                // If any valide alt+X key, and screen associated
                if ((screenNumKey > -1) && (screenNumKey <= Screen.AllScreens.Length))
                {
                    if (CommonData.dataInstance.BlackWindows[screenNumKey - 1] == null)
                    { // Creating first time BlackWindows for this screen
                        CommonData.dataInstance.BlackWindows[screenNumKey - 1] = new BlackWindow();
                        //BlackWindows[screenNumKey - 1].fParams = fParams;
                        CommonData.dataInstance.BlackWindows[screenNumKey - 1].updateBlackWindowParams();
                    }
                    BlackWindow currentBlackWindow = CommonData.dataInstance.BlackWindows[screenNumKey - 1];
                    if (currentBlackWindow.IsVisible)
                    { // Hidding black screen window if already visible
                        currentBlackWindow.HideWindow();
                        CommonData.dataInstance.FParams.Topmost = false;
                    }
                    else
                    { // Showing Blackscreen window if not visible
                        Screen myScreen = Screen.AllScreens[screenNumKey - 1];
                        Rectangle bounds = myScreen.Bounds;
                        //currentBlackWindow.Left = bounds.X;
                        //currentBlackWindow.Top = bounds.Y;
                        currentBlackWindow.Left = bounds.Left;
                        currentBlackWindow.Top = bounds.Top;
                        currentBlackWindow.Height = bounds.Height;
                        currentBlackWindow.Width = bounds.Width;
                        currentBlackWindow.KeyToUse = "Use key "+keyToUseTmp+" to switch";
                        currentBlackWindow.ShowWindow();
                        // Hidding parameters window if it's on the same black screen to switch visible
                        if (CommonData.dataInstance.ParamsScreenDeviceName == currentBlackWindow.ScreenDeviceName)
                        {
                            CommonData.dataInstance.FParams.WindowState = WindowState.Minimized;
                        }
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