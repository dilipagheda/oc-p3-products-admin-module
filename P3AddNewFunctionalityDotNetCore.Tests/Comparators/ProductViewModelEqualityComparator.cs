using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace P3AddNewFunctionalityDotNetCore.UnitTests.Comparators
{
    public class ProductViewModelEqualityComparator : IEqualityComparer<ProductViewModel>
    {
        public bool Equals(ProductViewModel x, ProductViewModel y)
        {
            if(x.Id == y.Id && 
               x.Name == y.Name &&
               x.Price == y.Price &&
               x.Stock == y.Stock &&
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

        public int GetHashCode(ProductViewModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
