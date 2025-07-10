namespace FirebaseAuthentication.FirebaseApp
{
    using Cysharp.Threading.Tasks;
    using FirebaseAuthentication;
    using FirebaseAuthentication.Objects;

    public class FirebaseAuth : IFirebaseAuth
    {
        public async UniTask<FirebaseUser> SignInWithCustomToken(string token)
        {
            var firebaseUser  = (await Firebase.Auth.FirebaseAuth.DefaultInstance.SignInWithCustomTokenAsync(token)).User;
            var firebaseToken = await firebaseUser.TokenAsync(false);

            var model = new FirebaseUser()
            {
                uid             = firebaseUser.UserId,
                email           = firebaseUser.Email,
                username        = firebaseUser.DisplayName,
                phoneNumber     = firebaseUser.PhoneNumber,
                isEmailVerified = firebaseUser.IsEmailVerified,
                isAnonymous     = firebaseUser.IsAnonymous,
                firebase_token  = firebaseToken,
                stsTokenManager = new FirebaseToken()
                {
                    accessToken    = firebaseToken,
                    apiKey         = Firebase.FirebaseApp.DefaultInstance.Options.ApiKey,
                    expirationTime = firebaseUser.Metadata.LastSignInTimestamp,
                    refreshToken   = firebaseToken
                }
            };

            return model;
        }

        public UniTask<FirebaseUser> SignInWithFacebook() { return default; }

        public async UniTask<FirebaseUser> SignInWithGoogle(string idToken = "", string acessToken = "")
        {
            var                      auth   = Firebase.Auth.FirebaseAuth.DefaultInstance;
            FirebaseUser             user   = null;
            Firebase.Auth.AuthResult result = null;

            var credential =
                Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, acessToken);

            var isComplete = false;
            await auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    isComplete = true;
                    return;
                }

                if (task.IsFaulted)
                {
                    isComplete = true;
                    return;
                }
                isComplete = true;
                result = task.Result;
            });

            await UniTask.WaitUntil(() => isComplete);

            if (result is not { User: { } }) return null;
            var firebaseUser  = result.User;
            var firebaseToken = await firebaseUser.TokenAsync(false);

            user = new FirebaseUser()
            {
                uid             = firebaseUser.UserId,
                email           = firebaseUser.Email,
                username        = firebaseUser.DisplayName,
                phoneNumber     = firebaseUser.PhoneNumber,
                isEmailVerified = firebaseUser.IsEmailVerified,
                isAnonymous     = firebaseUser.IsAnonymous,
                firebase_token  = firebaseToken,
                stsTokenManager = new FirebaseToken()
                {
                    accessToken    = firebaseToken,
                    apiKey         = Firebase.FirebaseApp.DefaultInstance.Options.ApiKey,
                    expirationTime = firebaseUser.Metadata.LastSignInTimestamp,
                    refreshToken   = firebaseToken
                }
            };

            return user;
        }

        public UniTask ForgotPassword(string email)
        {
            var auth= Firebase.Auth.FirebaseAuth.DefaultInstance;
            return auth.SendPasswordResetEmailAsync(email).AsUniTask();
        }
    }
}