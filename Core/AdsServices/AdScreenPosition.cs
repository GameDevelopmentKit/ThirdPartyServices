namespace Core.AdsServices
{
    using Screen = UnityEngine.Device.Screen;

    public struct AdScreenPosition
    {
        public int x;
        public int y;

        public AdScreenPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static AdScreenPosition BottomCenter => new AdScreenPosition(Screen.width / 2, 0);

        public static AdScreenPosition BottomLeft => new AdScreenPosition(0, 0);

        public static AdScreenPosition BottomRight => new AdScreenPosition(Screen.width, 0);

        public static AdScreenPosition Centered => new AdScreenPosition(Screen.width / 2, Screen.height / 2);

        public static AdScreenPosition CenterLeft => new AdScreenPosition(0, Screen.height / 2);

        public static AdScreenPosition CenterRight => new AdScreenPosition(Screen.width, Screen.height / 2);

        public static AdScreenPosition TopLeft => new AdScreenPosition(0, Screen.height);

        public static AdScreenPosition TopCenter => new AdScreenPosition(Screen.width / 2, Screen.height);

        public static AdScreenPosition TopRight => new AdScreenPosition(Screen.width, Screen.height);
    }
}