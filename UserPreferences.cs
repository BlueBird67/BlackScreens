namespace BlackScreensWPF
{
    public class UserPreferences
    {
        internal int _Opacity;
        public int Opacity
        {
            get { return _Opacity; }
            set { _Opacity = value; }
        }

        internal bool _showTextsOnBlackScreens;
        public bool showTextsOnBlackScreens
        {
            get { return _showTextsOnBlackScreens; }
            set { _showTextsOnBlackScreens = value; }
        }

        internal bool _ClickThrough;
        public bool ClickThrough
        {
            get { return _ClickThrough; }
            set { _ClickThrough = value; }
        }
    }
}
