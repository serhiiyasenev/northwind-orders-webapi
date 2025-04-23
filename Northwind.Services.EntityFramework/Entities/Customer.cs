using System.Diagnostics;

namespace Northwind.Services.EntityFramework.Entities
{
    [DebuggerDisplay("{CustomerId}, {CompanyName}")]
    public class Customer
    {
        public string? CustomerId { get; set; }

        public string? CompanyName { get; set; } = default!;

        public string? ContactName { get; set; } = default!;

        public string? ContactTitle { get; set; } = default!;

        public string? Address { get; set; } = default!;

        public string? City { get; set; } = default!;

        public string? Region { get; set; } = default!;

        public string? PostalCode { get; set; } = default!;

        public string? Country { get; set; } = default!;

        public string? Phone { get; set; } = default!;

        public string? Fax { get; set; } = default!;
    }
}
