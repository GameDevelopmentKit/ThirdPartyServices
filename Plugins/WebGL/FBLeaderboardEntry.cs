namespace Packages.com.gdk._3rd.Plugins.WebGL
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