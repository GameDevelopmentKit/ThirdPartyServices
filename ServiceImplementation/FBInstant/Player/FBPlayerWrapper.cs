namespace ServiceImplementation.FBInstant.Player
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;

    public class FBPlayerWrapper : MonoBehaviour
    {
        private readonly Dictionary<string, UniTaskCompletionSource<string>>           saveUserDataTcs = new();
        private readonly Dictionary<string, UniTaskCompletionSource<(string, string)>> loadUserDataTcs = new();

        private void OnUserDataSaved(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var error   = @params["error"];
            var id      = @params["id"];

            this.saveUserDataTcs[id].TrySetResult(error);
        }

        public void SaveUserData(string key, string json)
        {
            var id = Guid.NewGuid().ToString();
            this.saveUserDataTcs[id] = new();
            FBPlayer.SaveUserData(key, json, this.gameObject.name, nameof(this.OnUserDataSaved), id);

            async UniTask<string> WaitForResult()
            {
                var (isTimeout, result) = await this.saveUserDataTcs[id].Task.TimeoutWithoutException(TimeSpan.FromSeconds(5));
                this.saveUserDataTcs.Remove(id);
                return isTimeout ? $"Save user data timeout ({key})" : result;
            }

            var error = WaitForResult().GetAwaiter().GetResult();
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

        public string LoadUserData(string key)
        {
            var id = Guid.NewGuid().ToString();
            this.loadUserDataTcs[id] = new();
            FBPlayer.LoadUserData(key, this.gameObject.name, nameof(this.OnUserDataLoaded), id);

            async UniTask<(string, string)> WaitForResult()
            {
                var (isTimeout, result) = await this.loadUserDataTcs[id].Task.TimeoutWithoutException(TimeSpan.FromSeconds(5));
                this.loadUserDataTcs.Remove(id);
                return isTimeout ? (null, $"Load user data timeout ({key})") : result;
            }

            var (data, error) = WaitForResult().GetAwaiter().GetResult();
            if (error is not null)
            {
                throw new(error);
            }

            return data;
        }
    }
}