using Microsoft.AspNetCore.Mvc;
using Shared.Idempotency.Filters;

namespace Shared.Idempotency.Attributes
{
    /// <summary>
    /// Marks an action as Idempotent.
    /// This acts as a Service Filter, instantiating the IdempotencyFilter 
    /// only for this specific request pipeline.
    /// </summary>
    public class IdempotentAttribute : TypeFilterAttribute
    {
        public IdempotentAttribute() : base(typeof(IdempotencyFilter))
        {
            // Arguments for the filter constructor can be passed here if needed,
            // but we rely on DI for the Filter's dependencies.
        }
    }
}