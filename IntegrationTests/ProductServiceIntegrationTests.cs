using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace IntegrationTests
{
    public class ProductServiceIntegrationTests
    {
        private readonly List<Product> _testProductsList;

        public ProductServiceIntegrationTests()
        {
            _testProductsList = new List<Product>()
            {
                new Product()
                {
                    Description = "test desc 1",
                    Details = "test details 1",
                    Name = "test product 1",
                    Price = 10.10,
                    Quantity = 100
                },
                new Product()
                {
                    Description = "test desc 2",
                    Details = "test details 2",
                    Name = "test product 2",
                    Price = 20.10,
                    Quantity = 200
                },
                new Product()
                {
                    Description = "test desc 3",
                    Details = "test details 3",
                    Name = "test product 3",
                    Price = 30.10,
                    Quantity = 300
                }
            };
        }


        private DbContextOptions<P3Referential> TestDbContextOptionsBuilder() 
        { 
            return new DbContextOptionsBuilder<P3Referential>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString(), new InMemoryDatabaseRoot()).Options; 
        }
        private void SeedTestDb(DbContextOptions<P3Referential> options) 
        { 
            using (var context = new P3Referential(options)) 
            { 
                foreach (var p in _testProductsList) 
                { 
                    context.Product.Add(p); 
                } 
                context.SaveChanges(); 
            } 
        }

        [Fact]
        public void Test_All_Products_Can_Be_Retrieved_From_Database()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);

                // Act
                result = productService.GetAllProducts();

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Assert
            Assert.NotNull(result);
            var products = Assert.IsType<List<Product>>(result);
            Assert.Equal(_testProductsList.Count, products.Count);
        }

        [Fact]
        public void Test_Product_Can_Be_Retrieved_From_Database_By_ProductId()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);
            var productId = 2;

            var result = (dynamic)null;
            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);

                // Act
                result = productService.GetProduct(productId);

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Assert
            Assert.NotNull(result);
            var taskWithProduct = Assert.IsType<Task<Product>>(result);
            var product = Assert.IsType<Product>(taskWithProduct.Result);
            var expectedProduct = _testProductsList.Find(x => x.Id == productId);
            
            var doesDataMatch = expectedProduct.Name == product.Name
                                && expectedProduct.Price == product.Price
                                && expectedProduct.Quantity == product.Quantity
                                && expectedProduct.Details == product.Details
                                && expectedProduct.Description == product.Description;
            
            Assert.True(doesDataMatch);
            
        }

        [Fact]
        public void Test_New_Product_Can_Be_Saved_To_Database()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();

            var productToAdd = new ProductViewModel()
            {
                Description = "test desc *",
                Details = "test details *",
                Name = "test product *",
                Price = "50.11",
                Stock = "501"
            };

            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);

                // Act
                productService.SaveProduct(productToAdd);
            }

            //Assert
            using (var context = new P3Referential(options))
            {
                var savedProducts = context.Product.ToList();
                Assert.Single(savedProducts);
                Assert.IsAssignableFrom<List<Product>>(savedProducts);

                var savedProduct = savedProducts.Find(x => x.Id == 1);

                var doesDataMatch = productToAdd.Description == savedProduct.Description
                        && productToAdd.Details == savedProduct.Details
                        && productToAdd.Name == savedProduct.Name
                        && productToAdd.Price == savedProduct.Price.ToString()
                        && productToAdd.Stock == savedProduct.Quantity.ToString();

                Assert.True(doesDataMatch);

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void Test_Existing_Product_Can_Be_Deleted()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);
            var productId = 2;

            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);

                // Act
                productService.DeleteProduct(productId);
            }

            //Assert
            using (var context = new P3Referential(options))
            {
                var products = context.Product.ToList();
                var doesProductExistAnyMore = products.Exists(x => x.Id == productId);
                Assert.False(doesProductExistAnyMore);

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void Test_Product_Stock_Can_Be_Updated()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);
            var productId = 2;
            var qtyToRemove = 20;

            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                cart.AddItem(_testProductsList.Find(x => x.Id == productId), qtyToRemove);
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);

                // Act
                productService.UpdateProductQuantities();
            }

            //Assert
            using (var context = new P3Referential(options))
            {
                var products = context.Product.ToList();
                var product = products.Find(x => x.Id == productId);
                var originalProduct = _testProductsList.Find(x => x.Id == productId);
                Assert.Equal(originalProduct.Quantity - qtyToRemove, product.Quantity);

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }
    }
}
