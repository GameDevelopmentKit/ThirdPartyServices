namespace ServiceImplementation.FBInstant.Player
{
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Utilities.LogService;
    using GameFoundation.Scripts.Utilities.UserData;
    using Newtonsoft.Json;

    public class HandleFBInstantRemoteUserDataServices : BaseHandleUserDataServices
    {
        private readonly FBInstantPlayerDataWrapper fbInstantPlayer;

        public HandleFBInstantRemoteUserDataServices(ILogService logService, FBInstantPlayerDataWrapper fbInstantPlayer) : base(logService)
        {
            this.fbInstantPlayer = fbInstantPlayer;
        }

        protected override UniTask SaveJsons(params (string key, string json)[] values)
        {
            return this.fbInstantPlayer.SaveUserData(JsonConvert.SerializeObject(values.ToDictionary(value => value.key, value => value.json)));
        }

        protected override UniTask<string[]> LoadJsons(params string[] keys)
        {
            return this.fbInstantPlayer.LoadUserData(keys);
        }
    }
}