using Microsoft.AspNetCore.Mvc;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<ILanguageService> _mockLanguageService;

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockProductService.Setup(x => x.GetAllProductsViewModel())
                .Returns(GetMockProductViewModels());
            _mockLanguageService = new Mock<ILanguageService>();
        }

        private List<ProductViewModel> GetMockProductViewModels()
        {
            return new List<ProductViewModel>()
            {
                new ProductViewModel()
                {
                    Id = 1,
                    Name = "test name",
                    Description = "Test description",
                    Details = "test details",
                    Stock = "12",
                    Price = "10.10"
                },
                new ProductViewModel()
                {
                    Id = 2,
                    Name = "test name 2",
                    Description = "Test description 2",
                    Details = "test details 2",
                    Stock = "15",
                    Price = "12.10"
                }
            };
        }


        [Fact]
        public void Index_ShouldReturnCorrectValue_ShouldCallCorrectMethod()
        {
            //Arrange
            var sut = new ProductController(_mockProductService.Object,_mockLanguageService.Object);

            //Act
            var returnedValue = sut.Index();

            //Assert
            _mockProductService.Verify(x => x.GetAllProductsViewModel(), Times.Once);
            Assert.IsAssignableFrom<IActionResult>(returnedValue);
            ViewResult viewResult = Assert.IsType<ViewResult>(returnedValue);
            var model = Assert.IsType<List<ProductViewModel>>(viewResult.ViewData.Model);
            Assert.Equal(GetMockProductViewModels().Count, model.Count);
        }

        [Fact]
        public void Admin_ShouldCallCorrectMethod()
        {
            //Arrange
            var sut = new ProductController(_mockProductService.Object, _mockLanguageService.Object);

            //Act
            var returnedValue = sut.Admin();

            //Assert
            _mockProductService.Verify(x => x.GetAllProductsViewModel(), Times.Once);
            Assert.IsAssignableFrom<IActionResult>(returnedValue);
            ViewResult viewResult = Assert.IsType<ViewResult>(returnedValue);
            var model = Assert.IsAssignableFrom<IOrderedEnumerable<ProductViewModel>>(viewResult.ViewData.Model).ToList<ProductViewModel>();
            Assert.Equal(GetMockProductViewModels().Count, model.Count);

            //Verify the data is sorted
            int i = 0;
            foreach(var p in GetMockProductViewModels().OrderByDescending(p => p.Id))
            {
                Assert.Equal(p.Id , model[i++].Id);
            }
        }

        [Fact]
        public void Create_ShouldReturnCorrectType()
        {
            //Arrange
            var sut = new ProductController(_mockProductService.Object, _mockLanguageService.Object);

            //Act
            var returnedValue = sut.Create();

            //Arrange
            Assert.IsType<ViewResult>(returnedValue);
        }

        [Fact]
        public void Post_Create_InvalidModel_ShouldReturnErrors()
        {
            //Arrange
            var sut = new ProductController(_mockProductService.Object, _mockLanguageService.Object);
            var errorMessage = "some error message";
            _mockProductService.Setup(x => x.CheckProductModelErrors(It.IsAny<ProductViewModel>()))
                .Returns(new List<string>() { errorMessage });
                    
            //Act
            var returnedValue = sut.Create(It.IsAny<ProductViewModel>());

            //Assert
            var viewResult = Assert.IsType<ViewResult>(returnedValue);
            var modelState = viewResult.ViewData.ModelState;
            Assert.False(modelState.IsValid);
            Assert.Single(modelState.Values);
            foreach(var modelStateValue in modelState.Values)
            {
                Assert.Single(modelStateValue.Errors);
                foreach(var error in modelStateValue.Errors)
                {
                    Assert.Equal(errorMessage, error.ErrorMessage);
                }
            }

            //Verify that SaveProduct should never be called
            _mockProductService.Verify(x => x.SaveProduct(It.IsAny<ProductViewModel>()), Times.Never);

        }

        [Fact]
        public void Post_Create_ValidModel_ShouldCallSaveProduct_ShouldRedirectToAdminPage()
        {
            //Arrange
            var sut = new ProductController(_mockProductService.Object, _mockLanguageService.Object);
            _mockProductService.Setup(x => x.CheckProductModelErrors(It.IsAny<ProductViewModel>()))
                .Returns(new List<string>());

            //Act
            var returnedValue = sut.Create(It.IsAny<ProductViewModel>());

            //Assert
            _mockProductService.Verify(x => x.SaveProduct(It.IsAny<ProductViewModel>()), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(returnedValue);
            Assert.Equal("Admin", redirectToActionResult.ActionName);
        }

        [Fact]
        public void DeleteProduct_ShouldCallCorrectMethod_ShouldRedirectToAdminPage()
        {
            //Arrange
            var sut = new ProductController(_mockProductService.Object, _mockLanguageService.Object);

            //Act
            var returnedValue = sut.DeleteProduct(It.IsAny<int>());

            //Assert
            _mockProductService.Verify(x => x.DeleteProduct(It.IsAny<int>()), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(returnedValue);
            Assert.Equal("Admin", redirectToActionResult.ActionName);
        }
    }
}
