namespace FirebaseAuthentication.FirebaseApp
{
    using Zenject;

    public class FirebaseAuthenticationInstaller : Installer<FirebaseAuthenticationInstaller>
    {
        public override void InstallBindings()
        {
#if FIREBASE_MOBILE_AUTHENTICATION
            this.Container.Bind<IFirebaseAuth>().To<FirebaseAuth>().AsCached().NonLazy();
#endif

        }
    }
}