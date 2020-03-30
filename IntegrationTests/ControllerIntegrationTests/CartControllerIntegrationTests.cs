using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.UnitTests.Comparators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.IntegrationTests.ControllerIntegrationTests
{

    public class CartControllerIntegrationTests
    {
        private readonly List<Product> _testProductsList;

        public CartControllerIntegrationTests()
        {
            _testProductsList = GetMockProducts();
        }

        private List<Product> GetMockProducts()
        {
            return new List<Product>()
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
        public void AddToCart_ProductExists_ShouldAddTheProductToCart()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            var productToAddToCart = 1;
            var cart = new Cart();

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var languageService = new LanguageService();
                var cartController = new CartController(cart, productService);

                // Act
                result = cartController.AddToCart(productToAddToCart);
            }
            Assert.Single(cart.Lines);
            var line = (cart.Lines as List<CartLine>).First();

            using (var context = new P3Referential(options))
            {
                Assert.Equal(context.Product.ToList().Find(x => x.Id == productToAddToCart), line.Product, new ProductEqualityComparator());

                //Cleanup
                context.Database.EnsureDeleted();
            }

        }

        [Fact]
        public void AddToCart_ProductDoesNotExist_ShouldNotAddTheProductToCart()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            var productToAddToCart = 99;
            var cart = new Cart();

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var languageService = new LanguageService();
                var cartController = new CartController(cart, productService);

                // Act
                result = cartController.AddToCart(productToAddToCart);
            }
            Assert.Empty(cart.Lines);

            using (var context = new P3Referential(options))
            {
                Assert.False(context.Product.ToList().Exists(x => x.Id == productToAddToCart));

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void RemoveFromCart_ProductExist_ShouldRemoveProduct()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            var productToAddToCart = 1;
            var cart = new Cart();

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var languageService = new LanguageService();
                var cartController = new CartController(cart, productService);

                // Act
                //First add item to cart
                result = cartController.AddToCart(productToAddToCart);
                //Verify that item is added
                Assert.Single(cart.Lines);
            }

            //Now remove item from cart
            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var languageService = new LanguageService();
                var cartController = new CartController(cart, productService);

                // Act
                //First add item to cart
                result = cartController.RemoveFromCart(productToAddToCart);

                //Verify that item is removed
                Assert.Empty(cart.Lines);
            }
        }

        [Fact]
        public void RemoveFromCart_ProductNotExist_ShouldNotRemoveProduct()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            var productToAddToCart = 1;
            var cart = new Cart();

            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var languageService = new LanguageService();
                var cartController = new CartController(cart, productService);

                // Act
                //First add item to cart
                result = cartController.AddToCart(productToAddToCart);
                //Verify that item is added
                Assert.Single(cart.Lines);
            }

            //Now remove item from cart
            using (var context = new P3Referential(options))
            {
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var languageService = new LanguageService();
                var cartController = new CartController(cart, productService);

                // Act
                //First add item to cart
                result = cartController.RemoveFromCart(99); //99 does not exist in the products

                //Verify that item is not removed
                Assert.Single(cart.Lines);
            }
        }
    }
}
