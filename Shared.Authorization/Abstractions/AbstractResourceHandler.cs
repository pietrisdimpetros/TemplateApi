namespace Shared.Authorization.Abstractions
{
    /// <summary>
    /// A strongly-typed base handler that enforces authentication before evaluating logic.
    /// </summary>
    /// <typeparam name="TResource">The type of the entity (e.g., Product)</typeparam>
    public abstract class AbstractResourceHandler<TResource>
      : AuthorizationHandler<OperationAuthorizationRequirement, TResource>
    {
        protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        TResource resource)
        {
            if (context.User.Identity?.IsAuthenticated != true) return;

            if (await CheckPolicyAsync(context, requirement, resource))
                context.Succeed(requirement);
        }

        /// <summary>
        /// Evaluates if the current user meets the requirement for the specific resource.
        /// </summary>
        protected abstract Task<bool> CheckPolicyAsync(
         AuthorizationHandlerContext context,
         OperationAuthorizationRequirement requirement,
         TResource resource);
    }
}