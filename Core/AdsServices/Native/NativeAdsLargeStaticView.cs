namespace Core.AdsServices.Native
{
    using System.Collections.Generic;
    using GoogleMobileAds.Api;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class NativeAdsLargeStaticView : NativeAdsView
    {
        public TMP_Text advertiserText;
        public TMP_Text bodyText;
        public TMP_Text storeText;
        public TMP_Text priceText;
        public Image    adImage;

        public override void BindData(NativeAd nativeAd)
        {
            base.BindData(nativeAd);

            if (nativeAd.GetAdvertiserText() != null)
            {
                this.logService.Log($"native advertiser text: {nativeAd.GetAdvertiserText()}");
                
                this.advertiserText.text = nativeAd.GetAdvertiserText();

                if (!nativeAd.RegisterAdvertiserTextGameObject(this.advertiserText.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Log($"Failed to register advertiser text for native ad: {this.name}");
                }
            }

            if (nativeAd.GetBodyText() != null)
            {
                this.logService.Log($"native body text: {nativeAd.GetBodyText()}");
                
                this.bodyText.text = nativeAd.GetBodyText();
                if (!nativeAd.RegisterBodyTextGameObject(this.bodyText.gameObject))
                {
                    // Handle failure to register ad asset.
                    this.logService.Log($"Failed to register body text for native ad: {this.name}");
                }
            }

            if (nativeAd.GetStore() != null)
            {
                this.logService.Log($"native store: {nativeAd.GetStore()}");

                this.storeText.text = nativeAd.GetStore();
                if (!nativeAd.RegisterStoreGameObject(this.storeText.gameObject))
                {
                    // Handle failure to register ad asset
                    this.logService.Log($"Failed to register store for native ad: {this.name}");
                }
            }

            if (nativeAd.GetPrice() != null)
            {
                this.logService.Log($"native Price: {nativeAd.GetPrice()}");
                
                this.priceText.text = nativeAd.GetPrice();
                if (!nativeAd.RegisterPriceGameObject(this.priceText.gameObject))
                {
                    // Handle failure to register ad asset
                    this.logService.Log($"Failed to register price for native ad: {this.name}");
                }
            }

            if (nativeAd.GetImageTextures() != null)
            {
                var adTextureImage = nativeAd.GetImageTextures()[0];
                this.adImage.sprite = Sprite.Create(adTextureImage, new Rect(0, 0, adTextureImage.width, adTextureImage.height), new Vector2(0.5f, 0.5f), 100);
                var images = new List<GameObject> { this.adImage.gameObject };
                if (nativeAd.RegisterImageGameObjects(images) <= 0)
                {
                    // Handle failure to register ad asset
                    this.logService.Log($"Failed to register images for native ad: {this.name}");
                }   
            }
        }
    }
}