namespace ServiceImplementation.FBInstant.Player
{
#if FB_INSTANT_PRODUCTION
    using System.Runtime.InteropServices;

#else
    using System;
#endif

    internal static class FBInstantPlayer
    {
#if FB_INSTANT_PRODUCTION
        [DllImport("__Internal")]
        internal static extern string GetUserId();

        [DllImport("__Internal")]
        internal static extern string GetUserName();

        [DllImport("__Internal")]
        internal static extern string GetUserAvatar();

        [DllImport("__Internal")]
        internal static extern void SaveUserData(string json, string callbackObj, string callbackFunc, string callbackId);

        [DllImport("__Internal")]
        internal static extern void LoadUserData(string keys, string callbackObj, string callbackFunc, string callbackId);
#else
        internal static string GetUserId() => "th31";

        internal static string GetUserName() => "TheOneStudio";

        internal static string GetUserAvatar() => "https://cdn.vox-cdn.com/thumbor/WR9hE8wvdM4hfHysXitls9_bCZI=/0x0:1192x795/1400x1400/filters:focal(596x398:597x399)/cdn.vox-cdn.com/uploads/chorus_asset/file/22312759/rickroll_4k.jpg";

        internal static void SaveUserData(string json, string callbackObj, string callbackFunc, string callbackId)
        {
            throw new NotImplementedException("This function should only be called in production");
        }

        internal static void LoadUserData(string keys, string callbackObj, string callbackFunc, string callbackId)
        {
            throw new NotImplementedException("This function should only be called in production");
        }
#endif
    }
}