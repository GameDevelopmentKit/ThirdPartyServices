namespace FirebaseAuthentication.FirebaseWebGL
{
    using Zenject;

    public class FirebaseAuthenticationInstaller : Installer<FirebaseAuthenticationInstaller>
    {
        public override void InstallBindings()
        {
#if FIREBASE_WEBGL_AUTHENTICATION
            this.Container.Bind<IFirebaseAuth>().To<FirebaseAuth>().AsCached().NonLazy();
#endif
        }
    }
}