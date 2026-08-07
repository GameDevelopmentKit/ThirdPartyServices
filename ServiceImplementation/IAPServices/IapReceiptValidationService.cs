namespace ServiceImplementation.IAPServices
{
    using System.Collections.Generic;
    using System.Linq;
    using Cysharp.Threading.Tasks;

    public sealed class IapReceiptValidationService : IIapReceiptValidationService
    {
        private readonly List<IIapReceiptValidator> validators;

        public IapReceiptValidationService(List<IIapReceiptValidator> validators)
        {
            this.validators = validators;
        }

        public UniTask<IapReceiptValidationResult> ValidateAsync(IapReceiptValidationRequest request)
        {
            var validator = this.validators
                .Where(candidate => candidate.IsAvailable)
                .OrderByDescending(candidate => candidate.Priority)
                .FirstOrDefault();

            return validator == null
                ? UniTask.FromResult(IapReceiptValidationResult.RetryableFailure(
                    nameof(IapReceiptValidationService), "No receipt validator is available."))
                : validator.ValidateAsync(request);
        }
    }
}
