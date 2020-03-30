using P3AddNewFunctionalityDotNetCore.Models.Entities;
using P3AddNewFunctionalityDotNetCore.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace P3AddNewFunctionalityDotNetCore.UnitTests.Comparators
{
    public class OrderEqualityComparator : IEqualityComparer<Order>
    {
        public bool Equals(Order x, Order y)
        {
            if (x.Id == y.Id &&
               x.Address == y.Address &&
               x.City == y.City &&
               x.Country == y.Country &&
               x.Name == y.Name &&
               x.Zip == y.Zip)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode(Order obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
