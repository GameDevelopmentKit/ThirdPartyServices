namespace ServiceImplementation.FBInstant.Payment
{
    /// <summary>
    /// The configuration of a purchase request for a product registered to the game.
    /// </summary>
    [System.Serializable]
    public class PurchaseConfig
    {

        /// <summary>
        /// The identifier of the product to purchase
        /// </summary>
        public string productID;

        /// <summary>
        /// An optional developer-specified payload, to be included in the returned purchase's signed request.
        /// </summary>
        public string developerPayload = "";

        public override string ToString()
        {
            return "PurchaseConfig: productID=" + this.productID + ",developerPayload=" + this.developerPayload;
        }

    }

}
