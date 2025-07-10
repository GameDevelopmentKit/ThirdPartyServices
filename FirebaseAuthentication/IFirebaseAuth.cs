namespace FirebaseAuthentication
{
    using Cysharp.Threading.Tasks;
    using FirebaseAuthentication.Objects;

    public interface IFirebaseAuth
    {
        UniTask<FirebaseUser> SignInWithCustomToken(string token);
        UniTask<FirebaseUser> SignInWithFacebook();
        UniTask<FirebaseUser> SignInWithGoogle(string idToken = "", string accessToken = "");
        UniTask               ForgotPassword(string email);
    }
}