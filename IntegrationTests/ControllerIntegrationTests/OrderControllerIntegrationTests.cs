using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Localization;
using Moq;
using P3AddNewFunctionalityDotNetCore.Controllers;
using P3AddNewFunctionalityDotNetCore.Data;
using P3AddNewFunctionalityDotNetCore.Models;
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.Repositories;
using P3AddNewFunctionalityDotNetCore.Models.Services;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace P3AddNewFunctionalityDotNetCore.IntegrationTests.ControllerIntegrationTests
{
    public class OrderControllerIntegrationTests
    {
        private readonly List<Product> _testProductsList;
        private readonly List<Order> _testOrdersList;
        private readonly Mock<IStringLocalizer<OrderController>> _mockLocalizer;

        public OrderControllerIntegrationTests()
        {
            _mockLocalizer = new Mock<IStringLocalizer<OrderController>>();

            _mockLocalizer.Setup(_ => _["CartEmpty"]).Returns(new LocalizedString("CartEmpty", "CartEmpty"));

            _testProductsList = new List<Product>()
            {
                new Product()
                {
                    Description = "test desc 1",
                    Details = "test details 1",
                    Name = "test product 1",
                    Price = 10.10,
                    Quantity = 100
                },
                new Product()
                {
                    Description = "test desc 2",
                    Details = "test details 2",
                    Name = "test product 2",
                    Price = 20.10,
                    Quantity = 200
                },
                new Product()
                {
                    Description = "test desc 3",
                    Details = "test details 3",
                    Name = "test product 3",
                    Price = 30.10,
                    Quantity = 300
                }
            };

            //mock orders
            _testOrdersList = new List<Order>()
            {
                new Order()
                {
                    Address = "test address",
                    City = "test city",
                    Country = "test country",
                    Date = new DateTime(2020,2,2),
                    Name = "test name",
                    Zip = "test zip",
                    OrderLine = new List<OrderLine>()
                    {
                        new OrderLine()
                        {
                            ProductId = 1,
                            Quantity = 10
                        },
                        new OrderLine()
                        {
                            ProductId = 2,
                            Quantity = 20
                        }
                    }
                },
                new Order()
                {
                    Address = "test address 2",
                    City = "test city 2",
                    Country = "test country 2",
                    Date = new DateTime(2019,5,5),
                    Name = "test name 2",
                    Zip = "test zip 2",
                    OrderLine = new List<OrderLine>()
                    {
                        new OrderLine()
                        {
                            ProductId = 2,
                            Quantity = 15
                        },
                        new OrderLine()
                        {
                            ProductId = 3,
                            Quantity = 25
                        }
                    }
                }
            };
        }


        private DbContextOptions<P3Referential> TestDbContextOptionsBuilder()
        {
            return new DbContextOptionsBuilder<P3Referential>()
                        .UseInMemoryDatabase(Guid.NewGuid().ToString(), new InMemoryDatabaseRoot()).Options;
        }
        private void SeedTestDb(DbContextOptions<P3Referential> options)
        {
            using (var context = new P3Referential(options))
            {
                //seed products
                foreach (var p in _testProductsList)
                {
                    context.Product.Add(p);
                }
                context.SaveChanges();

                //seed orders
                foreach (var o in _testOrdersList)
                {
                    context.Order.Add(o);
                }
                context.SaveChanges();
            }
        }

        [Fact]
        public void Post_Index_ValidOrder_ShouldBeSaved()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            var productToAdd = 1;
            var orderToAdd = new OrderViewModel()
            {
                Name = "new order",
                Address = "new address",
                City = "new City",
                Zip = "new zip",
                Country = "new country",
            };
            int totalOrderBefore = 0;
            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var orderRepository = new OrderRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var orderService = new OrderService(cart, orderRepository, productService);

                var orderController = new OrderController(cart, orderService, null);
                var cartController = new CartController(cart, productService);
                cartController.AddToCart(productToAdd);
                totalOrderBefore = context.Order.ToList().Count;
                // Act
                result = orderController.Index(orderToAdd);

            }

            //Verify that order was added
            using (var context = new P3Referential(options))
            {
                var orders = context.Order.ToList();
                int totalOrdersAfter = orders.Count;

                //Verify that order total is increased by one
                Assert.Equal(totalOrderBefore + 1, totalOrdersAfter);

                //Get the most recent order
                var order = orders.Last();
                var didDataMatch = orderToAdd.Name == order.Name
                    && orderToAdd.Address == order.Address
                    && orderToAdd.City == orderToAdd.City
                    && orderToAdd.Zip == orderToAdd.Zip
                    && orderToAdd.Country == orderToAdd.Country;
                Assert.True(didDataMatch);

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void Post_Index_InValidOrder_ShouldNotBeSaved()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;

            var orderToAdd = new OrderViewModel()
            {
                Name = "new order",
                Address = "new address",
                City = "new City",
                Zip = "new zip",
                Country = "new country",
            };

            int totalOrderBefore = 0;
            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var orderRepository = new OrderRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var orderService = new OrderService(cart, orderRepository, productService);

                var orderController = new OrderController(cart, orderService, _mockLocalizer.Object);
                var cartController = new CartController(cart, productService);
                totalOrderBefore = context.Order.ToList().Count;
                // Act
                result = orderController.Index(orderToAdd);

            }

            //Verify that order was added
            using (var context = new P3Referential(options))
            {
                var orders = context.Order.ToList();
                int totalOrdersAfter = orders.Count;

                //Verify that order total is not increased by one
                Assert.Equal(totalOrderBefore, totalOrdersAfter);

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public void Completed_ClearsTheCart()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var productToAdd = 1;

            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var orderRepository = new OrderRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var orderService = new OrderService(cart, orderRepository, productService);

                var orderController = new OrderController(cart, orderService, null);
                var cartController = new CartController(cart, productService);
                cartController.AddToCart(productToAdd);
                
                //Check that cart has a single item
                Assert.Single(cart.Lines);

                //Act
                orderController.Completed();

                //Verify that the cart gets cleared 
                Assert.Empty(cart.Lines);

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }
    }
}
