namespace ServiceImplementation.FirebaseAnalyticTracker
{
    using System.Text.RegularExpressions;

    /// <summary>
    /// Methods for checking names on events and parameters
    /// </summary>
    public static class FirebaseExtension
    {
        private static readonly Regex Reg = new Regex("^[a-zA-Z0-9_]+$");

        /// <summary>
        /// Based on the firebase analytics names requirements
        /// https://firebase.google.com/docs/reference/cpp/group/event-names
        /// </summary>
        /// <param name="str"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static bool IsNameValid(this string str, out string error)
        {
            error = null;
            if (str == null)
            {
                error = "Name is null";
                return false;
            }

            if (str.Length > 40)
                error = "Name too long";

            if (!char.IsLetter(str[0]))
                error += "\nName must start with a letter";

            if (!Reg.IsMatch(str))
                error += "\nName contains invalid characters";

            if (str.StartsWith("firebase_") || str.StartsWith("google_") || str.StartsWith("ga_"))
                error += "\nName starts with reserved prefix from google";

            return string.IsNullOrEmpty(error);
        }

        /// <summary>
        /// Based on the firebase analytics parameter values requirments
        /// https://firebase.google.com/docs/reference/cpp/group/parameter-names
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsParameterValueValid(this object obj, out string error)
        {
            error = null;
            if (obj == null)
            {
                error = "Parameter is null";
                return false;
            }

            string str = obj.ToString();
            if (str.Length <= 100) return true;

            error = "Parameter too long";
            return false;
        }
    }
}
