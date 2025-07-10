#if FIREBASE_WEBGL_AUTHENTICATION
namespace FirebaseServices.FirebaseWebGL
{
    using System;
    using Newtonsoft.Json;

    [Serializable]
    public class FirebaseCallbackResponse
    {
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("status")]
        public bool Status;
        [JsonProperty("data")]
        public string Data;
    }
}
#endif