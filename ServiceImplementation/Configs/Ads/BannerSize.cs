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

        public static bool operator ==(BannerSize bannerSize1, BannerSize bannerSize2) { return bannerSize1?.Equals(bannerSize2) ?? ReferenceEquals(bannerSize2, null); }

        public static bool operator !=(BannerSize bannerSize1, BannerSize bannerSize2) { return !(bannerSize1 == bannerSize2); }
    }
}