using Microsoft.AspNetCore.Mvc;
using Northwind.Orders.WebApi.Models;
using Northwind.Services.Repositories;

namespace Northwind.Orders.WebApi.Controllers;

public sealed class OrdersController
{
    public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
    {
    }

    public Task<ActionResult<FullOrder>> GetOrderAsync(long orderId)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResult<IEnumerable<BriefOrder>>> GetOrdersAsync(int? skip, int? count)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResult<AddOrder>> AddOrderAsync(BriefOrder order)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResult> RemoveOrderAsync(long orderId)
    {
        throw new NotImplementedException();
    }

    public Task<ActionResult> UpdateOrderAsync(long orderId, BriefOrder order)
    {
        throw new NotImplementedException();
    }
}
