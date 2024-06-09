using System.Diagnostics;

namespace Northwind.Services.EntityFramework.Entities
{
    [DebuggerDisplay("{Id}, {CompanyName}")]
    public class Shipper
    {
        public Shipper()
        {
            this.Orders = new HashSet<Order>();
        }

        public long Id { get; set; }
        public string CompanyName { get; set; } = default!;
        public string Phone { get; set; } = default!;

        public ICollection<Order> Orders { get; set; }
    }
}
