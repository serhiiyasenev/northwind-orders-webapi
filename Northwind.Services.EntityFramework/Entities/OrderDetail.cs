using System.Diagnostics;

namespace Northwind.Services.EntityFramework.Entities
{
    [DebuggerDisplay("{Order.Id}, {Product.Id}")]
    public class OrderDetail
    {
        public long OrderDetailId { get; set; }
        public long OrderId { get; set; }
        public long ProductId { get; set; }
        public double UnitPrice { get; set; }
        public long Quantity { get; set; }
        public double Discount { get; set; }

        public Order Order { get; set; } = default!;
        public Product Product { get; set; } = default!;
    }
}
