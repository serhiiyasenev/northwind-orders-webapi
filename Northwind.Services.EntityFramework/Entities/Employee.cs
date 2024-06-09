using System.Diagnostics;

namespace Northwind.Services.EntityFramework.Entities
{
    [DebuggerDisplay("{Id}, {FirstName}, {LastName}")]
    public class Employee
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string Title { get; set; } = default!;
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public string Address { get; set; } = default!;
        public string City { get; set; } = default!;
        public string Region { get; set; } = default!;
        public string PostalCode { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string HomePhone { get; set; } = default!;
        public string Extension { get; set; } = default!;
        public string Notes { get; set; } = default!;
        public long ReportsTo { get; set; }
    }
}
