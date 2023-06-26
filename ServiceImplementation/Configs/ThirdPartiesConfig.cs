namespace ServiceImplementation.Configs
{
    using ServiceImplementation.Configs.Ads;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ThirdPartiesConfig", menuName = "ScriptableObjects/SpawnThirdPartiesConfig", order = 1)]
    public class ThirdPartiesConfig : ScriptableObject
    {
        #region Private members

        [SerializeField] private AdSettings mAdvertisingSettings = null;
        public                   AdSettings AdSettings => this.mAdvertisingSettings;

        #endregion
    }
}