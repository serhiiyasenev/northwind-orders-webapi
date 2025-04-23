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
        private readonly NorthwindContext context;

        public OrderRepository(NorthwindContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RepositoryOrder> GetOrderAsync(long orderId)
        {
            var order = await this.context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Supplier)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                throw new OrderNotFoundException($"Order with ID {orderId} not found.");
            }

            return MapToRepositoryOrder(order);
        }

        public async Task<IList<RepositoryOrder>> GetOrdersAsync(int skip, int count)
        {
            VerifyGetOrdersRequest(skip, count);

            var orders = await this.context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Employee)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Supplier)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .ThenInclude(p => p.Category)
                .OrderBy(o => o.OrderId)
                .Skip(skip)
                .Take(count)
                .ToListAsync();

            return orders.Select(MapToRepositoryOrder).ToList();
        }

        public async Task<long> AddOrderAsync(RepositoryOrder order)
        {
            VerifyAddOrderRequest(order);

            try
            {
                var entityOrder = this.MapToEntityOrder(order);
                this.context.Orders.Add(entityOrder);

                if (entityOrder.OrderDetails.Any(orderDetail => orderDetail.ProductId <= 0))
                {
                    throw new RepositoryException("Invalid ProductId in OrderDetail.");
                }

                await this.context.SaveChangesAsync();
                return entityOrder.OrderId;
            }
            catch (DbUpdateException ex)
            {
                throw new RepositoryException("Error adding order.", ex);
            }
            catch (Exception ex)
            {
                throw new RepositoryException("An error occurred while adding the order.", ex);
            }
        }

        public async Task RemoveOrderAsync(long orderId)
        {
            var order = await this.context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            VerifyRemoveOrderRequest(order!, orderId);

            this.context.Orders.Remove(order!);
            await this.context.SaveChangesAsync();
        }

        public async Task UpdateOrderAsync(RepositoryOrder order)
        {
            VerifyUpdateOrderRequest(order);

            Order? existingOrder = await this.context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == order.Id);

            VerifyExistingOrder(existingOrder, order.Id);

            existingOrder!.CustomerId = order.Customer.Code.Code;
            existingOrder.EmployeeId = order.Employee.Id;
            existingOrder.OrderDate = order.OrderDate;
            existingOrder.RequiredDate = order.RequiredDate;
            existingOrder.ShippedDate = order.ShippedDate;
            existingOrder.ShipVia = order.Shipper.Id;
            existingOrder.Freight = order.Freight;
            existingOrder.ShipName = order.ShipName;
            existingOrder.ShipAddress = order.ShippingAddress.Address;
            existingOrder.ShipCity = order.ShippingAddress.City;
            existingOrder.ShipRegion = order.ShippingAddress.Region!;
            existingOrder.ShipPostalCode = order.ShippingAddress.PostalCode;
            existingOrder.ShipCountry = order.ShippingAddress.Country;

            this.context.OrderDetails.RemoveRange(existingOrder.OrderDetails);

            foreach (var orderDetail in order.OrderDetails)
            {
                existingOrder.OrderDetails.Add(this.MapToEntityOrderDetail(orderDetail));
            }

            await this.context.SaveChangesAsync();
        }

        private static void VerifyGetOrdersRequest(int skip, int count)
        {
            if (skip < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(skip));
            }

            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        private static void VerifyAddOrderRequest(RepositoryOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
        }

        private static void VerifyRemoveOrderRequest(Order order, long orderId)
        {
            if (order == null)
            {
                throw new OrderNotFoundException($"Order with ID {orderId} not found.");
            }
        }

        private static void VerifyUpdateOrderRequest(RepositoryOrder order)
        {
            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }
        }

        private static void VerifyExistingOrder(Order? existingOrder, long orderId)
        {
            if (existingOrder == null)
            {
                throw new OrderNotFoundException($"Order with ID {orderId} not found.");
            }
        }

        private static RepositoryOrder MapToRepositoryOrder(Order order)
        {
            var repositoryOrder = new RepositoryOrder(order.OrderId)
            {
                Customer = new Services.Repositories.Customer(new CustomerCode(order.CustomerId!))
                {
                    CompanyName = order.Customer.CompanyName!,
                },
                Employee = new Services.Repositories.Employee(order.EmployeeId)
                {
                    FirstName = order.Employee.FirstName!,
                    LastName = order.Employee.LastName!,
                    Country = order.Employee.Country!,
                },
                OrderDate = order.OrderDate,
                RequiredDate = order.RequiredDate,
                ShippedDate = order.ShippedDate,
                Shipper = new Services.Repositories.Shipper(order.ShipVia)
                {
                    CompanyName = order.Shipper.CompanyName!,
                },
                Freight = order.Freight,
                ShipName = order.ShipName!,
                ShippingAddress = new ShippingAddress(
                    order.ShipAddress!,
                    order.ShipCity!,
                    order.ShipRegion,
                    order.ShipPostalCode!,
                    order.ShipCountry!),
            };

            foreach (var orderDetail in order.OrderDetails)
            {
                var product = new Services.Repositories.Product(orderDetail.ProductId)
                {
                    ProductName = orderDetail.Product.ProductName!,
                    SupplierId = orderDetail.Product.SupplierId,
                    Supplier = orderDetail.Product.Supplier?.CompanyName ?? string.Empty,
                    CategoryId = orderDetail.Product.CategoryId,
                    Category = orderDetail.Product.Category?.CategoryName ?? string.Empty,
                };

                var repositoryOrderDetail = new Services.Repositories.OrderDetail(repositoryOrder)
                {
                    Product = product,
                    UnitPrice = orderDetail.UnitPrice,
                    Quantity = orderDetail.Quantity,
                    Discount = orderDetail.Discount,
                };

                repositoryOrder.OrderDetails.Add(repositoryOrderDetail);
            }

            return repositoryOrder;
        }

        private Order MapToEntityOrder(RepositoryOrder order)
        {
            var entityOrder = new Order
            {
                CustomerId = order.Customer.Code.Code,
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
                ShipCountry = order.ShippingAddress.Country,
            };

            foreach (var orderDetail in order.OrderDetails)
            {
                entityOrder.OrderDetails.Add(this.MapToEntityOrderDetail(orderDetail));
            }

            return entityOrder;
        }

        private OrderDetail MapToEntityOrderDetail(Services.Repositories.OrderDetail orderDetail)
        {
            var product = this.context.Products
                .Include(p => p.Supplier)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ProductId == orderDetail.Product.Id);

            if (product == null)
            {
                product = new Entities.Product
                {
                    ProductId = orderDetail.Product.Id,
                    ProductName = orderDetail.Product.ProductName,
                    SupplierId = orderDetail.Product.SupplierId,
                    Supplier = new Supplier
                    {
                        SupplierId = orderDetail.Product.SupplierId,
                        CompanyName = orderDetail.Product.Supplier,
                    },
                    CategoryId = orderDetail.Product.CategoryId,
                    Category = new Category
                    {
                        CategoryId = orderDetail.Product.CategoryId,
                        CategoryName = orderDetail.Product.Category,
                    },
                };
            }
            else
            {
                if (product.Supplier == null || product.Supplier.CompanyName != orderDetail.Product.Supplier)
                {
                    product.Supplier = new Supplier
                    {
                        SupplierId = orderDetail.Product.SupplierId,
                        CompanyName = orderDetail.Product.Supplier,
                    };
                }

                if (product.Category == null || product.Category.CategoryName != orderDetail.Product.Category)
                {
                    product.Category = new Category
                    {
                        CategoryId = orderDetail.Product.CategoryId,
                        CategoryName = orderDetail.Product.Category,
                    };
                }
            }

            return new OrderDetail
            {
                OrderId = orderDetail.Order.Id,
                ProductId = product.ProductId,
                UnitPrice = orderDetail.UnitPrice,
                Quantity = orderDetail.Quantity,
                Discount = orderDetail.Discount,
                Product = product,
            };
        }
    }
}
