
using P3AddNewFunctionalityDotNetCore.Models.Entities;
using System.Collections.Generic;

namespace P3AddNewFunctionalityDotNetCore.Models
{
    public interface ICart
    {
        void AddItem(Product product, int quantity);

        void RemoveLine(Product product);

        void Clear();

        double GetTotalValue();

        double GetAverageValue();
        IEnumerable<CartLine> Lines { get;}
    }
}