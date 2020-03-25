using Moq;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<ICart> _mockCart;
        private readonly Mock<IOrderRepository> _mockOrderRepository;
        private readonly Mock<IProductService> _mockProductService;
        private delegate void MockOrderSave(Order order);
        public OrderServiceTests()
        {
            _mockCart = new Mock<ICart>();
            _mockOrderRepository = new Mock<IOrderRepository>();
            _mockOrderRepository.Setup(x => x.GetOrders()).Returns(Task.FromResult(GetMockOrders()));
            _mockOrderRepository.Setup(x => x.GetOrder(It.IsAny<int>()))
                                .Returns((int id) => Task.FromResult(GetMockOrders().FirstOrDefault(x => x.Id == id)));
            _mockProductService = new Mock<IProductService>();
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
        public async void GetOrder_ShouldCallCorrectMethod_WithCorrectArgument()
        {
            //Arrange
            var sut = new OrderService(_mockCart.Object, _mockOrderRepository.Object, _mockProductService.Object);

            //Act
            int orderId = 1;
            var returnedValue = await sut.GetOrder(orderId);

            //Assert
            Assert.IsType<Order>(returnedValue);
            Assert.Equal(1, returnedValue.Id);
            _mockOrderRepository.Verify(x => x.GetOrder(orderId), Times.Once);
        }

        [Fact]
        public async void GetOrders_ShouldCallCorrectMethod_WithCorrectReturnValue()
        {
            //Arrange
            var sut = new OrderService(_mockCart.Object, _mockOrderRepository.Object, _mockProductService.Object);

            //Act
            var returnedValue = await sut.GetOrders();

            //Assert
            Assert.IsAssignableFrom<IList<Order>>(returnedValue);
            Assert.Equal(3, returnedValue.Count);
        }

        [Fact]
        public void SaveOrder_ShouldCallCorrectMethod_WithCorrectArgument()
        {
            //Arrange
            var sut = new OrderService(_mockCart.Object, _mockOrderRepository.Object, _mockProductService.Object);
            var orderViewModel = new OrderViewModel()
            {
                OrderId = 1,
                Name = "test name",
                Address = "test address",
                City = "test city",
                Zip = "232323",
                Country = "test country",
                Date = new DateTime(2020, 2, 2),
                Lines = new List<CartLine>()
                {
                    new CartLine
                    {
                        OrderLineId = 1,
                        Product = new Product
                        {
                            Id = 1,
                            Description = "test product",
                            Details = "test details",
                            Name = "test name",
                            Price = 10.10,
                            Quantity = 10
                        },
                        Quantity = 1
                    }
                }
            };

           
            Order orderPassedAsArgument = null;
            _mockOrderRepository.Setup(x => x.Save(It.IsAny<Order>()))
                                .Callback(new MockOrderSave((Order order) => orderPassedAsArgument = order));
            
            //Act
            sut.SaveOrder(orderViewModel);

            //Assert
            _mockOrderRepository.Verify(x => x.Save(It.IsAny<Order>()), Times.Once);
            _mockCart.Verify(x => x.Clear(), Times.Once);
            _mockProductService.Verify(x => x.UpdateProductQuantities(), Times.Once);

            //Verify order object that gets passed as argument of Save method of OrderRepository
            Assert.NotNull(orderPassedAsArgument);
            Assert.Equal(orderViewModel.Address, orderPassedAsArgument.Address);
            Assert.Equal(orderViewModel.City, orderPassedAsArgument.City);
            Assert.Equal(orderViewModel.Country, orderPassedAsArgument.Country);
            Assert.Equal(orderViewModel.Name, orderPassedAsArgument.Name);
            Assert.Equal(orderViewModel.Zip, orderPassedAsArgument.Zip);
            Assert.Equal(DateTime.UtcNow.ToString("yyyyMMdd"), orderPassedAsArgument.Date.ToString("yyyyMMdd"));
            Assert.Equal(orderViewModel.Lines.Count, orderPassedAsArgument.OrderLine.Count);
        }
    }
}
