using PullRequests_Review_Assistant.Domain.Enums;
using PullRequests_Review_Assistant.Domain.ValueObjects;

namespace PullRequests_Review_Assistant.Domain.Interfaces
{
    /// <summary>
    /// Provides LLM model configuration based on the user's subscription tier.
    /// Supports fallback when the preferred model is unavailable.
    /// </summary>
    public interface IModelConfigProvider
    {
        /// <summary>
        /// Returns the preferred model (with fallback) for the given tier.
        /// </summary>
        /// 
        /// <param name="tier">The subscription tier to get the model for.</param>
        /// 
        /// <returns>
        /// The preferred model (with fallback) for the given tier.
        /// </returns>
        ModelPreference GetPreferredModel(SubscriptionTier tier);

        /// <summary>
        /// Attempts to resolve a usable model, falling back if needed.
        /// </summary>
        /// 
        /// <param name="tier">The subscription tier to get the model for.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// 
        /// <returns>
        /// String identifier of the resolved model.
        /// </returns>
        Task<string> ResolveModelAsync(SubscriptionTier tier, CancellationToken cancellationToken = default);
    }
}