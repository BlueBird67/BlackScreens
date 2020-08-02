using BlackScreenAppWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace BlackScreensWPF
{

    /// <summary>
    /// keyboard Hook Process called hooked to and called by Windows.
    /// </summary>
    /// <param name="code">A code the hook procedure uses to determine 
    /// how to process the message.</param>
    /// <param name="wParam">The virtual-key code of the key that generated 
    /// the keystroke message.</param>
    /// <param name="lParam">The repeat count, scan code, extended-key flag, 
    /// context code, previous key-state flag,</param>                                                       
    /// <returns></returns>
    public delegate int keyboardHookProc(int code, int wParam, ref KeyboardHookData lParam);

    /// <summary>
    /// Keyboard Hook Event called by <typeparamref name="KeyboardHook"/>.
    /// </summary>
    /// <param name="wParam">The virtual-key code of the key that generated 
    /// the keystroke message.</param>
    /// <param name="lParam">The repeat count, scan code, extended-key flag, 
    /// context code, previous key-state flag,</param>     
    public delegate bool KeyboardHookEvent(int wParam, KeyboardHookData lParam);

    /// <summary>
    /// Wrapper class for a Win32 Keyboard event hook.
    /// </summary>
    public class KeyboardHook
    {
        //#############################################################       
        #region [# Win32 Constants #]

        /// <summary>
        /// The WH_KEYBOARD_LL hook enables you to monitor keyboard 
        /// input events about to be posted in a thread input queue. 
        /// </summary>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644959%28v=vs.85%29.aspx#wh_keyboard_llhook"/>
        public static readonly int WH_KEYBOARD_LL = 13;
        /// <summary>
        ///  
        /// </summary>
        public static readonly int WM_KEYDOWN = 0x100;
        /// <summary>
        /// 
        /// </summary>
        public static readonly int WM_KEYUP = 0x101;
        /// <summary>
        /// 
        /// </summary>
        public static readonly int WM_SYSKEYDOWN = 0x104;
        /// <summary>
        /// 
        /// </summary>
        public static readonly int WM_SYSKEYUP = 0x105;

        //public static IntPtr hInstance = LoadLibrary("User32");

        #endregion
        //#############################################################
        #region [# Properties #]

        protected IntPtr hhook = IntPtr.Zero;
        protected keyboardHookProc hookDelegate;

        #endregion
        //#############################################################  
        #region [# Flags #]        

        public bool Hooked { get { return bHooked; } }
        private volatile bool bHooked = false;
        public bool LeftShiftHeld { get { return bLeftShiftHeld; } }
        private volatile bool bLeftShiftHeld = false;
        public bool RightShiftHeld { get { return bRightShiftHeld; } }
        private volatile bool bRightShiftHeld = false;
        public bool ShiftHeld { get { return bShiftHeld; } }
        private volatile bool bShiftHeld = false;
        public bool AltHeld { get { return bAltHeld; } }
        private volatile bool bAltHeld = false;
        public bool CtrlHeld { get { return bCtrlHeld; } }
        private volatile bool bCtrlHeld = false;

        #endregion
        //#############################################################
        #region [# Events #]

        /// <summary>
        /// KeyDown event for when a key is pressed down.
        /// </summary>
        public event KeyboardHookEvent KeyDown;
        /// <summary>
        /// KeyUp event for then the key is released.
        /// </summary>
        public event KeyboardHookEvent KeyUp;

        #endregion
        //#############################################################
        #region [# Construction / Destruction #]

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="autoHook"></param>
        public KeyboardHook(bool autoHook = false) { if (autoHook) hook(); }

        /// <summary>
        /// Destructor
        /// </summary>
        ~KeyboardHook() { unhook(); }

        #endregion   
        //#############################################################
        #region [# Hooks #]

        /// <summary>
        /// Hooks keyboard event process <paramref name="_hookProc"/> from Windows.
        /// </summary>
        public virtual void hook()
        {
            hookDelegate = new keyboardHookProc(_hookProc);
            //Get library instance
            IntPtr hInstance = LoadLibrary("User32");
            //Call library hook function
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, hookDelegate, hInstance, 0);
            //Set bHooked to true if successful.
            bHooked = (hhook != null);
        }

        /// <summary>
        /// Unhooks the keyboard event process from Windows.
        /// </summary>
        public virtual void unhook()
        {
            //Call library unhook function
            UnhookWindowsHookEx(hhook);
            bHooked = false;
        }

        /// <summary>
        /// Private hook that checks the return code 
        /// and calls the overridden hook process <paramref name="hookProc"/> 
        /// </summary>
        /// <param name="code">A code the hook procedure uses to determine 
        /// how to process the message.</param>
        /// <param name="wParam">The virtual-key code of the key that generated 
        /// the keystroke message.</param>
        /// <param name="lParam">The repeat count, scan code, extended-key flag, 
        /// context code, previous key-state flag,</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644984%28v=vs.85%29.aspx"/>
        /// <returns></returns>
        private int _hookProc(int code, int wParam, ref KeyboardHookData lParam)
        {
            if (code >= 0)
            {
                //Pass on for other objects to process.
                return this.hookProc(code, wParam, ref lParam);
            }
            else
                return CallNextHookEx(hhook, code, wParam, ref lParam);
        }

        /// <summary>
        /// Overridable function called by the hooked procedure
        /// function <typeparamref name="_hookProc"/>.
        /// </summary>
        /// <param name="code">A code the hook procedure uses to determine 
        /// how to process the message.</param>
        /// <param name="wParam">The virtual-key code of the key that generated 
        /// the keystroke message.</param>
        /// <param name="lParam">The repeat count, scan code, extended-key flag, 
        /// context code, previous key-state flag,</param>
        /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644984%28v=vs.85%29.aspx"/>
        /// <returns></returns>
        public virtual int hookProc(
            int code, int wParam, ref KeyboardHookData lParam)
        {

            Keys k = (Keys)lParam.vkCode;

            // Check for shift(s), alt, and ctrl.

            // Shift
            if (k == Keys.LShiftKey)
                bLeftShiftHeld = bShiftHeld = (wParam == WM_KEYDOWN);
            else if (k == Keys.RShiftKey)
                bRightShiftHeld = bShiftHeld = (wParam == WM_KEYDOWN);
            // Control
            if ((lParam.vkCode & 0xA2) == 0xA2 || (lParam.vkCode & 0xA3) == 0xA3)
            {
                bCtrlHeld = (wParam == WM_KEYDOWN);
                //return 1;
            }
            // 164 mean ALT, 165 mean ALT+GR
            if (lParam.vkCode == 164)
            {
                bAltHeld = (wParam == WM_SYSKEYDOWN);
                //return 1;
            }

            //Key Press Event        
            KeyEventArgs kea = new KeyEventArgs(k);
            bool handled = false;
            if ((wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN) && (KeyDown != null))
            {
                handled = KeyDown(wParam, lParam);
            }
            else if ((wParam == WM_KEYUP || wParam == WM_SYSKEYUP) && (KeyUp != null))
            {
                handled = KeyUp(wParam, lParam);
            }
            if ((kea.Handled) || (handled))
                return 1;

            return CallNextHookEx(hhook, code, wParam, ref lParam);
        }

        #endregion
        //#############################################################
        #region [# DLL Imports #]

        /// <summary>
        /// Sets the windows hook, do the desired event, one of hInstance or threadId must be non-null
        /// </summary>
        /// <param name="idHook">The id of the event you want to hook</param>
        /// <param name="callback">The callback.</param>
        /// <param name="hInstance">The handle you want to attach the event to, can be null</param>
        /// <param name="threadId">The thread you want to attach the event to, can be null</param>
        /// <returns>a handle to the desired hook</returns>
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, keyboardHookProc callback, IntPtr hInstance, uint threadId);

        /// <summary>
        /// Unhooks the windows hook.
        /// </summary>
        /// <param name="hInstance">The hook handle that was returned from SetWindowsHookEx</param>
        /// <returns>True if successful, false otherwise</returns>
        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        /// <summary>
        /// Calls the next hook.
        /// </summary>
        /// <param name="idHook">The hook id</param>
        /// <param name="nCode">The hook code</param>
        /// <param name="wParam">The wparam.</param>
        /// <param name="lParam">The lparam.</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyboardHookData lParam);

        /// <summary>
        /// Loads the library.
        /// </summary>
        /// <param name="lpFileName">Name of the library</param>
        /// <returns>A handle to the library</returns>
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        #endregion
    }
}