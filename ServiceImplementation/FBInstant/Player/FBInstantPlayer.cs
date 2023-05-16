namespace ServiceImplementation.FBInstant.Player
{
    using System.Runtime.InteropServices;

    public static class FBInstantPlayer
    {
        [DllImport("__Internal")]
        public static extern string GetUserId();

        [DllImport("__Internal")]
        public static extern string GetUserName();

        [DllImport("__Internal")]
        public static extern string GetUserAvatar();

        [DllImport("__Internal")]
        public static extern void SaveUserData(string key, string json, string callbackObj, string callbackFunc, string callbackId);

        [DllImport("__Internal")]
        public static extern void LoadUserData(string key, string callbackObj, string callbackFunc, string callbackId);

        [DllImport("__Internal")]
        public static extern void player_canSubscribeBotAsync();

        [DllImport("__Internal")]
        public static extern void player_subscribeBotAsync();
    }
}