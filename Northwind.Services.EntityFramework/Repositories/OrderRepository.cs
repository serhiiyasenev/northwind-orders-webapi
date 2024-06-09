using Microsoft.EntityFrameworkCore;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.Repositories;
using Order = Northwind.Services.EntityFramework.Entities.Order;
using OrderDetail = Northwind.Services.EntityFramework.Entities.OrderDetail;
using RepositoryOrder = Northwind.Services.Repositories.Order;

namespace Northwind.Services.EntityFramework.Repositories
{
    public sealed class OrderRepository : IOrderRepository
    {
        private readonly NorthwindContext _context;

        public OrderRepository(NorthwindContext context)
        {
            this._context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RepositoryOrder> GetOrderAsync(long orderId)
        {
            var order = await this._context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                throw new OrderNotFoundException($"Order with ID {orderId} not found.");
            }

            return MapToRepositoryOrder(order);
        }

        public async Task<IList<RepositoryOrder>> GetOrdersAsync(int skip, int count)
        {
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var orders = await this._context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .OrderBy(o => o.OrderId)
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            return orders.Select(MapToRepositoryOrder).ToList();
        }

        public async Task<long> AddOrderAsync(RepositoryOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            try
            {
                var entityOrder = MapToEntityOrder(order);
                this._context.Orders.Add(entityOrder);
                await this._context.SaveChangesAsync();
                return entityOrder.OrderId;
            }
            catch (DbUpdateException ex)
            {
                throw new RepositoryException("Error adding order.", ex);
            }
        }

        public async Task RemoveOrderAsync(long orderId)
        {
            var order = await this._context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                throw new OrderNotFoundException($"Order with ID {orderId} not found.");
            }

            this._context.Orders.Remove(order);
            await this._context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(RepositoryOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var existingOrder = await this._context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == order.Id);

            if (existingOrder == null)
            {
                throw new OrderNotFoundException($"Order with ID {order.Id} not found.");
            }

            this._context.Entry(existingOrder).CurrentValues.SetValues(MapToEntityOrder(order));
            existingOrder.OrderDetails.Clear();
            foreach (var orderDetail in order.OrderDetails)
            {
                existingOrder.OrderDetails.Add(MapToEntityOrderDetail(orderDetail));
            }

            await this._context.SaveChangesAsync();
        }

        private static RepositoryOrder MapToRepositoryOrder(Order order)
        {
            var repositoryOrder = new RepositoryOrder(order.OrderId)
            {
                Customer = new Services.Repositories.Customer(new CustomerCode(order.CustomerId.ToString()))
                {
                    CompanyName = order.Customer.CompanyName
                },
                Employee = new Services.Repositories.Employee(order.EmployeeId)
                {
                    FirstName = order.Employee.FirstName,
                    LastName = order.Employee.LastName,
                    Country = order.Employee.Country
                },
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                Shipper = new Services.Repositories.Shipper(order.ShipVia)
                {
                    CompanyName = order.Shipper.CompanyName
                },
                Freight = order.Freight,
                ShipName = order.ShipName,
                ShippingAddress = new ShippingAddress(
                    order.ShipAddress,
                    order.ShipCity,
                    order.ShipRegion,
                    order.ShipPostalCode,
                    order.ShipCountry
                )
            };

            foreach (var orderDetail in order.OrderDetails)
            {
                var product = new Services.Repositories.Product(orderDetail.ProductId)
                {
                    ProductName = orderDetail.Product.ProductName,
                    SupplierId = orderDetail.Product.SupplierId,
                    Supplier = orderDetail.Product.Supplier?.CompanyName ?? string.Empty,
                    CategoryId = orderDetail.Product.CategoryId,
                    Category = orderDetail.Product.Category?.CategoryName ?? string.Empty
                };

                var repositoryOrderDetail = new Services.Repositories.OrderDetail(repositoryOrder)
                {
                    Product = product,
                    UnitPrice = orderDetail.UnitPrice,
                    Quantity = orderDetail.Quantity,
                    Discount = orderDetail.Discount
                };

                repositoryOrder.OrderDetails.Add(repositoryOrderDetail);
            }

            return repositoryOrder;
        }


        private static Order MapToEntityOrder(RepositoryOrder order)
        {
            var entityOrder = new Order
            {
                OrderId = order.Id,
                CustomerId = long.Parse(order.Customer.Code.Code),
                EmployeeId = order.Employee.Id,
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                ShipVia = order.Shipper.Id,
                Freight = order.Freight,
                ShipName = order.ShipName,
                ShipAddress = order.ShippingAddress.Address,
                ShipCity = order.ShippingAddress.City,
                ShipRegion = order.ShippingAddress.Region!,
                ShipPostalCode = order.ShippingAddress.PostalCode,
                ShipCountry = order.ShippingAddress.Country
            };

            foreach (var orderDetail in order.OrderDetails)
            {
                entityOrder.OrderDetails.Add(MapToEntityOrderDetail(orderDetail));
            }

            return entityOrder;
        }

        private static OrderDetail MapToEntityOrderDetail(Services.Repositories.OrderDetail orderDetail)
        {
            return new OrderDetail
            {
                OrderId = orderDetail.Order.Id,
                ProductId = orderDetail.Product.Id,
                UnitPrice = orderDetail.UnitPrice,
                Quantity = orderDetail.Quantity,
                Discount = orderDetail.Discount
            };
        }
    }
}
