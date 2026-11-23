namespace ServiceImplementation.IAPServices
{
    /// <summary>
    /// Told about each store order once per session, when it is first seen: before it is validated, granted or confirmed.
    /// For SDKs that must read the order while it is still unfinished (e.g. AppsFlyer ROI360 with StoreKit 2 consumables).
    /// Never decides delivery. Must not block or throw. Every implementation is bound by <see cref="IapInstaller"/>.
    /// </summary>
    public interface IIapOrderObserver
    {
        void OnOrderReceived(IapOrder order);
    }
}
