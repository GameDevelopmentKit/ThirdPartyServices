namespace ServiceImplementation.Configs
{
    using ServiceImplementation.Configs.Ads;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = nameof(ThirdPartiesConfig), menuName = "ScriptableObjects/SpawnThirdPartiesConfig", order = 1)]
    public class ThirdPartiesConfig : ScriptableObject
    {
        public static string ResourcePath = $"GameConfigs/{nameof(ThirdPartiesConfig)}";

        public AdSettings AdSettings => this.mAdvertisingSettings;

        [SerializeField] [LabelText("Advertising Setting")] private AdSettings mAdvertisingSettings = null;
    }
}