namespace ServiceImplementation.Configs
{
    using ServiceImplementation.Configs.Ads;
    using Sirenix.OdinInspector;
    using UnityEngine;

    [CreateAssetMenu(fileName = "ThirdPartiesConfig", menuName = "ScriptableObjects/SpawnThirdPartiesConfig", order = 1)]
    public class ThirdPartiesConfig : ScriptableObject
    {
        #region Private members

        private static ThirdPartiesConfig sInstance;

        [SerializeField] [LabelText("Advertising Setting")]
        private AdSettings mAdvertisingSettings = null;

        #endregion
    }
}