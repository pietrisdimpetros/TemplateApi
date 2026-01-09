using Shared.Data.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace TemplateApi.Business.Entities
{
    public class Product : IAuditableEntity, ISoftDelete
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        public decimal Price { get; set; }

        // --- Audit Fields (Handled by AuditingInterceptor) ---
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }

        // --- Soft Delete Fields (Handled by SoftDeleteInterceptor) ---
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}