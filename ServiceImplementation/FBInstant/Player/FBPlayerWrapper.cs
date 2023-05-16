namespace ServiceImplementation.FBInstant.Player
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Newtonsoft.Json;
    using UnityEngine;

    public class FBPlayerWrapper : MonoBehaviour
    {
        private UniTaskCompletionSource<string>           onUserDataSavedUcs;
        private UniTaskCompletionSource<(string, string)> onUserDataLoadedUcs;

        private void OnUserDataSaved(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var error   = @params["error"];

            this.onUserDataSavedUcs.TrySetResult(error);
        }

        public void SaveUserData(string key, string json)
        {
            this.onUserDataSavedUcs = new();
            FBPlayer.SaveUserData(key, json, this.gameObject.name, nameof(this.OnUserDataSaved));

            async UniTask<string> WaitForResult()
            {
                var (isTimeout, result) = await this.onUserDataSavedUcs.Task.TimeoutWithoutException(TimeSpan.FromSeconds(5));
                return isTimeout ? $"Save user data timeout ({key})" : result;
            }

            var error = WaitForResult().GetAwaiter().GetResult();
            if (error is not null)
            {
                throw new Exception(error);
            }
        }

        private void OnUserDataLoaded(string message)
        {
            var @params = JsonConvert.DeserializeObject<Dictionary<string, string>>(message);
            var data    = @params["data"];
            var error   = @params["error"];

            this.onUserDataLoadedUcs.TrySetResult((data, error));
        }

        public string LoadUserData(string key)
        {
            this.onUserDataLoadedUcs = new();
            FBPlayer.LoadUserData(key, this.gameObject.name, nameof(this.OnUserDataLoaded));

            async UniTask<(string, string)> WaitForResult()
            {
                var (isTimeout, result) = await this.onUserDataLoadedUcs.Task.TimeoutWithoutException(TimeSpan.FromSeconds(5));
                return isTimeout ? (null, $"Load user data timeout ({key})") : result;
            }

            var (data, error) = WaitForResult().GetAwaiter().GetResult();
            if (error is not null)
            {
                throw new Exception(error);
            }

            return data;
        }
    }
}