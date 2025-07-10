namespace FirebaseAuthentication.FirebaseWebGL
{
    using Zenject;

    public class FirebaseAuthenticationInstaller : Installer<FirebaseAuthenticationInstaller>
    {
        public override void InstallBindings()
        {
            this.Container.Bind<IFirebaseAuth>().To<FirebaseAuth>().AsCached().NonLazy();
        }
    }
}