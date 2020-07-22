// MP Hooks © 2016 Mitchell Pell
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlackScreenAppWPF
{
    /// <summary>
    /// C# Structure wrapper for Win32 C++ KBDLLHOOKSTRUCT
    /// <see cref="https://msdn.microsoft.com/en-us/library/windows/desktop/ms644967(v=vs.85).aspx"/>
    /// </summary>
    [Serializable]
    public struct KeyboardHookData {
        public int vkCode;
        public int scanCode;
        public int flags;
        public int time;
        public int dwExtraInfo;
    }   
}