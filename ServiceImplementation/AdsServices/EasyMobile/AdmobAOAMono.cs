namespace ServiceImplementation.AdsServices.EasyMobile
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using Zenject;

    //Need to init AdsMob through mono, it doesn't work if init in Zenject's initialize
    public class AdmobAOAMono : MonoBehaviour
    {
#if ADMOB
        private AdModWrapper adModWrapper;

        [Inject]
        public void Init(AdModWrapper adModWrapper) { this.adModWrapper = adModWrapper; }

        private async void Start()
        {
            await UniTask.WaitUntil(() => this.adModWrapper != null);
            this.adModWrapper.Init();
        }
#endif
    }
}