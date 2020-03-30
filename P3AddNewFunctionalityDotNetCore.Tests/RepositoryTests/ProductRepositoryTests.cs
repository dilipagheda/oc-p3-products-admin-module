using Microsoft.EntityFrameworkCore;
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

    }
}
