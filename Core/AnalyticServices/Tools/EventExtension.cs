namespace Core.AnalyticServices.Tools
{
    using System.Linq;
    using Core.AnalyticServices.Data;

    /// <summary>
    ///
    /// </summary>
    public static class EventExtension
    {
        // todo - this should be able to be handled at compile time
        public static string ToSnakeCase(this IEvent trackedEvent)
        {
            return trackedEvent.GetType().Name.ToSnakeCase();
        }

        /// <summary>
        /// convert a string to snake_case
        /// </summary>
        /// <param name="str"></param>
        /// <returns>the string in snake case</returns>
        /// <remarks>from https://www.30secondsofcode.org/c-sharp/s/to-snake-case</remarks>
        public static string ToSnakeCase(this string str)
        {
            return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x)
                ? "_" + x.ToString()
                : x.ToString())).ToLower();
        }
    }
}