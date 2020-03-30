using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using P3AddNewFunctionalityDotNetCore.UnitTests.Comparators;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.IntegrationTests.ControllerIntegrationTests
{
    public class ProductControllerIntegrationTests
    {
        private readonly List<Product> _testProductsList;
        private readonly List<ProductViewModel> _testProductViewModels;
        private readonly Mock<IStringLocalizer<ProductService>> _mockLocalizer;

        public ProductControllerIntegrationTests()
        {
            _testProductsList = GetMockProducts();
            _testProductViewModels = GetMockProductViewModels();
            _mockLocalizer = new Mock<IStringLocalizer<ProductService>>();

            _mockLocalizer.Setup(_ => _["MissingName"]).Returns(new LocalizedString("MissingName", "MissingName"));
            _mockLocalizer.Setup(_ => _["MissingPrice"]).Returns(new LocalizedString("MissingPrice", "MissingPrice"));
            _mockLocalizer.Setup(_ => _["PriceNotANumber"]).Returns(new LocalizedString("PriceNotANumber", "PriceNotANumber"));
            _mockLocalizer.Setup(_ => _["PriceNotGreaterThanZero"]).Returns(new LocalizedString("PriceNotGreaterThanZero", "PriceNotGreaterThanZero"));
            _mockLocalizer.Setup(_ => _["MissingStock"]).Returns(new LocalizedString("MissingStock", "MissingStock"));
            _mockLocalizer.Setup(_ => _["StockNotAnInteger"]).Returns(new LocalizedString("StockNotAnInteger", "StockNotAnInteger"));
            _mockLocalizer.Setup(_ => _["StockNotGreaterThanZero"]).Returns(new LocalizedString("StockNotGreaterThanZero", "StockNotGreaterThanZero"));

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

        private List<ProductViewModel> GetMockProductViewModels()
        {
            var expectedViewModelId_1 = new ProductViewModel()
            {
                Id = 1,
                Description = "test desc 1",
                Details = "test details 1",
                Name = "test product 1",
                Price = "10.1",
                Stock = "100"
            };
            var expectedViewModelId_2 = new ProductViewModel()
            {
                Id = 2,
                Description = "test desc 2",
                Details = "test details 2",
                Name = "test product 2",
                Price = "20.1",
                Stock = "200"
            };
            var expectedViewModelId_3 = new ProductViewModel()
            {
                Id = 3,
                Description = "test desc 3",
                Details = "test details 3",
                Name = "test product 3",
                Price = "30.1",
                Stock = "300"
            };
            var productViewModels = new List<ProductViewModel>();
            productViewModels.Add(expectedViewModelId_1);
            productViewModels.Add(expectedViewModelId_2);
            productViewModels.Add(expectedViewModelId_3);
            return productViewModels;
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
        public void Index_ShouldReturnCorrectValue()
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
                var languageService = new LanguageService();
                var productController = new ProductController(productService, languageService);
                // Act
                result = productController.Index();

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result) as ViewResult;
            var model = Assert.IsType<List<ProductViewModel>>(viewResult.ViewData.Model);
            Assert.Equal(_testProductViewModels, model, new ProductViewModelEqualityComparator());
        }

        [Fact]
        public void Admin_ShouldReturnCorrectValue()
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
                var languageService = new LanguageService();
                var productController = new ProductController(productService, languageService);
                // Act
                result = productController.Admin();

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Assert
            Assert.NotNull(result);
            var viewResult = Assert.IsType<ViewResult>(result) as ViewResult;
            var model = Assert.IsAssignableFrom<IOrderedEnumerable<ProductViewModel>>(viewResult.ViewData.Model);
            Assert.Equal(_testProductViewModels.OrderByDescending(p => p.Id), model, new ProductViewModelEqualityComparator());
        }

        [Fact]
        public void Create_ShouldReturnCorrectValue()
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
                var languageService = new LanguageService();
                var productController = new ProductController(productService, languageService);
                // Act
                result = productController.Create();

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Assert
            Assert.IsType<ViewResult>(result);
        }


        [Fact]
        public void Post_Create_ValidProduct_ShouldBeSaved()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            int totalProductsBefore = 0;
            var productToAdd = new ProductViewModel()
            {
                Description = "product 4 desc",
                Details = "product 4 details",
                Name = "product 4 name",
                Price = "50.3",
                Stock = "500"
            };

            using (var context = new P3Referential(options))
            {
                totalProductsBefore = context.Product.Count();
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var languageService = new LanguageService();
                var productController = new ProductController(productService, languageService);

                // Act
                result = productController.Create(productToAdd);
            }

            using (var context = new P3Referential(options))
            { 
                var productsSaved = context.Product.ToList();
                int totalProductsAfter = productsSaved.Count;
                
                //Verify that product count is increased by one
                Assert.Equal(totalProductsBefore + 1, totalProductsAfter);

                var doesProductExist = productsSaved.Exists(p => p.Description == productToAdd.Description
                  && p.Details == productToAdd.Details
                  && p.Name == productToAdd.Name
                  && p.Price.ToString() == productToAdd.Price
                  && p.Quantity.ToString() == productToAdd.Stock);

                Assert.True(doesProductExist);

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Other Assertions
            var redirectToAction = Assert.IsType<RedirectToActionResult>(result) as RedirectToActionResult;
            Assert.Equal("Admin", redirectToAction.ActionName);
        }

        [Fact]
        public void Post_Create_InvalidProduct_MissingPrice_ShouldNotBeSaved()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            int totalProductsBefore = 0;
            var productToAdd = new ProductViewModel()
            {
                Description = "product 4 desc",
                Details = "product 4 details",
                Name = "product 4 name",
                Stock = "500"
            };

            using (var context = new P3Referential(options))
            {
                totalProductsBefore = context.Product.Count();
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, _mockLocalizer.Object);
                var languageService = new LanguageService();
                var productController = new ProductController(productService, languageService);

                // Act
                result = productController.Create(productToAdd);
            }
            using (var context = new P3Referential(options))
            { 
                var productsSaved = context.Product.ToList();
                int totalProductsAfter = productsSaved.Count;

                //Verify that product count did not increase by one
                Assert.Equal(totalProductsBefore, totalProductsAfter);

                var doesProductExist = productsSaved.Exists(p => p.Description == productToAdd.Description
                  && p.Details == productToAdd.Details
                  && p.Name == productToAdd.Name
                  && p.Quantity.ToString() == productToAdd.Stock);

                Assert.False(doesProductExist);

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Other Assertions
            var viewResult = Assert.IsType<ViewResult>(result) as ViewResult;
            Assert.False(viewResult.ViewData.ModelState.IsValid);
        }


        [Fact]
        public void DeleteProduct_ProductShouldBeDeleted()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            int totalProductsBefore = 0;
            var productIdToDelete = 1;

            using (var context = new P3Referential(options))
            {
                totalProductsBefore = context.Product.Count();
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var languageService = new LanguageService();
                var productController = new ProductController(productService, languageService);

                // Act
                result = productController.DeleteProduct(productIdToDelete);
            }
            using (var context = new P3Referential(options))
            { 
                var productsRemaining = context.Product.ToList();
                int totalProductsAfter = productsRemaining.Count;
                
                //Verify that product count is increased by one
                Assert.Equal(totalProductsBefore - 1, totalProductsAfter);

                var doesProductExist = productsRemaining.Exists(p => p.Id == productIdToDelete);

                Assert.False(doesProductExist);

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }
    }
}
