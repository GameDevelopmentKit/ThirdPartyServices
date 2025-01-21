namespace ServiceImplementation.Configs.Ads
{
    using System;
    using GameFoundation.DI;
    using GameFoundation.Signals;
    using ServiceImplementation.FireBaseRemoteConfig;
    using UnityEngine.Scripting;

    public partial class AdServicesConfig
    {
        #if BRAVESTARS
        public bool UseAoaResume { get; private set; }
        public bool AoaFirstOpen { get; private set; } // Aoa open game first time
        public bool AoaStartGame { get; private set; } // Aoa open game

        partial void FetchRemoteConfigPartial()
        {
            this.UseAoaResume = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.UseAoaResume);
            this.AoaFirstOpen = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.AoaFirstOpen);
            this.AoaStartGame = RemoteConfigHelpers.GetBoolRemoteValue(this.remoteConfig, this.remoteConfigSetting, RemoteConfigKey.AoaStartGame);
        }

        #endif
    }
}