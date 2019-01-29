using System;
using System.Collections.Generic;

namespace P3AddNewFunctionalityDotNetCore.Models.Entities
{
    public partial class Order
    {
        public Order()
        {
            OrderLine = new HashSet<OrderLine>();
        }

        public int Id { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Zip { get; set; }
        public virtual ICollection<OrderLine> OrderLine { get; set; }
    }
}
