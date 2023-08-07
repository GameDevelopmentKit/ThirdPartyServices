namespace ServiceImplementation.FBInstant.Advertising
{
    using System;
    using Models;
    using UnityEngine;

    [Serializable]
    [CreateAssetMenu(fileName = nameof(FbInstantAdConfig), menuName = "Configs/" + nameof(FbInstantAdConfig))]
    public class FbInstantAdConfig : ScriptableObject, IGameConfig
    {
        [field: SerializeField] public string[] BannerAdIds       { get; private set; }
        [field: SerializeField] public string[] InterstitialAdIds { get; private set; }
        [field: SerializeField] public string[] RewardedAdIds     { get; private set; }
    }
}