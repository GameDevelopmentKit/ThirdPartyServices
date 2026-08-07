#if APPSFLYER
namespace ServiceImplementation.AppsflyerAnalyticTracker
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using AppsFlyerSDK;
    using Core.AnalyticServices;
    using Cysharp.Threading.Tasks;
    using ServiceImplementation.IAPServices;

    public sealed class AppsflyerPurchaseConnectorReceiptValidator : IIapReceiptValidator, IDisposable
    {
        private const int ValidationTimeoutSeconds = 30;
        private const int CachedResultSeconds      = 60;
        private const string StrongIdentifierPrefix = "id:";
        private const string ProductIdentifierPrefix = "product:";

        private static readonly HashSet<string> ProductIdentifierKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "productId",
            "productIdentifier"
        };

        private static readonly HashSet<string> StrongIdentifierKeys = new(StringComparer.OrdinalIgnoreCase)
        {
            "transactionId",
            "transactionID",
            "transactionIdentifier",
            "originalTransactionId",
            "originalTransactionID",
            "orderId",
            "latestOrderId",
            "purchaseToken",
            "token"
        };

        private readonly object                  syncRoot           = new();
        private readonly List<PendingValidation> pendingValidations = new();
        private readonly List<CachedValidation>  cachedValidations  = new();
        private readonly AnalyticConfig          analyticConfig;

        public string ProviderId => "appsflyer-roi360-purchase-connector";
        public int    Priority   => 100;

        public bool IsAvailable
        {
            get
            {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
                return this.analyticConfig.AppsflyerIsEnableRoi360;
#else
                return false;
#endif
            }
        }

        public AppsflyerPurchaseConnectorReceiptValidator(AnalyticConfig analyticConfig)
        {
            this.analyticConfig = analyticConfig;
            AppsflyerMono.ValidationInfoReceived  += this.HandleValidationInfo;
            AppsflyerMono.ValidationErrorReceived += this.HandleValidationError;
        }

        public void Dispose()
        {
            AppsflyerMono.ValidationInfoReceived  -= this.HandleValidationInfo;
            AppsflyerMono.ValidationErrorReceived -= this.HandleValidationError;
        }

        public async UniTask<IapReceiptValidationResult> ValidateAsync(IapReceiptValidationRequest request)
        {
            if (!this.IsAvailable)
            {
                return IapReceiptValidationResult.RetryableFailure(this.ProviderId,
                    "AppsFlyer ROI360 Purchase Connector is not available.");
            }

            var identifiers = ExtractRequestIdentifiers(request);
            if (identifiers.Count == 0)
            {
                return IapReceiptValidationResult.RetryableFailure(this.ProviderId,
                    "The purchase does not contain an identifier that can be correlated with AppsFlyer.");
            }

            PendingValidation pendingValidation;
            lock (this.syncRoot)
            {
                this.RemoveExpiredCachedValidations();

                var cachedValidation = this.cachedValidations.FirstOrDefault(cached => IdentifiersMatch(cached.Identifiers, identifiers));
                if (cachedValidation != null)
                {
                    this.cachedValidations.Remove(cachedValidation);
                    return cachedValidation.Result;
                }

                pendingValidation = new PendingValidation(identifiers);
                this.pendingValidations.Add(pendingValidation);
            }

            try
            {
                var timeout = UniTask.Delay(TimeSpan.FromSeconds(ValidationTimeoutSeconds), DelayType.Realtime);
                var (hasResult, result) = await UniTask.WhenAny(pendingValidation.Completion.Task, timeout);
                return hasResult
                    ? result
                    : IapReceiptValidationResult.RetryableFailure(this.ProviderId,
                        $"AppsFlyer validation timed out after {ValidationTimeoutSeconds} seconds.");
            }
            finally
            {
                lock (this.syncRoot)
                {
                    this.pendingValidations.Remove(pendingValidation);
                }
            }
        }

        public void HandleValidationInfo(string validationInfo)
        {
            var callbackData = ParseDictionary(validationInfo);
            var identifiers  = ExtractIdentifiers(callbackData);

            IapReceiptValidationResult result;
            if ((TryReadBoolean(callbackData, "success", out var success) && !success) ||
                ContainsNonNullValue(callbackData, "failureData"))
            {
                result = IapReceiptValidationResult.Invalid(this.ProviderId,
                    "AppsFlyer rejected the store purchase.");
            }
            else
            {
                result = IapReceiptValidationResult.Valid(this.ProviderId);
            }

            this.CompleteValidations(identifiers, result);
        }

        public void HandleValidationError(string error)
        {
            var identifiers = ExtractIdentifiers(ParseDictionary(error));
            this.CompleteValidations(identifiers,
                IapReceiptValidationResult.RetryableFailure(this.ProviderId,
                    "AppsFlyer could not validate the purchase."));
        }

        private void CompleteValidations(HashSet<string> identifiers, IapReceiptValidationResult result)
        {
            List<PendingValidation> matches;
            lock (this.syncRoot)
            {
                this.RemoveExpiredCachedValidations();

                if (identifiers.Count > 0)
                {
                    matches = this.pendingValidations
                        .Where(pending => IdentifiersMatch(pending.Identifiers, identifiers))
                        .ToList();
                }
                else if (this.pendingValidations.Count == 1)
                {
                    matches = new List<PendingValidation> { this.pendingValidations[0] };
                }
                else
                {
                    matches = new List<PendingValidation>();
                }

                if (matches.Count == 0 && identifiers.Any(IsStrongIdentifier))
                {
                    this.cachedValidations.Add(new CachedValidation(identifiers, result,
                        DateTime.UtcNow.AddSeconds(CachedResultSeconds)));
                }
            }

            foreach (var match in matches)
            {
                match.Completion.TrySetResult(result);
            }
        }

        private void RemoveExpiredCachedValidations()
        {
            var now = DateTime.UtcNow;
            this.cachedValidations.RemoveAll(cached => cached.ExpiresAt <= now);
        }

        private static HashSet<string> ExtractRequestIdentifiers(IapReceiptValidationRequest request)
        {
            var identifiers = new HashSet<string>(StringComparer.Ordinal);
            AddIdentifier(identifiers, request?.TransactionId, StrongIdentifierPrefix);

            if (request?.Products != null)
            {
                foreach (var product in request.Products)
                {
                    AddIdentifier(identifiers, product.ProductId, ProductIdentifierPrefix);
                }
            }

            if (!string.IsNullOrWhiteSpace(request?.Receipt))
            {
                identifiers.UnionWith(ExtractIdentifiers(ParseDictionary(request.Receipt)));
            }

            return identifiers;
        }

        private static Dictionary<string, object> ParseDictionary(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            try
            {
                return AppsFlyer.CallbackStringToDictionary(json);
            }
            catch
            {
                return null;
            }
        }

        private static HashSet<string> ExtractIdentifiers(object value)
        {
            var identifiers = new HashSet<string>(StringComparer.Ordinal);
            CollectIdentifiers(value, identifiers);
            return identifiers;
        }

        private static void CollectIdentifiers(object value, HashSet<string> identifiers)
        {
            switch (value)
            {
                case IDictionary dictionary:
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        var key = entry.Key?.ToString();
                        if (key != null && StrongIdentifierKeys.Contains(key))
                        {
                            AddIdentifier(identifiers, entry.Value?.ToString(), StrongIdentifierPrefix);
                        }
                        else if (key != null && ProductIdentifierKeys.Contains(key))
                        {
                            AddIdentifier(identifiers, entry.Value?.ToString(), ProductIdentifierPrefix);
                        }

                        CollectIdentifiers(entry.Value, identifiers);
                    }
                    break;
                case IList list:
                    foreach (var item in list)
                    {
                        CollectIdentifiers(item, identifiers);
                    }
                    break;
                case string nestedJson when nestedJson.TrimStart().StartsWith("{"):
                    CollectIdentifiers(ParseDictionary(nestedJson), identifiers);
                    break;
            }
        }

        private static bool TryReadBoolean(object value, string key, out bool result)
        {
            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (string.Equals(entry.Key?.ToString(), key, StringComparison.OrdinalIgnoreCase))
                    {
                        if (entry.Value is bool boolean)
                        {
                            result = boolean;
                            return true;
                        }

                        if (bool.TryParse(entry.Value?.ToString(), out result)) return true;
                    }

                    if (TryReadBoolean(entry.Value, key, out result)) return true;
                }
            }
            else if (value is IList list)
            {
                foreach (var item in list)
                {
                    if (TryReadBoolean(item, key, out result)) return true;
                }
            }

            result = false;
            return false;
        }

        private static bool ContainsNonNullValue(object value, string key)
        {
            if (value is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (string.Equals(entry.Key?.ToString(), key, StringComparison.OrdinalIgnoreCase) && entry.Value != null)
                    {
                        return true;
                    }

                    if (ContainsNonNullValue(entry.Value, key)) return true;
                }
            }
            else if (value is IList list)
            {
                foreach (var item in list)
                {
                    if (ContainsNonNullValue(item, key)) return true;
                }
            }

            return false;
        }

        private static bool IdentifiersMatch(HashSet<string> left, HashSet<string> right)
        {
            var leftStrong  = left.Where(IsStrongIdentifier).ToHashSet(StringComparer.Ordinal);
            var rightStrong = right.Where(IsStrongIdentifier).ToHashSet(StringComparer.Ordinal);

            return leftStrong.Count > 0 && rightStrong.Count > 0
                ? leftStrong.Overlaps(rightStrong)
                : left.Overlaps(right);
        }

        private static bool IsStrongIdentifier(string identifier)
        {
            return identifier.StartsWith(StrongIdentifierPrefix, StringComparison.Ordinal);
        }

        private static void AddIdentifier(HashSet<string> identifiers, string identifier, string prefix)
        {
            if (!string.IsNullOrWhiteSpace(identifier)) identifiers.Add(prefix + identifier.Trim());
        }

        private sealed class PendingValidation
        {
            public HashSet<string> Identifiers { get; }
            public UniTaskCompletionSource<IapReceiptValidationResult> Completion { get; } = new();

            public PendingValidation(HashSet<string> identifiers)
            {
                this.Identifiers = identifiers;
            }
        }

        private sealed class CachedValidation
        {
            public HashSet<string>             Identifiers { get; }
            public IapReceiptValidationResult Result      { get; }
            public DateTime                    ExpiresAt   { get; }

            public CachedValidation(HashSet<string> identifiers, IapReceiptValidationResult result, DateTime expiresAt)
            {
                this.Identifiers = identifiers;
                this.Result      = result;
                this.ExpiresAt   = expiresAt;
            }
        }
    }
}
#endif
