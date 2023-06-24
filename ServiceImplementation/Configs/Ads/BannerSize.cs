namespace ServiceImplementation.Configs.Ads
{
    public class BannerSize
    {
        public int width, height;

        public BannerSize(int width, int height)
        {
            this.width  = width;
            this.height = height;
        }

        public override bool Equals(object obj)
        {
            var other = obj as BannerSize;

            if (other == null)
                return false;

            return this.width.Equals(other.width) && this.height.Equals(other.height);
        }

        public override int GetHashCode() { return this.width.GetHashCode(); }
        
        public static bool operator ==(BannerSize bannerSize1, BannerSize bannerSize2) { return bannerSize1?.Equals(bannerSize2) ?? ReferenceEquals(bannerSize2, null); }

        public static bool operator !=(BannerSize bannerSize1, BannerSize bannerSize2) { return !(bannerSize1 == bannerSize2); }
    }
}