using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class LanguageControllerTests
    {
        private readonly Mock<ILanguageService> _mockLanguageService;

        public LanguageControllerTests()
        {
            _mockLanguageService = new Mock<ILanguageService>();
        }

        [Fact]
        public void ChangeUiLanguage_LanguageIsNull_ShouldNotCallChangeUILanguageMethod()
        {
            //Arranage
            var sut = new LanguageController(_mockLanguageService.Object);
            var languageModel = new LanguageViewModel()
            {
                Language = null
            };
            //Act
            sut.ChangeUiLanguage(languageModel, "test url");

            //Assert
            _mockLanguageService.Verify(x => x.ChangeUiLanguage(It.IsAny<HttpContext>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void ChangeUiLanguage_LanguageIsNotNull_ShouldCallChangeUILanguageMethod()
        {
            //Arranage
            var sut = new LanguageController(_mockLanguageService.Object);
            var languageModel = new LanguageViewModel()
            {
                Language = "English"
            };
            //Act
            var returnedValue = sut.ChangeUiLanguage(languageModel, "test url");

            //Assert
            _mockLanguageService.Verify(x => x.ChangeUiLanguage(It.IsAny<HttpContext>(), languageModel.Language), Times.Once);
            var redirectResult = Assert.IsType<RedirectResult>(returnedValue);
            Assert.Equal("test url", redirectResult.Url);
        }
    }
}
