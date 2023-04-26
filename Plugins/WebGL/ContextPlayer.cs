namespace Plugins.WebGL
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using Newtonsoft.Json;

    public class ContextPlayer
    {

        /// <summary>
        /// callback after get data
        /// </summary>
        public static System.Action<Dictionary<string, object>> getDataAsync_Callback = null;

        /// <summary>
        /// callback after set data
        /// </summary>
        public static System.Action setDataAsync_Callback = null;

        public static System.Action<bool> canSubscribeBotAsync_Callback = null;

        public static System.Action subscribeBot_Success_Callback = null;
        public static System.Action<FBError> subscribeBot_Error_Callback = null;

        [DllImport("__Internal")]
        public static extern string player_getID();

        [DllImport("__Internal")]
        public static extern string player_getName();

        [DllImport("__Internal")]
        public static extern string player_getPhoto();

        [DllImport("__Internal")]
        public static extern void player_getDataAsync(string keysJsonStr);

        [DllImport("__Internal")]
        public static extern void player_setDataAsync(string gameDataJsonStr);

        [DllImport("__Internal")]
        public static extern void player_canSubscribeBotAsync();

        [DllImport("__Internal")]
        public static extern void player_subscribeBotAsync();


        public string getID()
        {
            return player_getID();
        }

        public string getName()
        {
            return player_getName();
        }

        public string getPhoto()
        {
            return player_getPhoto();
        }

        public void getDataAsync(List<string> keys, System.Action<Dictionary<string, object>> cb)
        {
            getDataAsync_Callback = cb;
            player_getDataAsync(JsonConvert.SerializeObject(keys));
        }

        /// <summary>
        /// </summary>
        /// <param name="gameData"></param>
        public void setDataAsync(Dictionary<string, object> gameData, System.Action cb)
        {
            setDataAsync_Callback = cb;
            player_setDataAsync(JsonConvert.SerializeObject(gameData));
        }

        public void canSubscribeBotAsync(System.Action<bool> cb)
        {
            canSubscribeBotAsync_Callback = cb;
            player_canSubscribeBotAsync();
        }

        public void subscribeBotAsync(System.Action successCallback, System.Action<FBError> errorCallback)
        {
            subscribeBot_Success_Callback = successCallback;
            subscribeBot_Error_Callback = errorCallback;
            player_subscribeBotAsync();
        }
    }

}
