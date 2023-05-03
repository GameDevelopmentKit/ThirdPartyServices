namespace ServiceImplementation.FBInstant.Leaderboard
{
    using System.Collections.Generic;

    [System.Serializable]
    public class FBLeaderboardEntry
    {
        public int rank;
        public string avatarUrl;
        public string nickName;
        public int score;
        public Dictionary<string, object> extraData;
    }
}