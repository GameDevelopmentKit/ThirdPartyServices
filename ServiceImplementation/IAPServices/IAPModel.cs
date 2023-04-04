namespace ServiceImplementation.IAPServices
{
    using System.Collections.Generic;

    public class IAPModel
    {
        public string    Id         { get; set; }
        public string    PackName   { get; set; }
        public List<int> PackValues { get; set; }

        public string ProductType  { get; set; }
        public string ImageAddress { get; set; }
        public string Name         { get; set; }
        public string Description  { get; set; }
    }
}