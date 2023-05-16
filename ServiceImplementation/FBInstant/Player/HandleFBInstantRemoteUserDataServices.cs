namespace ServiceImplementation.FBInstant.Player
{
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.UserData;

    public class HandleFBInstantRemoteUserDataServices : BaseHandleUserDataServices
    {
        private readonly FBInstantPlayerDataWrapper fbInstantPlayer;

        public HandleFBInstantRemoteUserDataServices(ILogService logService, FBInstantPlayerDataWrapper fbInstantPlayer) : base(logService)
        {
            this.fbInstantPlayer = fbInstantPlayer;
        }

        protected override void SaveJson(string key, string json)
        {
            this.fbInstantPlayer.SaveUserData(key, json);
        }

        protected override string LoadJson(string key)
        {
            return this.fbInstantPlayer.LoadUserData(key);
        }
    }
}