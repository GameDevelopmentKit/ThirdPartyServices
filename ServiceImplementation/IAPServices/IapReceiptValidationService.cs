namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;

    /// <summary>Available validators decide delivery, by priority, falling back to the next one on a retryable failure.
    /// An <see cref="IapReceiptValidationStatus.Invalid"/> result is final.</summary>
    public sealed class IapReceiptValidationService : IIapReceiptValidationService
    {
        private readonly List<IIapReceiptValidator> validators;

        public IapReceiptValidationService(List<IIapReceiptValidator> validators)
        {
            this.validators = validators ?? new List<IIapReceiptValidator>();
        }

        public async UniTask<IapReceiptValidationResult> ValidateAsync(IapReceiptValidationRequest request)
        {
            var gates = this.validators
                .Where(candidate => candidate.IsAvailable)
                .OrderByDescending(candidate => candidate.Priority)
                .ToList();

            if (gates.Count == 0)
            {
                return IapReceiptValidationResult.RetryableFailure(nameof(IapReceiptValidationService), "No receipt validator is available.");
            }

            IapReceiptValidationResult lastRetryable = null;
            foreach (var gate in gates)
            {
                IapReceiptValidationResult result;
                try
                {
                    result = await gate.ValidateAsync(request);
                }
                catch (Exception exception)
                {
                    result = IapReceiptValidationResult.RetryableFailure(gate.ProviderId, exception.Message);
                }

                if (result == null)
                {
                    result = IapReceiptValidationResult.RetryableFailure(gate.ProviderId, "Validator returned no result.");
                }

                if (result.Status != IapReceiptValidationStatus.RetryableFailure)
                {
                    return result;
                }

                lastRetryable = result;
            }

            return lastRetryable;
        }
    }
}
