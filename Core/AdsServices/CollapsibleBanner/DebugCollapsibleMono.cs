namespace Core.AdsServices.CollapsibleBanner
{
    using UnityEngine;
    using Zenject;

    public class DebugCollapsibleMono : MonoBehaviour
    {
        private ICollapsibleBanner collapsibleBanner;

        [Inject]
        private void Init(ICollapsibleBanner collapsibleBanner) { this.collapsibleBanner = collapsibleBanner; }

        private void OnGUI()
        {
            if (this.collapsibleBanner == null) return;
            if (GUI.Button(new Rect(200f, 100f, 200f, 120f), "Load Ad"))
            {
                this.collapsibleBanner.LoadBanner();
            }

            if (GUI.Button(new Rect(200f, 300f, 200f, 120f), "Show Ad"))
            {
                this.collapsibleBanner.ShowBanner();
            }

            if (GUI.Button(new Rect(Screen.width - 400f, 100f, 200f, 120f), "Hide Ad"))
            {
                this.collapsibleBanner.HideBanner();
            }

            if (GUI.Button(new Rect(Screen.width - 400f, 300f, 200f, 120f), "Destroy Ad"))
            {
                this.collapsibleBanner.DestroyCollapsibleBanner();
            }
        }
    }
}