using Microsoft.Extensions.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System.Collections.Generic;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductServiceTests
    {
        private readonly Mock<ICart> _mockCart;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IStringLocalizer<ProductService>> _mockLocalizer;
        public ProductServiceTests()
        {
            _mockCart = new Mock<ICart>();
            _mockProductRepository = new Mock<IProductRepository>();
            _mockProductRepository.Setup(x => x.GetAllProducts()).Returns(GetMockProducts());
            _mockOrderRepository = new Mock<IOrderRepository>();
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
            var products = new List<Product>();
            products.Add(new Product()
            {
                Id = 1,
                Description = "product 1 desc",
                Details = "product 1 details",
                Name = "product 1 name",
                Price = 10.10,
                Quantity = 20
            });
            products.Add(new Product()
            {
                Id = 2,
                Description = "product 2 desc",
                Details = "product 2 details",
                Name = "product 2 name",
                Price = 10.20,
                Quantity = 30
            });
            products.Add(new Product()
            {
                Id = 3,
                Description = "product 3 desc",
                Details = "product 3 details",
                Name = "product 3 name",
                Price = 10.30,
                Quantity = 40
            });
            return products;
        }

        private List<ProductViewModel> GetMockProductViewModels()
        {
            var expectedViewModelId_1 = new ProductViewModel()
            {
                Id = 1,
                Description = "product 1 desc",
                Details = "product 1 details",
                Name = "product 1 name",
                Price = "10.1",
                Stock = "20"
            };
            var expectedViewModelId_2 = new ProductViewModel()
            {
                Id = 2,
                Description = "product 2 desc",
                Details = "product 2 details",
                Name = "product 2 name",
                Price = "10.2",
                Stock = "30"
            };
            var expectedViewModelId_3 = new ProductViewModel()
            {
                Id = 3,
                Description = "product 3 desc",
                Details = "product 3 details",
                Name = "product 3 name",
                Price = "10.3",
                Stock = "40"
            };
            var productViewModels = new List<ProductViewModel>();
            productViewModels.Add(expectedViewModelId_1);
            productViewModels.Add(expectedViewModelId_2);
            productViewModels.Add(expectedViewModelId_3);
            return productViewModels;
        }

        [Fact]
        public void GetAllProductsViewModel_ShouldReturnCorrectValue()
        {
            // Arrange
            ProductService sut = new ProductService(_mockCart.Object, _mockProductRepository.Object, _mockOrderRepository.Object, _mockLocalizer.Object);

            //Act
            var returnedValue = sut.GetAllProductsViewModel();

            //Assert
            //Check type
            Assert.IsType<List<ProductViewModel>>(returnedValue);
            
            //Check total count
            Assert.Equal(3, returnedValue.Count);

            //Check data
            GetMockProductViewModels().ForEach(expectedProductViewModel =>
            {
                Assert.Contains<ProductViewModel>(expectedProductViewModel, returnedValue, new ProductViewModelEqualityComparator());
            });

        }


        /// <summary>
        /// Verify that if product name doesn't have a value because of below conditions, correct error is returned.
        /// Scenarios:
        ///     product is null
        ///     product only contains white space
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void CheckProductModelErrors_ProductMustHaveName(string productName)
        {
            // Arrange
            ProductService sut = new ProductService(_mockCart.Object, _mockProductRepository.Object, _mockOrderRepository.Object, _mockLocalizer.Object);
            
            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = 1,
                Name = productName,
                Description = "test description",
                Details = "test details",
                Stock = "100",
                Price = "10.10"
            };
            // Act
            var validationErrors = sut.CheckProductModelErrors(productViewModel);

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("MissingName", validationErrors);
        }

        /// <summary>
        /// Verify that if price doesn't have a value because of below conditions, correct error is returned.
        /// Scenarios:
        ///     price is null
        ///     price only contains white space
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void CheckProductModelErrors_PriceMustHaveValue(string price)
        {
            // Arrange
            ProductService sut = new ProductService(_mockCart.Object, _mockProductRepository.Object, _mockOrderRepository.Object, _mockLocalizer.Object);

            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = 1,
                Name = "test product name",
                Description = "test description",
                Details = "test details",
                Stock = "100",
                Price = price
            };
            // Act
            var validationErrors = sut.CheckProductModelErrors(productViewModel);

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("MissingPrice", validationErrors);
        }

        /// <summary>
        /// Verify that if price is not a decimal value, correct error is returned.
        /// Scenarios:
        /// 
        ///     price contains string
        ///     price only contains white space
        ///     price is null
        ///     price contains alphanumeric
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        [InlineData("price")]
        [InlineData("p30")]
        public void CheckProductModelErrors_PriceMustBeNumeric(string price)
        {
            // Arrange
            ProductService sut = new ProductService(_mockCart.Object, _mockProductRepository.Object, _mockOrderRepository.Object, _mockLocalizer.Object);

            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = 1,
                Name = "test product name",
                Description = "test description",
                Details = "test details",
                Stock = "100",
                Price = price
            };
            // Act
            var validationErrors = sut.CheckProductModelErrors(productViewModel);

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("PriceNotANumber", validationErrors);
        }

        /// <summary>
        /// Verify that if price must be greater than zero
        /// Scenarios:
        ///     price is 0
        ///     price is less than 0
        ///     price is -0.01
        /// </summary>
        [Theory]
        [InlineData("0")]
        [InlineData("-10")]
        [InlineData("-0.01")]
        public void CheckProductModelErrors_PriceMustBeGreaterThanZero(string price)
        {
            // Arrange
            ProductService sut = new ProductService(_mockCart.Object, _mockProductRepository.Object, _mockOrderRepository.Object, _mockLocalizer.Object);

            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = 1,
                Name = "test product name",
                Description = "test description",
                Details = "test details",
                Stock = "100",
                Price = price
            };
            // Act
            var validationErrors = sut.CheckProductModelErrors(productViewModel);

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("PriceNotGreaterThanZero", validationErrors);
        }

        /// <summary>
        /// Verify that quantity/stock is required
        /// Scenarios:
        ///     quantity is null
        ///     quantity is whitespace only
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("   ")]
        public void CheckProductModelErrors_QtyIsRequired(string qty)
        {
            // Arrange
            ProductService sut = new ProductService(_mockCart.Object, _mockProductRepository.Object, _mockOrderRepository.Object, _mockLocalizer.Object);

            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = 1,
                Name = "test product name",
                Description = "test description",
                Details = "test details",
                Stock = qty,
                Price = "22",
            };
            // Act
            var validationErrors = sut.CheckProductModelErrors(productViewModel);

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("MissingStock", validationErrors);
        }

        /// <summary>
        /// Verify that quantity must be an integer
        /// Scenarios:
        ///     quantity is string
        ///     quantity has decimals
        /// </summary>
        [Theory]
        [InlineData("qty")]
        [InlineData("10.10")]
        public void CheckProductModelErrors_QtyMustBeInteger(string qty)
        {
            // Arrange
            ProductService sut = new ProductService(_mockCart.Object, _mockProductRepository.Object, _mockOrderRepository.Object, _mockLocalizer.Object);

            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = 1,
                Name = "test product name",
                Description = "test description",
                Details = "test details",
                Stock = qty,
                Price = "22",
            };
            // Act
            var validationErrors = sut.CheckProductModelErrors(productViewModel);

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("StockNotAnInteger", validationErrors);
        }

        /// <summary>
        /// Verify that quantity must be greater than zero
        /// Scenarios:
        ///     quantity is 0
        ///     quantity is less than 0
        /// </summary>
        [Theory]
        [InlineData("0")]
        [InlineData("-10")]
        public void CheckProductModelErrors_QtyMustBeGreaterThanZero(string qty)
        {
            // Arrange
            ProductService sut = new ProductService(_mockCart.Object, _mockProductRepository.Object, _mockOrderRepository.Object, _mockLocalizer.Object);

            ProductViewModel productViewModel = new ProductViewModel()
            {
                Id = 1,
                Name = "test product name",
                Description = "test description",
                Details = "test details",
                Stock = qty,
                Price = "22",
            };
            // Act
            var validationErrors = sut.CheckProductModelErrors(productViewModel);

            // Assert
            Assert.True(validationErrors.Count > 0);
            Assert.Contains("StockNotGreaterThanZero", validationErrors);
        }
    }
}