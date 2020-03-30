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
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.UnitTests.RepositoryTests
{
    public class OrderRepositoryTests
    {
        private readonly Mock<P3Referential> _mockContext;
        private readonly Mock<DbSet<Order>> _mockDbSetOrders;

        public OrderRepositoryTests()
        {
            _mockContext = new Mock<P3Referential>();

            _mockDbSetOrders = GetMockOrders().AsQueryable().BuildMockDbSet();

            _mockDbSetOrders.Setup(x => x.Add(It.IsAny<Order>())).Returns(It.IsAny<EntityEntry<Order>>());

            _mockContext.SetupGet(x => x.Order).Returns(_mockDbSetOrders.Object);
        }

        private IList<Order> GetMockOrders()
        {
            return new List<Order>()
            {
                new Order()
                {
                    Id = 1,
                    Address = "test address 1",
                    City = "test city 1",
                    Country = "test country 1",
                    Date = new DateTime(2020,03,16),
                    Name = "test name 1",
                    Zip = "test zip 1"
                },
                new Order()
                {
                    Id = 2,
                    Address = "test address 2",
                    City = "test city 2",
                    Country = "test country 2",
                    Date = new DateTime(2020,03,16),
                    Name = "test name 2",
                    Zip = "test zip 2"
                },
                new Order()
                {
                    Id = 3,
                    Address = "test address 3",
                    City = "test city 3",
                    Country = "test country 3",
                    Date = new DateTime(2020,03,16),
                    Name = "test name 3",
                    Zip = "test zip 3"
                }
            };
        }

        [Fact]
        public void Save_OrderIsNull_ShouldNotCallMethods()
        {
            //Arrange
            var sut = new OrderRepository(_mockContext.Object);
            Order order = null;

            //Act
            sut.Save(order);

            //Assert
            _mockDbSetOrders.Verify(x => x.Add(It.IsAny<Order>()), Times.Never);
        }

        [Fact]
        public void Save_OrderIsNotNull_ShouldCallMethods()
        {
            //Arrange
            var sut = new OrderRepository(_mockContext.Object);
            Order order = GetMockOrders().First();

            //Act
            sut.Save(order);

            //Assert
            _mockDbSetOrders.Verify(x => x.Add(It.IsAny<Order>()), Times.Once);
        }

        [Fact]
        public void GetOrder_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new OrderRepository(_mockContext.Object);
            var orderId = 1;
            //Act
            var returnedValue = sut.GetOrder(orderId);

            //Assert
            var taskOrder = Assert.IsType<Task<Order>>(returnedValue);
            Assert.NotNull(taskOrder.Result);
            Assert.Equal(taskOrder.Result, GetMockOrders().ToList().Find(x => x.Id == orderId), new OrderEqualityComparator());
        }

        [Fact]
        public void GetOrders_ShouldReturnCorrectValue()
        {
            //Arrange
            var sut = new OrderRepository(_mockContext.Object);

            //Act
            var result = sut.GetOrders();

            //Assert
            var taskOrderList = Assert.IsAssignableFrom<Task<IList<Order>>>(result);
            Assert.NotNull(taskOrderList.Result);
            Assert.Equal(GetMockOrders().Count, taskOrderList.Result.Count);
        }
    }
}
