#if FIREBASE_WEBGL_AUTHENTICATION
namespace FirebaseServices.FirebaseWebGL
{
    using System;

    public class FirebaseCallback
    {
        public string         id;
        public bool           once;
        public Action<string> callback;

        public FirebaseCallback()
        {
        }

        public FirebaseCallback(string id, Action<string> callback, bool once = false)
        {
            this.id       = id;
            this.once     = once;
            this.callback = callback;
        }
    }
}
#endif