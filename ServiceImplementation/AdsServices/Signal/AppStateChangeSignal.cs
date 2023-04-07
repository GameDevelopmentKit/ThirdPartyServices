namespace ServiceImplementation.AdsServices.Signal
{
    public class AppStateChangeSignal
    {
        public bool IsBackground { get; set; }

        public AppStateChangeSignal(bool isBackground) { this.IsBackground = isBackground; }
    }
}