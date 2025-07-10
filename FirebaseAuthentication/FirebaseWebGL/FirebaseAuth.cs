namespace FirebaseAuthentication.FirebaseWebGL
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using FirebaseAuthentication;
    using FirebaseAuthentication.Objects;
    using FirebaseServices.FirebaseWebGL;
    using Newtonsoft.Json;
    using UnityEngine;

    public class FirebaseAuth:IFirebaseAuth
    {
        public void AuthChanged(Action<FirebaseUser> callback) =>
            AuthChangedWeb(FirebaseWebGLListener.Add((res => { callback(res == "NULL" ? null : JsonConvert.DeserializeObject<FirebaseUser>(res)); }), false));


        public async UniTask<FirebaseUser> SignInWithCustomToken(string token)
        {
            string response = null;
            SignInWithCustomTokenWeb(token, FirebaseWebGLListener.Add(res => response = res));
            await UniTask.WaitUntil(() => response != null);
            Debug.Log($"1111 Check firebase login response: " + response);
            return response == "NULL" ? null : JsonConvert.DeserializeObject<FirebaseUser>(response);
        }

        public async UniTask<FirebaseUser> SignInWithEmailAndPassword(string email, string password)
        {
            string response = null;
            SignInWithEmailAndPasswordWeb(email, password, FirebaseWebGLListener.Add(res => response = res));
            await UniTask.WaitUntil(() => response != null);
            Debug.Log($"2222 Check firebase login response: " + response);
            return response == "NULL" ? null : JsonConvert.DeserializeObject<FirebaseUser>(response);
        }

        public async UniTask<FirebaseUser> SignInWithGoogle(string idToken = "", string acessToken = "")
        {
            string response = null;
            SignInWithGoogleWeb(FirebaseWebGLListener.Add(res => response = res));
            await UniTask.WaitUntil(() => response != null);
            return response == "NULL" ? null : JsonConvert.DeserializeObject<FirebaseUser>(response);
        }

        public UniTask ForgotPassword(string email)
        {
            ResetPasswordWeb(email);
            return UniTask.CompletedTask;
        }

        public async UniTask<FirebaseUser> SignInWithFacebook()
        {
            string response = null;
            SignInWithFacebookWeb(FirebaseWebGLListener.Add(res => response = res));
            await UniTask.WaitUntil(() => response != null);
            return response == "NULL" ? null : JsonConvert.DeserializeObject<FirebaseUser>(response);
        }

        [DllImport("__Internal")]
        private static extern string AuthChangedWeb(string cid);

        [DllImport("__Internal")]
        private static extern string SignInWithCustomTokenWeb(string token, string cid);

        [DllImport("__Internal")]
        private static extern string SignInWithEmailAndPasswordWeb(string email, string password, string cid);

        [DllImport("__Internal")]
        private static extern string SignInWithGoogleWeb(string cid);

        [DllImport("__Internal")]
        private static extern string SignInWithFacebookWeb(string cid);
        [DllImport("__Internal")]
        private static extern string ResetPasswordWeb(string email);
    }
}