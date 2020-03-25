using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.UnitTests.ControllerTests
{
    public class OrderControllerTests
    {

        private readonly Mock<ICart> _mockCart;
        private readonly Mock<IOrderService> _mockOrderService;
        private readonly Mock<IStringLocalizer<OrderController>> _mockLocalizer;

        public OrderControllerTests()
        {
            _mockCart = new Mock<ICart>();
            _mockOrderService = new Mock<IOrderService>();
            _mockLocalizer = new Mock<IStringLocalizer<OrderController>>();
            _mockLocalizer.Setup(_ => _["CartEmpty"]).Returns(new LocalizedString("CartEmpty", "CartEmpty"));

        }

        [Fact]
        public void Index_ShouldCallCorrectMethod_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new OrderController(_mockCart.Object, _mockOrderService.Object, _mockLocalizer.Object);

            //Act
            var returnedValue = sut.Index();

            //Assert         
            var viewResult = Assert.IsType<ViewResult>(returnedValue);
            var model = Assert.IsType<OrderViewModel>(viewResult.ViewData.Model);
            Assert.NotNull(model);
        }

        [Fact]
        public void Post_Index_InvalidCart_ShouldReturnErrors()
        {
            //Arrange
            var sut = new OrderController(_mockCart.Object, _mockOrderService.Object, _mockLocalizer.Object);
            var errorMessage = "CartEmpty";

            //Act
            var returnedValue = sut.Index(It.IsAny<OrderViewModel>());

            //Assert
            var viewResult = Assert.IsType<ViewResult>(returnedValue);

            Assert.False(viewResult.ViewData.ModelState.IsValid);
            var modelState = viewResult.ViewData.ModelState;

            //Verify the error message
            foreach (var modelStateValue in modelState.Values)
            {
                Assert.Single(modelStateValue.Errors);
                foreach (var error in modelStateValue.Errors)
                {
                    Assert.Equal(errorMessage, error.ErrorMessage);
                }
            }
            
            //Verify that SaveOrder method was not called
            _mockOrderService.Verify(x => x.SaveOrder(It.IsAny<OrderViewModel>()), Times.Never);
        }

        [Fact]
        public void Post_Index_InvalidModel_ShouldReturnErrors()
        {
            //Arrange
            var sut = new OrderController(_mockCart.Object, _mockOrderService.Object, _mockLocalizer.Object);
            var errorMessage = "random error";
            sut.ModelState.AddModelError("error", errorMessage);

            _mockCart.SetupGet(x => x.Lines).Returns(new List<CartLine>() { new CartLine()});

            //Act
            var returnedValue = sut.Index(It.IsAny<OrderViewModel>());

            //Assert
            var viewResult = Assert.IsType<ViewResult>(returnedValue);

            Assert.False(viewResult.ViewData.ModelState.IsValid);
            var modelState = viewResult.ViewData.ModelState;
            
            //Verify the error message
            foreach (var modelStateValue in modelState.Values)
            {
                Assert.Single(modelStateValue.Errors);
                foreach (var error in modelStateValue.Errors)
                {
                    Assert.Equal(errorMessage, error.ErrorMessage);
                }
            }

            //Verify that SaveOrder method was not called
            _mockOrderService.Verify(x => x.SaveOrder(It.IsAny<OrderViewModel>()), Times.Never);
        }

        [Fact]
        public void Post_Index_ValidModel_ShouldReturnCorrectValue_NoErrors()
        {
            //Arrange
            var sut = new OrderController(_mockCart.Object, _mockOrderService.Object, _mockLocalizer.Object);

            _mockCart.SetupGet(x => x.Lines).Returns(new List<CartLine>() { new CartLine() });

            //Act
            var orderViewModel = new OrderViewModel()
            {
                OrderId = 1,
                Name = "test order",
                Address = "test address",
                City = "test city",
                Zip = "test zip",
                Country = "test country",
                Date = new DateTime(2020, 2, 2)
            };

            var returnedValue = sut.Index(orderViewModel);

            //Assert
            var viewResult = Assert.IsType<RedirectToActionResult>(returnedValue);
            Assert.Equal("Completed", viewResult.ActionName);
            _mockOrderService.Verify(x => x.SaveOrder(It.Is<OrderViewModel>(o => o.OrderId == orderViewModel.OrderId
                && o.Name == orderViewModel.Name
                && o.Address == orderViewModel.Address
                && o.City == orderViewModel.City
                && o.Zip == orderViewModel.Zip
                && o.Country == orderViewModel.Country
                && o.Date.Equals(orderViewModel.Date)
            )), Times.Once);
        }

        [Fact]
        public void Completed_ShouldCallCorrectMethod_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new OrderController(_mockCart.Object, _mockOrderService.Object, _mockLocalizer.Object);

            //Act
            var returnedValue = sut.Completed();

            //Assert
            _mockCart.Verify(x => x.Clear(), Times.Once);
            Assert.IsType<ViewResult>(returnedValue);
        }
    }
}
