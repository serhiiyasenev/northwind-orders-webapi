using System.Diagnostics;

namespace Northwind.Services.EntityFramework.Entities
{
    [DebuggerDisplay("{Order.OrderId}, {Product.ProductId}")]
    public class OrderDetail
    {
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public double UnitPrice { get; set; }
        public long Quantity { get; set; }
        public double Discount { get; set; }

        public Order Order { get; set; } = default!;
        public Product Product { get; set; } = default!;
    }
}
