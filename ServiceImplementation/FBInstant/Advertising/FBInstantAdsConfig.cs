namespace ServiceImplementation.FBInstant.Advertising
{
    using System;
    using Models;
    using UnityEngine;

    [Serializable]
    [CreateAssetMenu(fileName = nameof(FBInstantAdsConfig), menuName = "Configs/" + nameof(FBInstantAdsConfig))]
    public class FBInstantAdsConfig : ScriptableObject, IGameConfig
    {
        [field: SerializeField] public string BannerAdId       { get; private set; }
        [field: SerializeField] public string InterstitialAdId { get; private set; }
        [field: SerializeField] public string RewardedAdId     { get; private set; }
    }
}