using System.Diagnostics;

namespace Northwind.Services.EntityFramework.Entities
{
    [DebuggerDisplay("{CategoryId}, {CategoryName}")]
    public class Category
    {
        public Category()
        {
            this.Products = new HashSet<Product>();
        }

        public long? CategoryId { get; set; }

        public string? CategoryName { get; set; } = default!;

        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
