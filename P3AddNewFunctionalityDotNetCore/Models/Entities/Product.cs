using System.Collections.Generic;

namespace P3AddNewFunctionalityDotNetCore.Models.Entities
{
    public partial class Product
    {
        public Product()
        {
            OrderLine = new HashSet<OrderLine>();
        }

        public int Id { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }

        public virtual ICollection<OrderLine> OrderLine { get; set; }
    }
}
