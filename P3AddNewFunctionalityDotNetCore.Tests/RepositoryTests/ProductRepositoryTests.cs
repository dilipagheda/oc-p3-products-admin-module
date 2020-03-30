using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using MockQueryable.Moq;
using Moq;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.UnitTests.Comparators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.UnitTests.RepositoryTests
{
    public class ProductRepositoryTests
    {
        private readonly Mock<P3Referential> _mockContext;
        private readonly Mock<DbSet<Product>> _mockDbSetProducts;
        public ProductRepositoryTests()
        {
            _mockContext = new Mock<P3Referential>();

            _mockDbSetProducts = GetMockProducts().AsQueryable().BuildMockDbSet();

            _mockDbSetProducts.Setup(x => x.Remove(It.IsAny<Product>())).Returns(It.IsAny<EntityEntry<Product>>());
            _mockDbSetProducts.Setup(x => x.Add(It.IsAny<Product>())).Returns(It.IsAny<EntityEntry<Product>>());

            _mockContext.SetupGet(x => x.Product).Returns(_mockDbSetProducts.Object);
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
            products.Add(new Product()
            {
                Id = 4,
                Description = "product 3 desc",
                Details = "product 3 details",
                Name = "product 3 name",
                Price = 10.30,
                Quantity = 0
            });
            return products;
        }

        [Fact]
        public void GetProduct_WithId_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);
            var productId = 1;

            //Act
            var result = sut.GetProduct(productId);

            //Assert
            var taskProduct = Assert.IsType<Task<Product>>(result);
            var expectedProduct = GetMockProducts().Where(x => x.Id == productId).First();
            var actualProduct = taskProduct.Result;
            Assert.Equal(expectedProduct, actualProduct, new ProductEqualityComparator());
        }


        [Fact]
        public void GetProduct_WithoutParams_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);

            //Act
            var result = sut.GetProduct();

            //Assert
            var taskProducts = Assert.IsType<Task<IList<Product>>>(result);
            var products = taskProducts.Result;
            Assert.Equal(GetMockProducts().ToList<Product>().Count, products.Count);
        }

        [Fact]
        public void GetAllProducts_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);

            //Act
            var result = sut.GetAllProducts();

            //Assert
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(result);
            var productsList = products as List<Product>;
            Assert.Equal(GetMockProducts().ToList<Product>().Count, productsList.Count);
        }

        [Fact]
        public void UpdateProductStocks_ProductStock_LessThanZero_ShouldCallCorrectMethod()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);

            //Act
            sut.UpdateProductStocks(4, 10);

            //Assert
            _mockDbSetProducts.Verify(x => x.Remove(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public void UpdateProductStocks_ProductStock_EqualToZero_ShouldCallCorrectMethod()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);

            //Act
            sut.UpdateProductStocks(4, 0);

            //Assert
            _mockDbSetProducts.Verify(x => x.Remove(It.IsAny<Product>()), Times.Once);
        }

        [Fact]
        public void UpdateProductStocks_ProductStock_GreaterThanZero_ShouldCallCorrectMethod()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);

            //Act
            sut.UpdateProductStocks(1, 10);

            //Assert
            _mockDbSetProducts.Verify(x => x.Update(It.IsAny<Product>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [Fact]
        public void SaveProduct_ProductIsNull_ShouldNotCallMethods()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);
            Product product = null;

            //Act
            sut.SaveProduct(product);

            //Assert
            _mockDbSetProducts.Verify(x => x.Add(It.IsAny<Product>()), Times.Never);
            _mockContext.Verify(x => x.SaveChanges(), Times.Never);
        }

        [Fact]
        public void SaveProduct_ProductIsNotNull_ShouldNotCallMethods()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);
            Product product = GetMockProducts().ToList<Product>().Find(x => x.Id == 1);
            
            //Act
            sut.SaveProduct(product);

            //Assert
            _mockDbSetProducts.Verify(x => x.Add(It.IsAny<Product>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        [Fact]
        public void DeleteProduct_ProductIsNull_ShouldNotCallMethods()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);

            //Act
            var productId = 10; //pass product Id that doesn't exist in mock dataset
            sut.DeleteProduct(productId);

            //Assert
            _mockDbSetProducts.Verify(x => x.Remove(It.IsAny<Product>()), Times.Never);
            _mockContext.Verify(x => x.SaveChanges(), Times.Never);
        }

        [Fact]
        public void DeleteProduct_ProductIsNotNull_ShouldNotCallMethods()
        {
            //Arrange
            var sut = new ProductRepository(_mockContext.Object);

            //Act
            var productId = 1; //pass product Id that exists in mock dataset
            sut.DeleteProduct(productId);

            //Assert
            _mockDbSetProducts.Verify(x => x.Remove(It.IsAny<Product>()), Times.Once);
            _mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }
    }
}
