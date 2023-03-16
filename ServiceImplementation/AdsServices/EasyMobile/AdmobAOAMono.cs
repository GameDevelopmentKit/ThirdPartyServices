namespace ServiceImplementation.AdsServices.EasyMobile
{
    using UnityEngine;
    using Zenject;

    //Need to init AdsMob through mono, it doesn't work if init in Zenject's initialize
    public class AdmobAOAMono : MonoBehaviour
    {
#if EM_ADMOB
        private AdModWrapper adModWrapper;

        [Inject]
        public void Init(AdModWrapper adModWrapper)
        {
            this.adModWrapper = adModWrapper;
            this.adModWrapper.Init();
        }
#endif
    }
}