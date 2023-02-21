namespace AnalyticServices.Data
{
    using Utilities.Extension;

    /// <summary>
    /// 
    /// </summary>
    public static class EventExtension
    {
        // todo - this should be able to be handled at compile time
        public static string ToSnakeCase(this IEvent trackedEvent)
            => trackedEvent.GetType().Name.ToSnakeCase();
    }
}