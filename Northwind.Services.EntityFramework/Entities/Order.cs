using System.Diagnostics;

namespace Northwind.Services.EntityFramework.Entities
{
    [DebuggerDisplay("Order #{OrderId}")]
    public class Order
    {
        public Order()
        {
            this.OrderDetails = new HashSet<OrderDetail>();
        }

        public long OrderId { get; set; }
        public string CustomerId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public DateTime? ShippedDate { get; set; }
        public long ShipVia { get; set; }
        public double Freight { get; set; }
        public string? ShipName { get; set; } = default!;
        public string? ShipAddress { get; set; } = default!;
        public string? ShipCity { get; set; } = default!;
        public string? ShipRegion { get; set; } = default!;
        public string? ShipPostalCode { get; set; } = default!;
        public string? ShipCountry { get; set; } = default!;

        public Customer Customer { get; set; } = default!;
        public Employee Employee { get; set; } = default!;
        public Shipper Shipper { get; set; } = default!;

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
