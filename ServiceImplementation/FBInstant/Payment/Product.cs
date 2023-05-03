namespace ServiceImplementation.FBInstant.Payment
{
    /// <summary>
    /// Represents a game's product information.
    /// </summary>
    [System.Serializable]
    public class Product
    {
        /// <summary>
        /// The title of the product
        /// </summary>
        public string title;

        /// <summary>
        /// The product's game-specified identifier
        /// </summary>
        public string productID;

        /// <summary>
        /// The product description
        /// </summary>
        public string description = "";

        /// <summary>
        /// A link to the product's associated image
        /// </summary>
        public string imageURI = "";

        /// <summary>
        /// The price of the product
        /// </summary>
        public string price;

        /// <summary>
        /// The currency code for the product
        /// </summary>
        public string priceCurrencyCode;

        public override string ToString()
        {
            return "Product: title=" + this.title + ",productID=" + this.productID + ",price=" + this.price + ",imageURI=" + this.imageURI + ",description=" + this.description;

        }
    }
}