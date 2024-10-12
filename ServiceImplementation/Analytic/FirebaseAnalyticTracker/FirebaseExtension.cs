namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Methods for checking names on events and parameters
    /// </summary>
    public static class FirebaseExtension
    {
        /// <summary>
        /// Based on the firebase analytics names requirements
        /// https://firebase.google.com/docs/reference/cpp/group/event-names
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string IsNameValid(this string str)
        {
            var reg = new Regex("^[a-zA-Z0-9_]+$");

            if (str == null) return "Name is null";

            if (str.Length > 40) return "Name is too long";

            if (!char.IsLetter(str[0])) return "Name does not start with letter";

            if (!reg.IsMatch(str)) return "Name contains special characters";

            if (str.Equals("firebase_") || str.Equals("google_") || str.Equals("ga_")) return "Name is reserved from google";

            return "Valid";
        }

        /// <summary>
        /// Based on the firebase analytics parameter values requirments
        /// https://firebase.google.com/docs/reference/cpp/group/parameter-names
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string IsParameterValueValid(this object obj)
        {
            if (obj == null) return "Valid";

            var str = obj.ToString();
            return str.Length <= 100 ? "Valid" : "Parameter too long";
        }
    }
}