namespace ServiceImplementation.AdsServices.EasyMobile
{
    using UnityEngine;
    using Zenject;

    public class AdmobAOAMono : MonoBehaviour
    {
        private AdModWrapper adModWrapper;

        [Inject]
        public void Init(AdModWrapper adModWrapper)
        {
            this.adModWrapper = adModWrapper;
            this.adModWrapper.Init();
        }
    }
}