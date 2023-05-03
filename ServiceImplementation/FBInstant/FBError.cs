namespace ServiceImplementation.FBInstant
{
    public class FBError
    {
        public string code;
        public string message;

        public override string ToString()
        {
            return "FBError: code=" + this.code + ",message=" + this.message;
        }
    }
}