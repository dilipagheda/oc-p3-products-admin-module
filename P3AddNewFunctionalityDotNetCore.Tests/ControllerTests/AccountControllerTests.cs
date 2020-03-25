using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.UnitTests.ControllerTests
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<IdentityUser>> _mockUserManager;
        private readonly Mock<SignInManager<IdentityUser>> _mockSignInManager;


        public AccountControllerTests()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(store.Object, null, null, null, null, null, null, null, null);

            var _mock_httpContextAccessor = new Mock<IHttpContextAccessor>();
            var _mockUserClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IdentityUser>>();

            _mockSignInManager = new Mock<SignInManager<IdentityUser>>(_mockUserManager.Object, _mock_httpContextAccessor.Object, _mockUserClaimsPrincipalFactory.Object, null,null,null);
        }

        [Fact]
        public void Login_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);

            //Act
            var returnedValue = sut.Login("test return url");

            //Assert
            var viewResult = Assert.IsType<ViewResult>(returnedValue);
            var loginModel = Assert.IsAssignableFrom<LoginModel>(viewResult.ViewData.Model);
            Assert.Equal("test return url", loginModel.ReturnUrl);
        }

        [Fact]
        public void Post_Login_InvalidModel_ShouldReturnError_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
            sut.ModelState.AddModelError("error", "some error");
            var loginModel = new LoginModel()
            {
                Name = "test name",
                Password = "test password",
                ReturnUrl = "return url"
            };

            //Act
            var returnedValue = sut.Login(loginModel);

            //Assert
            var result = Assert.IsType<ViewResult>(returnedValue.Result);
            var model = Assert.IsType<LoginModel>(result.ViewData.Model);
            Assert.NotNull(model);

            //Verify errors
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.Equal(2, result.ViewData.ModelState.Values.Count());
            bool isErrorFound = false;
            foreach(var value in result.ViewData.ModelState.Values)
            {
                Assert.Single(value.Errors);
                foreach(var error in value.Errors)
                {
                    if(error.ErrorMessage.Equals("Invalid name or password"))
                    {
                        isErrorFound = true;
                    }
                }
            }
            Assert.True(isErrorFound);

            //Verify that there was no call - FindByNameAsync
            _mockUserManager.Verify(x => x.FindByNameAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void Post_Login_ValidModel_UserNotFound_ShouldReturnError_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
            var loginModel = new LoginModel()
            {
                Name = "test name",
                Password = "test password",
                ReturnUrl = "return url"
            };

            //Act
            var returnedValue = sut.Login(loginModel);

            //Assert
            //Verify that there was no call - SignOutAsync
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Never);

            var result = Assert.IsType<ViewResult>(returnedValue.Result);
            var model = Assert.IsType<LoginModel>(result.ViewData.Model);
            Assert.NotNull(model);

            //Verify errors
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.Single(result.ViewData.ModelState.Values);
            bool isErrorFound = false;
            foreach (var value in result.ViewData.ModelState.Values)
            {
                Assert.Single(value.Errors);
                foreach (var error in value.Errors)
                {
                    if (error.ErrorMessage.Equals("Invalid name or password"))
                    {
                        isErrorFound = true;
                    }
                }
            }
            Assert.True(isErrorFound);
        }

        [Fact]
        public void Post_Login_ValidModel_UserFound_LoginSucceeded_ReturnUrlExists_ShouldCallCorrectMethod_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
            var loginModel = new LoginModel()
            {
                Name = "test name",
                Password = "test password",
                ReturnUrl = "return url"
            };
            var _mockUser = new IdentityUser()
            {
                Id = "1"
            };
            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_mockUser));

            _mockSignInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(_mockUser, loginModel.Password, false, false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

            //Act
            var returnedValue = sut.Login(loginModel);

            //Assert
            var result = Assert.IsType<RedirectResult>(returnedValue.Result);
            Assert.Equal("return url", result.Url);

            //Verify the correct methods were called
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.Is<IdentityUser>(p => p.Id == _mockUser.Id), loginModel.Password, false, false), Times.Once);
        }

        [Fact]
        public void Post_Login_ValidModel_UserFound_LoginSucceeded_ReturnUrlDoesNotExists_ShouldCallCorrectMethod_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
            var loginModel = new LoginModel()
            {
                Name = "test name",
                Password = "test password",
                ReturnUrl = null
            };
            var _mockUser = new IdentityUser()
            {
                Id = "1"
            };
            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_mockUser));

            _mockSignInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(_mockUser, loginModel.Password, false, false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Success));

            //Act
            var returnedValue = sut.Login(loginModel);

            //Assert
            var redirectResult = Assert.IsType<RedirectResult>(returnedValue.Result);
            Assert.Equal("/Admin/Index", redirectResult.Url);

            //Verify the correct methods were called
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.Is<IdentityUser>(p => p.Id == _mockUser.Id), loginModel.Password, false, false), Times.Once);
        }

        [Fact]
        public void Post_Login_ValidModel_UserNotFound_LoginFailed_ShouldReturnError_ShouldNotCallMethods()
        {
            //Arrange
            var sut = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
            var loginModel = new LoginModel()
            {
                Name = "test name",
                Password = "test password",
                ReturnUrl = "return url"
            };
            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .Returns(Task.FromResult<IdentityUser>(null));

            _mockSignInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(It.IsAny<IdentityUser>(), loginModel.Password, false, false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

            //Act
            var returnedValue = sut.Login(loginModel);

            //Assert
            var result = Assert.IsType<ViewResult>(returnedValue.Result);
            var model = Assert.IsType<LoginModel>(result.ViewData.Model);
            Assert.NotNull(model);

            //Verify errors
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.Single(result.ViewData.ModelState.Values);
            bool isErrorFound = false;
            foreach (var value in result.ViewData.ModelState.Values)
            {
                Assert.Single(value.Errors);
                foreach (var error in value.Errors)
                {
                    if (error.ErrorMessage.Equals("Invalid name or password"))
                    {
                        isErrorFound = true;
                    }
                }
            }
            Assert.True(isErrorFound);

            //Verify the below methods were NOT called
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Never);
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.IsAny<IdentityUser>(), loginModel.Password, false, false), Times.Never);
        }


        [Fact]
        public void Post_Login_ValidModel_UserFound_LoginFailed_ShouldReturnErrors_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new AccountController(_mockUserManager.Object, _mockSignInManager.Object);
            var loginModel = new LoginModel()
            {
                Name = "test name",
                Password = "test password",
                ReturnUrl = "return url"
            };
            var _mockUser = new IdentityUser()
            {
                Id = "1"
            };
            _mockUserManager.Setup(x => x.FindByNameAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(_mockUser));

            _mockSignInManager.Setup(x => x.SignOutAsync()).Returns(Task.CompletedTask);

            _mockSignInManager.Setup(x => x.PasswordSignInAsync(_mockUser, loginModel.Password, false, false))
                .Returns(Task.FromResult(Microsoft.AspNetCore.Identity.SignInResult.Failed));

            //Act
            var returnedValue = sut.Login(loginModel);

            //Assert
            var result = Assert.IsType<ViewResult>(returnedValue.Result);
            var model = Assert.IsType<LoginModel>(result.ViewData.Model);
            Assert.NotNull(model);

            //Verify errors
            Assert.False(result.ViewData.ModelState.IsValid);
            Assert.Single(result.ViewData.ModelState.Values);
            bool isErrorFound = false;
            foreach (var value in result.ViewData.ModelState.Values)
            {
                Assert.Single(value.Errors);
                foreach (var error in value.Errors)
                {
                    if (error.ErrorMessage.Equals("Invalid name or password"))
                    {
                        isErrorFound = true;
                    }
                }
            }
            Assert.True(isErrorFound);

            //Verify the correct methods were called
            _mockSignInManager.Verify(x => x.SignOutAsync(), Times.Once);
            _mockSignInManager.Verify(x => x.PasswordSignInAsync(It.Is<IdentityUser>(p => p.Id == _mockUser.Id), loginModel.Password, false, false), Times.Once);
        }

    }
}
