namespace FirebaseAuthentication.Objects
{
    using System;

    [Serializable]
    public class FirebaseError
    {
        public string code;
        public string message;
        public string details;
    }
}
