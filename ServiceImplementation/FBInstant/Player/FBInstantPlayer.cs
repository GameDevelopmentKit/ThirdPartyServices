namespace ServiceImplementation.FBInstant.Player
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;

    public class FBInstantPlayer : MonoBehaviour
    {
        private readonly Dictionary<string, UniTaskCompletionSource<string>>           saveUserDataTcs = new();
        private readonly Dictionary<string, UniTaskCompletionSource<(string, string)>> loadUserDataTcs = new();

        public string GetUserId()
        {
            return FBInstantPlayerLibrary.GetUserId();
        }

        public string GetUserName()
        {
            return FBInstantPlayerLibrary.GetUserName();
        }

        public string GetUserAvatar()
        {
            return FBInstantPlayerLibrary.GetUserAvatar();
        }

        private void OnUserDataSaved(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var error   = @params["error"];
            var id      = @params["id"];

            this.saveUserDataTcs[id].TrySetResult(error);
        }

        public async UniTask SaveUserData((string key, string json)[] values)
        {
            var id = Guid.NewGuid().ToString();
            this.saveUserDataTcs[id] = new();

            FBInstantPlayerLibrary.SaveUserData(JsonConvert.SerializeObject(values.ToDictionary(value => value.key, value => value.json)), this.gameObject.name, nameof(this.OnUserDataSaved), id);

            var error = await this.saveUserDataTcs[id].Task;
            this.saveUserDataTcs.Remove(id);

            if (error is not null)
            {
                throw new(error);
            }
        }

        private void OnUserDataLoaded(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var data    = @params["data"];
            var error   = @params["error"];
            var id      = @params["id"];

            this.loadUserDataTcs[id].TrySetResult((data, error));
        }

        public async UniTask<string[]> LoadUserData(string[] keys)
        {
            var id = Guid.NewGuid().ToString();
            this.loadUserDataTcs[id] = new();

            FBInstantPlayerLibrary.LoadUserData(JsonConvert.SerializeObject(keys), this.gameObject.name, nameof(this.OnUserDataLoaded), id);

            var (data, error) = await this.loadUserDataTcs[id].Task;
            this.loadUserDataTcs.Remove(id);

            if (error is not null)
            {
                throw new(error);
            }

            return JsonConvert.DeserializeObject<string[]>(data);
        }
    }
}