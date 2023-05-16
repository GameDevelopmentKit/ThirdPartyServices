namespace ServiceImplementation.FBInstant.Player
{
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.UserData;

    public class HandleFBRemoteUserDataServices : BaseHandleUserDataServices
    {
        private readonly FBPlayerWrapper fbPlayer;

        public HandleFBRemoteUserDataServices(ILogService logService, FBPlayerWrapper fbPlayer) : base(logService)
        {
            this.fbPlayer = fbPlayer;
        }

        protected override void SaveJson(string key, string json)
        {
            this.fbPlayer.SaveUserData(key, json);
        }

        protected override string LoadJson(string key)
        {
            return this.fbPlayer.LoadUserData(key);
        }
    }
}