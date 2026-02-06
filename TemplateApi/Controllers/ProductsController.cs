using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Authorization.Requirements;
using TemplateApi.Business.Data;

namespace TemplateApi.Controllers
{
    public record UpdateProductDto(string Name);

    [ApiController]
    [Route("api/[controller]")]
    public sealed class ProductsController(
     IAuthorizationService authService,
     CatalogDbContext dbContext) : ControllerBase
    {
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
        {
            // 1. Fetch the Resource
            // (In a real app, use a Service/CQRS, but this demonstrates the point)
            var product = await dbContext!.Products!.FindAsync(id);
            if (product is null)
            {
                return NotFound();
            }

            // 2. The "Ask"
            // "Can the current User perform 'Update' on THIS specific 'Product'?"
            var authResult = await authService.AuthorizeAsync(User, product, ResourceOperations.Update);

            if (!authResult.Succeeded)
            {
                // Return 403 Forbidden (I know who you are, but you can't do this)
                // vs 401 Unauthorized (I don't know who you are)
                return Forbid();
            }

            // 3. The Action (Safe to proceed)
            product.Name = dto.Name;
            await dbContext.SaveChangesAsync();

            return Ok(product);
        }
    }
}
