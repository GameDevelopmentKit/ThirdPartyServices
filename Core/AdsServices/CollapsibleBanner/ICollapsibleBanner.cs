namespace Core.AdsServices.CollapsibleBanner
{
    public interface ICollapsibleBanner
    {
        public void LoadBanner();
        public void ShowBanner();
        public void HideBanner();
        public void DestroyCollapsibleBanner();
    }
}