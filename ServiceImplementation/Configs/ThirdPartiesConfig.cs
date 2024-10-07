namespace ServiceImplementation.Configs
{

    using System;
    using GameFoundation.Signals;
    using ServiceImplementation.Configs.Ads;
    using ServiceImplementation.FireBaseRemoteConfig;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = nameof(ThirdPartiesConfig), menuName = "ScriptableObjects/SpawnThirdPartiesConfig", order = 1)]
    public class ThirdPartiesConfig : ScriptableObject
    {
        public static    string    ResourcePath = $"GameConfigs/{nameof(ThirdPartiesConfig)}";

        public AdSettings AdSettings => this.mAdvertisingSettings;

        [SerializeField] [LabelText("Advertising Setting")]
        private AdSettings mAdvertisingSettings = null;

        private readonly SignalBus signalBus;
        public ThirdPartiesConfig(SignalBus signalBus) { this.signalBus = signalBus; }

        private void Awake()
        {
            this.signalBus.Subscribe<RemoteConfigFetchedSucceededSignal>(this.OnRemoteConfigFetched);
        }

        private void OnRemoteConfigFetched(RemoteConfigFetchedSucceededSignal signal)
        {
            Debug.Log("oneLog: Remote config fetched");
            this.AdSettings.AOAThreshHold = signal.RemoteConfig.GetRemoteConfigIntValue(RemoteConfigKey.AOALoadingThreshHold, 5);
        }
    }
}