namespace ServiceImplementation.FBInstant.Player
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;

    public class FBInstantPlayerDataWrapper : MonoBehaviour
    {
        private readonly Dictionary<string, TaskCompletionSource<string>>           saveUserDataTcs = new();
        private readonly Dictionary<string, TaskCompletionSource<(string, string)>> loadUserDataTcs = new();

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

            FBInstantPlayer.SaveUserData(key, json, this.gameObject.name, nameof(this.OnUserDataSaved), id);

            // var error = Task.Run(() => this.saveUserDataTcs[id].Task).Result;
            var error = Task.Run(() => this.saveUserDataTcs[id].Task).ConfigureAwait(false).GetAwaiter().GetResult();
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

        public string LoadUserData(string key)
        {
            var id = Guid.NewGuid().ToString();
            this.loadUserDataTcs[id] = new();

            FBInstantPlayer.LoadUserData(key, this.gameObject.name, nameof(this.OnUserDataLoaded), id);

            // var (data, error) = Task.Run(() => this.loadUserDataTcs[id].Task).Result;
            var (data, error) = Task.Run(() => this.loadUserDataTcs[id].Task).ConfigureAwait(false).GetAwaiter().GetResult();
            this.loadUserDataTcs.Remove(id);

            if (error is not null)
            {
                throw new(error);
            }

            return data;
        }
    }
}