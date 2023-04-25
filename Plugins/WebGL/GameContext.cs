﻿namespace Packages.com.gdk._3rd.Plugins.WebGL
{
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class GameContext
    {

        /// <summary>
        /// callback after confirmed choose context
        /// </summary>
        public static System.Action chooseAsync_Callback = null;

        /// <summary>
        /// callback after context.getPlayersAsync
        /// </summary>
        public static System.Action<ContextPlayerEntry[]> getPlayersAsync_Callback = null;

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
            context_chooseAsync(/*SimpleJson.SimpleJson.SerializeObject(p)*/"");
        }

        public void getPlayersAsync(System.Action<ContextPlayerEntry[]> cb)
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
