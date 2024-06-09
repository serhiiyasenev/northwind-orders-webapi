using Microsoft.AspNetCore.Mvc;
using Northwind.Orders.WebApi.Models;
using Northwind.Services.Repositories;

namespace Northwind.Orders.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly ILogger<OrdersController> logger;

        public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
        {
            this.orderRepository = orderRepository;
            this.logger = logger;
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<FullOrder>> GetOrderAsync(long orderId)
        {
            try
            {
                var order = await this.orderRepository.GetOrderAsync(orderId);
                return this.Ok(MapToFullOrder(order));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving order with ID {OrderId}", orderId);
                return ex.Message.Contains("NotFound") ? new NotFoundResult() : new StatusCodeResult(500);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BriefOrder>>> GetOrdersAsync(int? skip, int? count)
        {
            try
            {
                var orders = await this.orderRepository.GetOrdersAsync(skip ?? 0, count ?? 10);
                return this.Ok(orders.Select(MapToBriefOrder));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving orders");
                return ex.Message.Contains("System.Exception") ? new StatusCodeResult(500) : new BadRequestResult();
            }
        }

        [HttpPost]
        public async Task<ActionResult<AddOrder>> AddOrderAsync(BriefOrder order)
        {
            try
            {
                var orderId = await this.orderRepository.AddOrderAsync(MapToRepositoryOrder(order));
                return this.Ok(new AddOrder { OrderId = orderId });
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error adding order");
                return new StatusCodeResult(500);
            }
        }

        [HttpDelete("{orderId}")]
        public async Task<ActionResult> RemoveOrderAsync(long orderId)
        {
            try
            {
                await this.orderRepository.RemoveOrderAsync(orderId);
                return this.NoContent();
            }
            catch (OrderNotFoundException)
            {
                return this.NotFound();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error removing order with ID {OrderId}", orderId);
                return new StatusCodeResult(500);
            }
        }

        [HttpPut("{orderId}")]
        public async Task<ActionResult> UpdateOrderAsync(long orderId, BriefOrder order)
        {
            if (orderId != order.Id)
            {
                return new BadRequestResult();
            }

            try
            {
                await this.orderRepository.UpdateOrderAsync(MapToRepositoryOrder(order));
                return new NoContentResult();
            }
            catch (OrderNotFoundException)
            {
                return new NotFoundResult();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error updating order with ID {OrderId}", orderId);
                return new StatusCodeResult(500);
            }
        }

        private static FullOrder MapToFullOrder(Order repositoryOrder)
        {
            return new FullOrder
            {
                Id = repositoryOrder.Id,
                Customer = new Models.Customer
                {
                    Code = repositoryOrder.Customer.Code.Code,
                    CompanyName = repositoryOrder.Customer.CompanyName
                },
                Employee = new Models.Employee
                {
                    Id = repositoryOrder.Employee.Id,
                    FirstName = repositoryOrder.Employee.FirstName,
                    LastName = repositoryOrder.Employee.LastName,
                    Country = repositoryOrder.Employee.Country
                },
                OrderDate = repositoryOrder.OrderDate,
                RequiredDate = repositoryOrder.RequiredDate,
                ShippedDate = repositoryOrder.ShippedDate,
                Shipper = new Models.Shipper
                {
                    Id = repositoryOrder.Shipper.Id,
                    CompanyName = repositoryOrder.Shipper.CompanyName
                },
                Freight = repositoryOrder.Freight,
                ShipName = repositoryOrder.ShipName,
                ShippingAddress = new Models.ShippingAddress
                {
                    Address = repositoryOrder.ShippingAddress.Address,
                    City = repositoryOrder.ShippingAddress.City,
                    Region = repositoryOrder.ShippingAddress.Region,
                    PostalCode = repositoryOrder.ShippingAddress.PostalCode,
                    Country = repositoryOrder.ShippingAddress.Country
                },
                OrderDetails = repositoryOrder.OrderDetails.Select(detail => new FullOrderDetail
                {
                    ProductId = detail.Product.Id,
                    ProductName = detail.Product.ProductName,
                    CategoryId = detail.Product.CategoryId,
                    CategoryName = detail.Product.Category,
                    SupplierId = detail.Product.SupplierId,
                    SupplierCompanyName = detail.Product.Supplier,
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                    Discount = detail.Discount
                }).ToList()
            };
        }

        private static BriefOrder MapToBriefOrder(Order repositoryOrder)
        {
            return new BriefOrder
            {
                Id = repositoryOrder.Id,
                CustomerId = repositoryOrder.Customer.Code.Code,
                EmployeeId = repositoryOrder.Employee.Id,
                OrderDate = repositoryOrder.OrderDate,
                RequiredDate = repositoryOrder.RequiredDate,
                ShippedDate = repositoryOrder.ShippedDate,
                ShipperId = repositoryOrder.Shipper.Id,
                Freight = repositoryOrder.Freight,
                ShipName = repositoryOrder.ShipName,
                ShipAddress = repositoryOrder.ShippingAddress.Address,
                ShipCity = repositoryOrder.ShippingAddress.City,
                ShipRegion = repositoryOrder.ShippingAddress.Region,
                ShipPostalCode = repositoryOrder.ShippingAddress.PostalCode,
                ShipCountry = repositoryOrder.ShippingAddress.Country,
                OrderDetails = repositoryOrder.OrderDetails.Select(detail => new BriefOrderDetail
                {
                    ProductId = detail.Product.Id,
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                    Discount = detail.Discount
                }).ToList()
            };
        }

        private static Order MapToRepositoryOrder(BriefOrder order)
        {
            var repositoryOrder = new Order(order.Id)
            {
                Customer = new Northwind.Services.Repositories.Customer(new Northwind.Services.Repositories.CustomerCode(order.CustomerId))
                {
                    CompanyName = order.CustomerId
                },
                Employee = new Northwind.Services.Repositories.Employee(order.EmployeeId),
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                Shipper = new Northwind.Services.Repositories.Shipper(order.ShipperId),
                Freight = order.Freight,
                ShipName = order.ShipName,
                ShippingAddress = new Northwind.Services.Repositories.ShippingAddress(
                    order.ShipAddress,
                    order.ShipCity,
                    order.ShipRegion,
                    order.ShipPostalCode,
                    order.ShipCountry
                )
            };

            foreach (var detail in order.OrderDetails)
            {
                var orderDetail = new Northwind.Services.Repositories.OrderDetail(repositoryOrder)
                {
                    Product = new Northwind.Services.Repositories.Product(detail.ProductId),
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                    Discount = detail.Discount
                };
                repositoryOrder.OrderDetails.Add(orderDetail);
            }

            return repositoryOrder;
        }
    }
}
