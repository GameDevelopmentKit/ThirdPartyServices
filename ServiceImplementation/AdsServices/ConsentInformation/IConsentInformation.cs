namespace ServiceImplementation.AdsServices.ConsentInformation
{
    public interface IConsentInformation
    {
        bool CanRequestAds();
        void RequestConsent();
        bool IsRequestingConsent();
    }
}