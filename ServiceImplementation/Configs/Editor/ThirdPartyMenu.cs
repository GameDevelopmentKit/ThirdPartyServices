namespace ServiceImplementation.Configs.Editor
{
    using UnityEditor;
    using UnityEngine;

    public static class ThirdPartyMenu
    {
        [MenuItem("Tools/TheOneStudio/Third Party/Advertising")]
        public static void OpenThirdPartyAsset()
        {
            var asset = Resources.Load(nameof(ThirdPartiesConfig));
            Selection.activeObject = asset;
        }
    }
}