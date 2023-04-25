namespace Packages.com.gdk._3rd.Plugins.WebGL
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
