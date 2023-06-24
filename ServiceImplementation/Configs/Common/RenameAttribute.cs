namespace ServiceImplementation.Configs.Common
{
    using UnityEngine;

    public class RenameAttribute : PropertyAttribute
    {
        public string NewName { get ; private set; }

        public RenameAttribute(string name)
        {
            this.NewName = name;
        }
    }
}