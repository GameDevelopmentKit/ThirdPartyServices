namespace ServiceImplementation.IAPServices
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Cysharp.Threading.Tasks;

    /// <summary>
    /// Gate validators decide delivery, by priority, falling back to the next one on a retryable failure.
    /// Audit validators (<see cref="IIapReceiptAuditValidator"/>) never block delivery: they run through <see cref="AuditAsync"/>.
    /// </summary>
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
                .Where(candidate => candidate.IsAvailable && candidate is not IIapReceiptAuditValidator)
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

        public async UniTask<IReadOnlyList<IapReceiptAuditResult>> AuditAsync(IapReceiptValidationRequest request)
        {
            var auditors = this.validators
                .Where(candidate => candidate.IsAvailable && candidate is IIapReceiptAuditValidator)
                .ToList();

            if (auditors.Count == 0)
            {
                return Array.Empty<IapReceiptAuditResult>();
            }

            return await UniTask.WhenAll(auditors.Select(auditor => RunAuditAsync(auditor, request)));
        }

        private static async UniTask<IapReceiptAuditResult> RunAuditAsync(IIapReceiptValidator auditor, IapReceiptValidationRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            IapReceiptValidationResult result;
            try
            {
                result = await auditor.ValidateAsync(request)
                         ?? IapReceiptValidationResult.RetryableFailure(auditor.ProviderId, "Validator returned no result.");
            }
            catch (Exception exception)
            {
                result = IapReceiptValidationResult.RetryableFailure(auditor.ProviderId, exception.Message);
            }

            return new IapReceiptAuditResult(result, stopwatch.ElapsedMilliseconds);
        }
    }
}
