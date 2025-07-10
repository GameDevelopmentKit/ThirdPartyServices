namespace FirebaseAuthentication.Objects
{
    using System;

    [Serializable]
    public class FirebaseUserMetadata
    {
        public ulong lastSignInTimestamp;

        public ulong creationTimestamp;
    }
}
