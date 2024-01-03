namespace Core.AdsServices.CollapsibleBanner
{
    using UnityEngine;
    using Zenject;

    public class CollapsibleBannerAdDebug : MonoBehaviour
    {
        private Vector2 buttonSize;

        private ICollapsibleBannerAd collapsibleBannerAd;

        [Inject]
        private void Init(ICollapsibleBannerAd collapsibleBannerAd) { this.collapsibleBannerAd = collapsibleBannerAd; }

        private void Awake() { this.buttonSize = new Vector2(Screen.width * 0.15f, Screen.height * 0.1f); }

        private void OnGUI()
        {
            if (GUI.Button(new Rect(new Vector2(0f, Screen.height * 0.75f), this.buttonSize), "Show Collap"))
            {
                this.collapsibleBannerAd.ShowCollapsibleBannerAd();
            }

            if (GUI.Button(new Rect(new Vector2(0f, Screen.height * 0.55f), this.buttonSize), "Hide Collap"))
            {
                this.collapsibleBannerAd.HideCollapsibleBannerAd();
            }

            if (GUI.Button(new Rect(new Vector2(0f, Screen.height * 0.35f), this.buttonSize), "Destroy Collap"))
            {
                this.collapsibleBannerAd.DestroyCollapsibleBannerAd();
            }
        }
    }
}