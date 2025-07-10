namespace FirebaseAuthentication.Objects
{
    using System;

    [Serializable]
    public class FirebaseUser
    {
        public string displayName;

        public string email;

        public bool isAnonymous;

        public bool isEmailVerified;

        public FirebaseUserMetadata metadata;

        public string phoneNumber;

        public FirebaseUserProvider[] providerData;

        public string providerId;

        public string uid;

        public string username;

        public string wallet_address;

        public string firebase_token;

        public FirebaseToken stsTokenManager;
    }

    public class FirebaseToken
    {
        public string apiKey;
        public string refreshToken;
        public string accessToken;
        public double expirationTime;
    }
}