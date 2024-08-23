namespace Core.AdsServices.Native
{
    using GameFoundation.Scripts.UIModule.ScreenFlow.Managers;
    using GameFoundation.Scripts.Utilities.Extension;
    using UnityEngine;

    // Disable collider when the screen is not active
    public class NativeAdsColliderUpdater : MonoBehaviour
    {
        public  string        screenPresenterName;
        private ScreenManager screenManager;
        private Collider[]    colliders;

        private void Awake()
        {
            this.screenManager = this.GetCurrentContainer().Resolve<ScreenManager>();
            this.colliders     = this.GetComponentsInChildren<Collider>(true);
        }

        private void Update()
        {
            if (this.screenManager.CurrentActiveScreen == null) return;
            var isActive = this.screenManager.CurrentActiveScreen.Value.GetType().Name == this.screenPresenterName;
            foreach (var col in this.colliders)
            {
                col.enabled = isActive;
            }
        }
    }
}