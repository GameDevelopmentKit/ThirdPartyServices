#if ADJUST

namespace ServiceImplementation.AdjustAnalyticTracker
{
    using Core.AnalyticServices.Signal;
    using GameFoundation.Scripts.Utilities.Extension;
    using UnityEngine;
    using Zenject;

    public class AdjustMono : MonoBehaviour
    {
        public static AdjustMono Create()
        {
            var obj = new GameObject();
            DontDestroyOnLoad(obj);
            obj.name = "AdjustMono";

            return obj.AddComponent<AdjustMono>();
        }

        public void OnDeepLinking(string message)
        {
            Debug.Log($"AdjustMono onDeepLinking: {message}");

            this.GetCurrentContainer().Resolve<ISignalBus>().Fire(new DeeplinkActiveSignal()
            {
                Message = message
            });
        }
    }
}
#endif