using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

namespace P3AddNewFunctionalityDotNetCore.IntegrationTests
{
    public class OrderServiceIntegrationTests
    {
        private readonly List<Product> _testProductsList;
        private readonly List<Order> _testOrdersList;

        public OrderServiceIntegrationTests()
        {
            //mock products
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
        public void Test_All_Orders_Can_Be_Retrieved_From_Database()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            var result = (dynamic)null;
            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var orderRepository = new OrderRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var orderService = new OrderService(cart, orderRepository, productService);

                // Act
                result = orderService.GetOrders();

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Assert
            Assert.NotNull(result.Result);
            var orders = Assert.IsType<List<Order>>(result.Result);
            Assert.Equal(_testOrdersList.Count, orders.Count);
        }


        [Fact]
        public void Test_Order_Can_Be_Retrieved_From_Database_By_OrderId()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);
            var orderId = 2;

            var result = (dynamic)null;
            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                var productRepository = new ProductRepository(context);
                var orderRepository = new OrderRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var orderService = new OrderService(cart, orderRepository, productService);

                // Act
                result = orderService.GetOrder(orderId);

                //Cleanup
                context.Database.EnsureDeleted();
            }

            //Assert
            Assert.NotNull(result);
            var taskWithProduct = Assert.IsType<Task<Order>>(result);
            var order = Assert.IsType<Order>(taskWithProduct.Result) as Order;
            var expectedOrder = _testOrdersList.Find(x => x.Id == orderId);

            var doesDataMatch = expectedOrder.Name == order.Name
                                && expectedOrder.Address == order.Address
                                && expectedOrder.City == order.City
                                && expectedOrder.Date == order.Date
                                && expectedOrder.Country == order.Country;

            Assert.True(doesDataMatch);

        }


        [Fact]
        public void Test_Given_Cart_Contains_Product_ToCheckout_New_Order_Can_Be_Saved_To_Database_ShouldUpdateStockInProductsTable()
        {
            //Arrange
            var options = TestDbContextOptionsBuilder();
            SeedTestDb(options);

            //This product should exist in the Products table (It is part of initial data seed)
            var productPurchased = new Product()
            {
                Id = 2,
                Description = "test desc 2",
                Details = "test details 2",
                Name = "test product 2",
                Price = 20.10,
                Quantity = 200
            };

            //Prepare the order to add 
            var orderToAdd = new OrderViewModel()
            {
                Name = "new order",
                Address = "new address",
                City = "new City",
                Zip = "new zip",
                Country = "new country",
                Lines = new List<CartLine>()
                {
                    new CartLine()
                    {
                        Product = productPurchased,
                        Quantity = 10
                    }
                }
            };

            using (var context = new P3Referential(options))
            {
                var cart = new Cart();
                
                //Add the item to the cart
                cart.AddItem(productPurchased, 10);

                var productRepository = new ProductRepository(context);
                var orderRepository = new OrderRepository(context);
                var productService = new ProductService(cart, productRepository, null, null);
                var orderService = new OrderService(cart, orderRepository, productService);

                // Act
                orderService.SaveOrder(orderToAdd);
            }

            //Assert
            using (var context = new P3Referential(options))
            {
                var savedOrders = context.Order.Include(x => x.OrderLine).ToList();
                Assert.Equal(_testOrdersList.Count + 1,savedOrders.Count);
                Assert.IsAssignableFrom<List<Order>>(savedOrders);

                //get the most recent order which is just newly added (2 were pre-existing)
                var savedOrder = savedOrders.Find(x => x.Id == 3);

                //get the orderLine of order passed in for adding
                var orderToAddFirstLine = orderToAdd.Lines.First(x => x.Product.Id == 2);

                //get the orderLine of actual saved
                var savedOrderFirstLine = savedOrder.OrderLine.First(x => x.ProductId == 2);

                var doesDataMatch = orderToAdd.Address == savedOrder.Address
                    && orderToAdd.City == savedOrder.City
                    && orderToAdd.Country == savedOrder.Country
                    && orderToAdd.Lines.Count == savedOrder.OrderLine.Count
                    && orderToAddFirstLine.Product.Id == savedOrderFirstLine.ProductId
                    && orderToAddFirstLine.Quantity == savedOrderFirstLine.Quantity;

                Assert.True(doesDataMatch);

                //Check if product stock is reduced after the order
                Assert.Equal(200 - 10, context.Product.ToList().Find(x => x.Id == 2).Quantity);

                //Cleanup
                context.Database.EnsureDeleted();
            }
        }

    }
}
