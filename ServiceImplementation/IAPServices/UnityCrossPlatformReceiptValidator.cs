namespace ServiceImplementation.IAPServices
{
    using System;
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Purchasing.Security;

    public sealed class UnityIapReceiptValidationKeys
    {
        public byte[] GooglePlayPublicKey { get; }
        public byte[] AppleRootCertificate { get; }

        public UnityIapReceiptValidationKeys(byte[] googlePlayPublicKey = null, byte[] appleRootCertificate = null)
        {
            this.GooglePlayPublicKey   = googlePlayPublicKey;
            this.AppleRootCertificate = appleRootCertificate;
        }
    }

    public sealed class UnityCrossPlatformReceiptValidator : IIapReceiptValidator
    {
        private const string FakeStoreName       = "fake";
        private const string GooglePlayStoreName = "GooglePlay";
        private const string AppleAppStoreName   = "AppleAppStore";
        private const string MacAppStoreName     = "MacAppStore";

        private readonly CrossPlatformValidator validator;

        public string ProviderId  => "unity-cross-platform";
        public int    Priority    => 0;
        public bool   IsAvailable => true;

        [Serializable]
        private sealed class UnityReceiptEnvelope
        {
            public string Store;
            public string TransactionID;
            public string Payload;
        }

        public UnityCrossPlatformReceiptValidator(UnityIapReceiptValidationKeys keys)
        {
            try
            {
                if (keys?.GooglePlayPublicKey == null)
                {
                    Debug.LogWarning("[Unity IAP] CrossPlatformValidator was not initialized because validation keys are missing.");
                    return;
                }

                this.validator = keys.AppleRootCertificate != null
                    ? new CrossPlatformValidator(keys.GooglePlayPublicKey, keys.AppleRootCertificate, Application.identifier)
                    : new CrossPlatformValidator(keys.GooglePlayPublicKey, Application.identifier);
                Debug.Log("[Unity IAP] CrossPlatformValidator initialized successfully.");
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[Unity IAP] CrossPlatformValidator initialization failed: {exception.Message}");
            }
        }

        public UniTask<IapReceiptValidationResult> ValidateAsync(IapReceiptValidationRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Receipt))
            {
                return UniTask.FromResult(IapReceiptValidationResult.Invalid(this.ProviderId, "Receipt is empty."));
            }

            if (!this.ValidateReceiptStore(request.Receipt, out var handledByStoreValidation, out var storeError))
            {
                return UniTask.FromResult(IapReceiptValidationResult.Invalid(this.ProviderId, storeError));
            }

            if (handledByStoreValidation)
            {
                return UniTask.FromResult(IapReceiptValidationResult.Valid(this.ProviderId));
            }

            if (this.validator == null)
            {
                return UniTask.FromResult(IapReceiptValidationResult.RetryableFailure(
                    this.ProviderId, "CrossPlatformValidator is unavailable."));
            }

            try
            {
                this.validator.Validate(request.Receipt);
                return UniTask.FromResult(IapReceiptValidationResult.Valid(this.ProviderId));
            }
            catch (IAPSecurityException exception)
            {
                return UniTask.FromResult(IapReceiptValidationResult.Invalid(this.ProviderId, exception.Message));
            }
            catch (Exception exception)
            {
                return UniTask.FromResult(IapReceiptValidationResult.RetryableFailure(this.ProviderId, exception.Message));
            }
        }

        private bool ValidateReceiptStore(string receipt, out bool handledByStoreValidation, out string error)
        {
            handledByStoreValidation = false;
            error                    = null;

            UnityReceiptEnvelope receiptEnvelope;
            try
            {
                receiptEnvelope = JsonUtility.FromJson<UnityReceiptEnvelope>(receipt);
            }
            catch (Exception exception)
            {
                error = $"Receipt envelope parsing failed: {exception.Message}";
                return false;
            }

            if (receiptEnvelope == null || string.IsNullOrWhiteSpace(receiptEnvelope.Store))
            {
                error = "Receipt envelope is malformed.";
                return false;
            }

            if (string.Equals(receiptEnvelope.Store, FakeStoreName, StringComparison.OrdinalIgnoreCase))
            {
#if UNITY_EDITOR || DEBUG_MODULE
                handledByStoreValidation = true;
                return true;
#else
                error = "FakeStore receipt was rejected outside a debug build.";
                return false;
#endif
            }

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!string.Equals(receiptEnvelope.Store, GooglePlayStoreName, StringComparison.OrdinalIgnoreCase))
            {
                error = $"Unexpected Android receipt store '{receiptEnvelope.Store}'.";
                return false;
            }
#elif UNITY_IOS && !UNITY_EDITOR
            if (!string.Equals(receiptEnvelope.Store, AppleAppStoreName, StringComparison.OrdinalIgnoreCase))
            {
                error = $"Unexpected iOS receipt store '{receiptEnvelope.Store}'.";
                return false;
            }
#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            if (!string.Equals(receiptEnvelope.Store, MacAppStoreName, StringComparison.OrdinalIgnoreCase))
            {
                error = $"Unexpected macOS receipt store '{receiptEnvelope.Store}'.";
                return false;
            }
#endif

            return true;
        }
    }
}
