namespace ServiceImplementation.FireBaseRemoteConfig
{
    public class RemoteConfigFetchedSucceededSignal
    {
        public IRemoteConfig RemoteConfig { get; private set; }

        public RemoteConfigFetchedSucceededSignal(IRemoteConfig remoteConfig)
        {
            this.RemoteConfig = remoteConfig;
        }
    }
}