using Core.Entities;
using Core.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductRepository repo) : ControllerBase
    {


        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(string? brand, string? type, string? sort)
        {
            return Ok(await repo.GetProductsAsync(brand,type,sort));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await repo.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            if (product == null)
            {
                return BadRequest("Product cannot be null.");
            }
            repo.AddProduct(product);

            if (!await repo.SaveChangesAsync())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving product to the database.");
            }

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateProduct(int id, Product product)
        {
            if (id != product.Id|| !ProductExists(id))
            {
                return BadRequest("Product ID mismatch.");
            }

            try
            {
                repo.UpdateProduct(product);
                if (!await repo.SaveChangesAsync())
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error updating product in the database.");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }

        private bool ProductExists(int id)
        {
            return repo.ProductExists(id);                                                                                                                              

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            var product = await repo.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            repo.DeleteProduct(product);
            if (!await repo.SaveChangesAsync())
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting product from the database.");
            }
            return NoContent();
        }


        [HttpGet("brands")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetBrands()
        {
            var brands = await repo.GetBrandsAsync();
            if (brands == null || !brands.Any())
            {
                return NotFound("No brands found.");
            }
            return Ok(brands);
        }

        [HttpGet("types")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetTypes()
        {
            var types = await repo.GetTypesAsync();
            if (types == null || !types.Any())
            {
                return NotFound("No types found.");
            }
            return Ok(types);
        }
    }
}
