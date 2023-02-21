namespace Core.AnalyticServices.CommonEvents
{
    using System;
    using Core.AnalyticServices.Data;

    /// <summary>
    /// When users successfully log out with a SDK account
    /// </summary>
    [Serializable]
    internal sealed class LogoutCompleted : IEvent
    {
    }
}