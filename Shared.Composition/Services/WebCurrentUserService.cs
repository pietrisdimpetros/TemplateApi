using Microsoft.AspNetCore.Http;
using Shared.Data.Abstractions;
using System.Security.Claims;

namespace Shared.Composition.Services
{
    /// <summary>
    /// An adapter that bridges the gap between the HTTP Context (Presentation Layer)
    /// and the Data Layer's auditing needs, without linking the two projects directly.
    /// </summary>
    internal sealed class WebCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
    {
        public string? EntraId => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? UserName => httpContextAccessor.HttpContext?.User?.Identity?.Name;
    }
}