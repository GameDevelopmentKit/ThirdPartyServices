namespace ServiceImplementation.FBInstant.Player
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.UserData;

    public class HandleFBInstantRemoteUserDataServices : BaseHandleUserDataServices
    {
        private readonly FBInstantPlayerDataWrapper fbInstantPlayer;

        public HandleFBInstantRemoteUserDataServices(ILogService logService, FBInstantPlayerDataWrapper fbInstantPlayer) : base(logService)
        {
            this.fbInstantPlayer = fbInstantPlayer;
        }

        protected override UniTask SaveJsons(params (string key, string json)[] values)
        {
            return this.fbInstantPlayer.SaveUserData(values);
        }

        protected override UniTask<string[]> LoadJsons(params string[] keys)
        {
            return this.fbInstantPlayer.LoadUserData(keys);
        }
    }
}