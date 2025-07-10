#if FIREBASE_WEBGL_AUTHENTICATION
namespace FirebaseServices.FirebaseWebGL
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using UnityEngine;

    /// <summary>
    /// Listen any event that call from firebase webGL library
    /// </summary>
    public class FirebaseWebGLListener : MonoBehaviour
    {
        #region Singleton

        private static FirebaseWebGLListener instance;

        public static FirebaseWebGLListener Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = FindObjectOfType<FirebaseWebGLListener>();
                if (instance != null) return instance;
                var obj = new GameObject
                {
                    name = nameof(FirebaseWebGLListener)
                };
                instance = obj.AddComponent<FirebaseWebGLListener>();

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;
            DontDestroyOnLoad(this);
        }

        #endregion
        private readonly Dictionary<string, FirebaseCallback> callbacks = new Dictionary<string, FirebaseCallback>();
        
        public static string Add(Action<string> callback, bool once = true)
        {
            var id = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace('+', '@').Replace('/', '$').Substring(0, 16);
            Instance.callbacks.Add(id, new FirebaseCallback(id, callback, once));
            return id;
        }

        public void CallbackHandler(string result)
        {
            var response         = JsonConvert.DeserializeObject<FirebaseCallbackResponse>(result);
            if (!this.callbacks.TryGetValue(response.Id, out var firebaseCallback)) return;
            if (response.Status)
            {
                firebaseCallback.callback(response.Data);
            }
            else
            {
                firebaseCallback.callback(null);
                Debug.LogError($"[FirebaseWebGLListener] [ERROR] - {response.Data}");
            }
            if (!firebaseCallback.once)
                return;
            this.callbacks.Remove(response.Id);
        }
    }
}

#endif