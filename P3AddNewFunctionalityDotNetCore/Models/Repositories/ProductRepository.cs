using Microsoft.EntityFrameworkCore;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P3AddNewFunctionalityDotNetCore.Models.Repositories
{
    public class ProductRepository : IProductRepository
    {
        //Bug Fix: context should not be static as it will result in race conditions by multiple threads
        private readonly P3Referential _context;

        public ProductRepository(P3Referential context)
        {
                _context = context;
        }
        public async Task<Product> GetProduct(int id)
        {
            var product = await _context.Product.SingleOrDefaultAsync(m => m.Id == id);
            return product;
        }

        public async Task<IList<Product>> GetProduct()
        {
            var products = await _context.Product.ToListAsync();
            return products;
        }
        /// <summary>
        /// Get all products from the inventory
        /// </summary>
        public IEnumerable<Product> GetAllProducts()
        {
            IEnumerable<Product> productEntities= _context.Product.Where(p => p.Id > 0);
            return productEntities.ToList();
        }

        /// <summary>
        /// Update the stock of a product by its id
        /// </summary>
        public void UpdateProductStocks(int id, int quantityToRemove)
        {
            Product product = _context.Product.First(p => p.Id == id);
            product.Quantity = product.Quantity - quantityToRemove;

            /*Bug Fixed: This condition should be <= 0 to cater for the scenario where
             * Product stock is already 0 and this method gets called with quantityToRemove as greater than zero.
             */
            if (product.Quantity <= 0)
                _context.Product.Remove(product);
            else
            {
                _context.Product.Update(product);
                _context.SaveChanges();
            }   
        }

        public void SaveProduct(Product product)
        {
            if (product != null)
            {
                _context.Product.Add(product);
                _context.SaveChanges();
            }
        }

        public void DeleteProduct(int id)
        {
            //Bug fix - Changed First to FirstOrDefault to unit test null condition
            //First will cause exception if product not found
            //FirstOrDefault will return null if proudct not found
            Product product = _context.Product.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _context.Product.Remove(product);
                _context.SaveChanges();
            }
        }
    }
}
