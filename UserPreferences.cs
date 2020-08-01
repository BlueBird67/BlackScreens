namespace BlackScreensWPF
{
    public class UserPreferences
    {
        internal int _Opacity = 90;
        public int Opacity
        {
            get { return _Opacity; }
            set { _Opacity = value; }
        }

        internal bool _ShowTextsOnBlackScreens = true;
        public bool ShowTextsOnBlackScreens
        {
            get { return _ShowTextsOnBlackScreens; }
            set { _ShowTextsOnBlackScreens = value; }
        }

        internal bool _ClickThrough = false;
        public bool ClickThrough
        {
            get { return _ClickThrough; }
            set { _ClickThrough = value; }
        }

        public int MsDelayMouseCursorHide 
        {
            get { return _MsDelayMouseCursorHide; }
            set { _MsDelayMouseCursorHide = value; }
        }
        internal int _MsDelayMouseCursorHide = 3000;
    }
}
