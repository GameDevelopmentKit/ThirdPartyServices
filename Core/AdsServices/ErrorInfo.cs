namespace Core.AdsServices
{
    public class ErrorInfo
    {
        public int    Code    { get; }
        public string Message { get; }

        public ErrorInfo(int code, string message)
        {
            this.Message = message;
            this.Code    = code;
        }
    }
}