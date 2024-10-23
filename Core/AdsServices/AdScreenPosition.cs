namespace Core.AdsServices
{
    using UnityEngine;
    using Screen = UnityEngine.Device.Screen;

    public struct AdScreenPosition
    {
        public readonly float x;
        public readonly float y;

        public AdScreenPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static AdScreenPosition TopCenter    => new AdScreenPosition(Screen.safeArea.width / 2, Screen.safeArea.height);
        public static AdScreenPosition TopLeft      => new AdScreenPosition(0,                         Screen.safeArea.height);
        public static AdScreenPosition TopRight     => new AdScreenPosition(Screen.safeArea.width,     Screen.safeArea.height);
        public static AdScreenPosition Centered     => new AdScreenPosition(Screen.safeArea.width / 2, Screen.safeArea.height / 2);
        public static AdScreenPosition CenterLeft   => new AdScreenPosition(0,                         Screen.safeArea.height / 2);
        public static AdScreenPosition CenterRight  => new AdScreenPosition(Screen.safeArea.width,     Screen.safeArea.height / 2);
        public static AdScreenPosition BottomLeft   => new AdScreenPosition(0,                         0);
        public static AdScreenPosition BottomCenter => new AdScreenPosition(Screen.safeArea.width / 2, 0);
        public static AdScreenPosition BottomRight  => new AdScreenPosition(Screen.safeArea.width,     0);

        public static implicit operator Vector2(AdScreenPosition adScreenPosition) { return new Vector2(adScreenPosition.x, adScreenPosition.y); }

        public static bool operator ==(AdScreenPosition pos1, AdScreenPosition pos2) { return Vector2.Distance(pos1, pos2) < Mathf.Epsilon; }

        public static bool operator !=(AdScreenPosition pos1, AdScreenPosition pos2) { return !(pos1 == pos2); }

        public static AdScreenPosition operator +(AdScreenPosition pos1, AdScreenPosition pos2)
        {
            return new AdScreenPosition(pos1.x + pos2.x, pos1.y + pos2.y);
        }
        
        public static AdScreenPosition operator -(AdScreenPosition pos1, AdScreenPosition pos2)
        {
            return new AdScreenPosition(pos1.x - pos2.x, pos1.y - pos2.y);
        }
    }
}