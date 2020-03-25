using Microsoft.AspNetCore.Mvc;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.UnitTests.ControllerTests
{
    public class CartControllerTests
    {
        private readonly Mock<ICart> _mockCart;
        private readonly Mock<IProductService> _mockProductService;

        public CartControllerTests()
        {
            _mockCart = new Mock<ICart>();
            _mockCart.Setup(x => x.Lines).Returns(GetMockCartItems());
            _mockProductService = new Mock<IProductService>();
            _mockProductService.Setup(x => x.GetAllProducts())
                      .Returns(GetMockProducts());

            _mockProductService.Setup(x => x.GetProductById(It.IsAny<int>()))
                                  .Returns<int>((id) => GetMockProducts().FirstOrDefault(x => x.Id == id));
        }

        private List<CartLine> GetMockCartItems()
        {
            return new List<CartLine>()
            {
                new CartLine()
                {
                    OrderLineId = 1,
                    Product = new Product()
                    {
                        Id = 1,
                        Description = "test description",
                        Details = "test details",
                        Name = "test name",
                        Price = 20.20,
                        Quantity = 120
                    },
                    Quantity = 1
                },
                new CartLine()
                {
                    OrderLineId = 2,
                    Product = new Product()
                    {
                        Id = 2,
                        Description = "test description 2",
                        Details = "test details 2",
                        Name = "test name 2",
                        Price = 40.20,
                        Quantity = 320
                    },
                    Quantity = 5
                }
            };
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
        [Fact]
        public void Index_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new CartController(_mockCart.Object, _mockProductService.Object);

            //Act
            var returnedValue = sut.Index();

            //Assert
            var viewResult = Assert.IsType<ViewResult>(returnedValue);
            Assert.IsAssignableFrom<ICart>(viewResult.ViewData.Model);
        }

        [Fact]
        public void AddToCart_ProductIsNull_ShouldRedirectToProduct()
        {
            //Arrange
            var sut = new CartController(_mockCart.Object, _mockProductService.Object);
            _mockProductService.Setup(x => x.GetProductById(1)).Returns((int id) => null);

            //Act
            var returnedValue = sut.AddToCart(1);

            //Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(returnedValue);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Product", redirectToActionResult.ControllerName);

            //Verify that addItem is not called on cart
            _mockCart.Verify(x => x.AddItem(It.IsAny<Product>(), It.IsAny<int>()), Times.Never);
        }
            
        [Fact]
        public void AddToCart_ProductIsNotNull_ShouldCallCorrectMethod_ShouldRedirectToIndex()
        {
            //Arrange
            var sut = new CartController(_mockCart.Object, _mockProductService.Object);

            //Act
            var returnedValue = sut.AddToCart(1);

            //Assert
            _mockCart.Verify(x => x.AddItem(It.Is<Product>(p => p.Id == 1), 1), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(returnedValue);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void RemoveFromCart_ProductIsNull_ShouldRedirectToIndex()
        {
            //Arrange
            var sut = new CartController(_mockCart.Object, _mockProductService.Object);

            //Act
            //pass id=10 which does not exist in mock data
            var returnedValue = sut.RemoveFromCart(10);

            //Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(returnedValue);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            //Verify that RemoveLine is never called on cart
            _mockCart.Verify(x => x.RemoveLine(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public void RemoveFromCart_ProductIsNotNull_ShouldCallCorrectMethod()
        {
            //Arrange
            var sut = new CartController(_mockCart.Object, _mockProductService.Object);

            //Act
            var returnedValue = sut.RemoveFromCart(1);

            //Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(returnedValue);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            //Verify that RemoveLine is called on cart once
            _mockCart.Verify(x => x.RemoveLine(It.Is<Product>(p => p.Id == 1)), Times.Once);
        }
    }
}
