namespace Core.AdsServices.CollapsibleBanner
{
    using UnityEngine;
    using Zenject;

    public class DebugCollapsibleMono : MonoBehaviour
    {
        private ICollapsibleBannerAd collapsibleBannerAd;

        [Inject]
        private void Init(ICollapsibleBannerAd collapsibleBannerAd) { this.collapsibleBannerAd = collapsibleBannerAd; }

        private void OnGUI()
        {
            if (this.collapsibleBannerAd == null) return;
            if (GUI.Button(new Rect(200f, 300f, 200f, 120f), "Show Ad"))
            {
                this.collapsibleBannerAd.ShowCollapsibleBannerAd();
            }

            if (GUI.Button(new Rect(Screen.width - 400f, 100f, 200f, 120f), "Hide Ad"))
            {
                this.collapsibleBannerAd.HideCollapsibleBannerAd();
            }

            if (GUI.Button(new Rect(Screen.width - 400f, 300f, 200f, 120f), "Destroy Ad"))
            {
                this.collapsibleBannerAd.DestroyCollapsibleBannerAd();
            }
        }
    }
}