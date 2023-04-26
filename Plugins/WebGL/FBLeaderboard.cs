namespace Plugins.WebGL
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;

    public class FBLeaderboard
    {

        /// <summary>
        /// callback after set leaderboard score
        /// </summary>
        public static System.Action setScoreAsync_Callback = null;

        /// <summary>
        /// callback after get player entry at leaderboard
        /// </summary>
        public static System.Action<FBLeaderboardEntry> getPlayerEntryAsync_Callback = null;

        /// <summary>
        /// callback after get player entries at leaderboard
        /// </summary>
        public static System.Action<FBLeaderboardEntry[]> getEntriesAsync_Callback = null;

        /// <summary>
        /// callback after get friends entries at leaderboard
        /// </summary>
        public static System.Action<FBLeaderboardEntry[]> getConnectedPlayerEntriesAsync_Callback = null;

        [DllImport("__Internal")]
        public static extern void leaderboard_setScoreAsync(string keyName, string extraDataJsonStr, long score);

        [DllImport("__Internal")]
        public static extern void leaderboard_getPlayerEntryAsync(string keyName);
        
        [DllImport("__Internal")]
        public static extern void leaderboard_getEntriesAsync(string keyName, int limit);

        [DllImport("__Internal")]
        public static extern void leaderboard_getConnectedPlayerEntriesAsync(string keyName, int limit);

        public void setScoreAsync(string keyName, long score, Dictionary<string, object> extraData, System.Action cb)
        {
            setScoreAsync_Callback = cb;
            string extraDataJsonStr = extraData.Count > 0 ? JsonConvert.SerializeObject(extraData) : "";
            leaderboard_setScoreAsync(keyName, extraDataJsonStr, score);
        }

        public void getPlayerEntryAsync(string keyName, System.Action<FBLeaderboardEntry> cb)
        {
            getPlayerEntryAsync_Callback = cb;
            leaderboard_getPlayerEntryAsync(keyName);
        }

        public void getEntriesAsync(string keyName, int limit, System.Action<FBLeaderboardEntry[]> cb)
        {
            getEntriesAsync_Callback = cb;
            leaderboard_getEntriesAsync(keyName, limit);
        }

        public void getConnectedPlayerEntriesAsync(string keyName, int limit, System.Action<FBLeaderboardEntry[]> cb)
        {
            getConnectedPlayerEntriesAsync_Callback = cb;
            leaderboard_getConnectedPlayerEntriesAsync(keyName, limit);
        }

    }

}
