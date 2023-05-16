namespace ServiceImplementation.FBInstant
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;
    using ServiceImplementation.FBInstant.Player;

    public class GameContext
    {
        /// <summary>
        /// callback after confirmed choose context
        /// </summary>
        public static System.Action chooseAsync_Callback = null;

        /// <summary>
        /// callback after context.getPlayersAsync
        /// </summary>
        public static System.Action<FBPlayerEntry[]> getPlayersAsync_Callback = null;

        [DllImport("__Internal")]
        public static extern void context_chooseAsync(string jsonStr);

        [DllImport("__Internal")]
        public static extern void context_getPlayersAsync();

        [DllImport("__Internal")]
        public static extern string context_getType();

        [DllImport("__Internal")]
        public static extern string context_getID();

        public void chooseAsync(Dictionary<string, object> p, System.Action cb)
        {
            chooseAsync_Callback = cb;
            context_chooseAsync(JsonConvert.SerializeObject(p));
        }

        public void getPlayersAsync(System.Action<FBPlayerEntry[]> cb)
        {
            getPlayersAsync_Callback = cb;
            context_getPlayersAsync();
        }

        public string getType()
        {
            return context_getType();
        }

        public string getID()
        {
            return context_getID();
        }
    }
}