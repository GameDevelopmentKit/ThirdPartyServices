namespace Core.AnalyticServices.CommonEvents
{
    using System;
    using Core.AnalyticServices.Data;

    /// <summary>
    /// When users successfully login with a SDK account
    /// </summary>
    [Serializable]
    internal sealed class LoginCompleted : IEvent
    {
    }
}