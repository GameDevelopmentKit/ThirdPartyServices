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

        protected override UniTask SaveJson(string key, string json)
        {
            return this.fbInstantPlayer.SaveUserData(key, json);
        }

        protected override UniTask<string> LoadJson(string key)
        {
            return this.fbInstantPlayer.LoadUserData(key);
        }
    }
}