using P3AddNewFunctionalityDotNetCore.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace P3AddNewFunctionalityDotNetCore.Models.Repositories
{
    public interface IProductRepository
    {
        IEnumerable<Product> GetAllProducts();
        void UpdateProductStocks(int productId, int quantityToRemove);
        void SaveProduct(Product product);
        void DeleteProduct(int id);
        Task<Product> GetProduct(int id);
        Task<IList<Product>> GetProduct();
    }
}
