using Microsoft.AspNetCore.Identity;

namespace Shared.Identity.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Add profile data here that is strictly related to Identity.
        // Avoid adding navigation properties to Business Domains (like Orders) 
        // to maintain the strict modular separation.

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Example: Storing a refresh token for JWT flows
        public string? RefreshToken { get; set; }
        public DateTimeOffset? RefreshTokenExpiryTime { get; set; }
    }
}