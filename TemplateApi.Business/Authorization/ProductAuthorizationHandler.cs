using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Shared.Authorization.Abstractions;
using Shared.Authorization.Requirements;
using System.Security.Claims;
using TemplateApi.Business.Entities;

namespace TemplateApi.Business.Authorization
{
    public sealed class ProductAuthorizationHandler : AbstractResourceHandler<Product>
    {
        protected override Task<bool> CheckPolicyAsync(
            AuthorizationHandlerContext context,
            OperationAuthorizationRequirement requirement,
            Product resource)
        {
            // Global Bypass: SuperAdmin can do anything
            if (context.User.IsInRole("SuperAdmin"))
                return Task.FromResult(true);

            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // -----------------------------------------------------------------------
            // READ Logic
            // -----------------------------------------------------------------------
            if (requirement.Name == ResourceOperations.Read.Name)
            {
                // Anyone can read public products.
                // FIX: Changed logic to be Secure By Default.
                // Old: !resource.IsDraft.GetValueOrDefault()  <- If null, returns TRUE (Public) [DANGEROUS]
                // New: resource.IsDraft == false              <- If null, returns FALSE (Private) [SAFE]
                if (resource.IsDraft == false)
                {
                    return Task.FromResult(true);
                }

                // Only the creator can read Drafts.
                return Task.FromResult(resource.CreatedBy == userId);
            }

            // -----------------------------------------------------------------------
            // UPDATE / DELETE Logic
            // -----------------------------------------------------------------------
            if (requirement.Name == ResourceOperations.Update.Name ||
                requirement.Name == ResourceOperations.Delete.Name)
            {
                // Only the owner can edit or delete
                return Task.FromResult(resource.CreatedBy == userId);
            }

            // -----------------------------------------------------------------------
            // APPROVE Logic
            // -----------------------------------------------------------------------
            if (requirement.Name == ResourceOperations.Approve.Name)
            {
                // Managers can approve, but they cannot approve their OWN products (Conflict of Interest)
                return Task.FromResult((context.User.IsInRole("Manager") && resource.CreatedBy != userId));
            }

            return Task.FromResult(false);
        }
    }
}