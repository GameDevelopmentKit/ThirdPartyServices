using ServiceImplementation.Configs;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class ThirdPartyPreprocessBuild : IPreprocessBuildWithReport
{
    public int callbackOrder => -1;

    public void OnPreprocessBuild(BuildReport report)
    {
        var thirdPartyConfig = Resources.Load<ThirdPartiesConfig>(ThirdPartiesConfig.ResourcePath);
        thirdPartyConfig.AdSettings.ImmersiveAds.SavePubScaleSetting();
    }
}