using System.Diagnostics;

namespace Northwind.Services.EntityFramework.Entities
{
    [DebuggerDisplay("{ProductId}, {ProductName}")]
    public class Product
    {
        public Product()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
        }

        public long ProductId { get; set; }
        public string? ProductName { get; set; } = default!;
        public long SupplierId { get; set; }
        public Supplier Supplier { get; set; } = default!;
        public long CategoryId { get; set; }
        public Category Category { get; set; } = default!;
        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
