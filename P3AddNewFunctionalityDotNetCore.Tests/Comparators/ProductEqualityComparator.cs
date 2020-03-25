using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace P3AddNewFunctionalityDotNetCore.UnitTests.Comparators
{
    class ProductEqualityComparator : IEqualityComparer<Product>
    {
        public bool Equals(Product x, Product y)
        {
            if (x.Id == y.Id &&
               x.Name == y.Name &&
               x.Price == y.Price &&
               x.Quantity == y.Quantity &&
               x.Details == y.Details &&
               x.Description == y.Description)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(Product obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
