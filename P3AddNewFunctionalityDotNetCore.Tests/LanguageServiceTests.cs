using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class LanguageServiceTests
    {
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IResponseCookies> _mockCookies;
        public LanguageServiceTests()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockCookies = new Mock<IResponseCookies>();

            _mockHttpContext.SetupGet(x => x.Response.Cookies).Returns(_mockCookies.Object);

        }

        [Theory]
        [InlineData("English", "en")]
        [InlineData("French", "fr")]
        [InlineData("Spanish", "es")]
        public void ChangeUiLanguage_ShouldCallCorrectMethod_WithCorrectArgument(string language, string culture)
        {
            //Arrange
            string keyPassedAsArgument = null, valuePassedAsArgument = null;
            _mockCookies.Setup(x => x.Append(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((string key, string value) =>
                {
                    keyPassedAsArgument = key;
                    valuePassedAsArgument = value;
                });
            var expectedArgumentToAppendMethod = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));

            var sut = new LanguageService();
            
            //Act
            sut.ChangeUiLanguage(_mockHttpContext.Object, language);

            //Assert
            _mockCookies.Verify(x => x.Append(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(expectedArgumentToAppendMethod, valuePassedAsArgument);
            Assert.Equal(CookieRequestCultureProvider.DefaultCookieName, keyPassedAsArgument);
        }

        [Theory]
        [InlineData("English", "en")]
        [InlineData("French", "fr")]
        [InlineData("Spanish", "es")]
        [InlineData("some random value", "en")]
        [InlineData("    ", "en")]
        [InlineData(null, "en")]
        [InlineData("1212#$#%#$", "en")]
        public void SetCulture_ShouldReturnCorrectValue(string languagePassedAsArgument, string expectedReturnValue)
        {
            //Arrange
            var sut = new LanguageService();
            //Act

            var returnedValue = sut.SetCulture(languagePassedAsArgument);

            //Assert
            Assert.Equal(returnedValue, expectedReturnValue);
        }

        [Theory]
        [InlineData("en")]
        [InlineData("fr")]
        [InlineData("es")]
        public void UpdateCultureCookie_ShouldCallCorrectMethod_WithCorrectArgument(string culture)
        {
            //Arrange
            string keyPassedAsArgument = null, valuePassedAsArgument = null;
            _mockCookies.Setup(x => x.Append(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((string key, string value) =>
                {
                    keyPassedAsArgument = key;
                    valuePassedAsArgument = value;
                });

            var sut = new LanguageService();
            var expectedArgumentToAppendMethod = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
            
            //Act
            sut.UpdateCultureCookie(_mockHttpContext.Object, culture);

            //Assert
            _mockCookies.Verify(x => x.Append(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.Equal(expectedArgumentToAppendMethod, valuePassedAsArgument);
            Assert.Equal(CookieRequestCultureProvider.DefaultCookieName, keyPassedAsArgument);

        }
    }
}
