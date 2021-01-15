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

        internal int _MsDelayMouseCursorHide = 3000;
        public int MsDelayMouseCursorHide 
        {
            get { return _MsDelayMouseCursorHide; }
            set { _MsDelayMouseCursorHide = value; }
        }

        internal string _ImageFileNameScreen1 = "";
        public string ImageFileNameScreen1
        {
            get { return _ImageFileNameScreen1; }
            set { _ImageFileNameScreen1 = value; }
        }

        internal string _ImageFileNameScreen2 = "";
        public string ImageFileNameScreen2
        {
            get { return _ImageFileNameScreen2; }
            set { _ImageFileNameScreen2 = value; }
        }

        internal string _ImageFileNameScreen3 = "";
        public string ImageFileNameScreen3
        {
            get { return _ImageFileNameScreen3; }
            set { _ImageFileNameScreen3 = value; }
        }

        internal string _ImageFileNameScreen4 = "";
        public string ImageFileNameScreen4
        {
            get { return _ImageFileNameScreen4; }
            set { _ImageFileNameScreen4 = value; }
        }

        internal string _ImageFileNameScreen5 = "";
        public string ImageFileNameScreen5
        {
            get { return _ImageFileNameScreen5; }
            set { _ImageFileNameScreen5 = value; }
        }

        internal string _ImageFileNameScreen6 = "";
        public string ImageFileNameScreen6
        {
            get { return _ImageFileNameScreen6; }
            set { _ImageFileNameScreen6 = value; }
        }
    }
}
