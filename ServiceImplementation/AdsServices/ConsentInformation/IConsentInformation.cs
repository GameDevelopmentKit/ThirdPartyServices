namespace ServiceImplementation.AdsServices.ConsentInformation
{
    public interface IConsentInformation
    {
        void Request();
        bool IsComplete { get; set; }
    }
}