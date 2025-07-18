using Core.Entities;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class ProductRepository(StoreContext context) : IProductRepository
    {
        public void AddProduct(Product product)
        {
            context.Products.Add(product);
        }

        public void DeleteProduct(Product product)
        {
            context.Remove(product);
        }

        public async Task<IReadOnlyList<string>?> GetBrandsAsync()
        {
            return await context.Products
                .Select(p => p.Brand)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await context.Products.FindAsync(id).AsTask();
        }

        public async Task<IReadOnlyList<Product>> GetProductsAsync(string? brand, string? type, string? sort)
        {
            var query = context.Products.AsQueryable();
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Brand == brand);
            }
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(p => p.Type == type);
            }
           
                query = sort switch
                {
                    "priceAsc" => query.OrderBy(p => p.Price),
                    "priceDesc" => query.OrderByDescending(p => p.Price),
                    _ => query.OrderBy(p => p.Name)
                };
            
            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<string>?> GetTypesAsync()
        {
            return await context.Products
                .Select(p => p.Type)
                .Distinct()
                .ToListAsync();
        }

        public bool ProductExists(int id)
        {
            return context.Products.Any(p => p.Id == id);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await context.SaveChangesAsync().ContinueWith(task => task.Result > 0);
        }

        public void UpdateProduct(Product product)
        {
            context.Entry(product).State = EntityState.Modified;
            // No need to call SaveChanges here, it should be done in the calling method

        }


    }
}
