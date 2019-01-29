using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using P3AddNewFunctionalityDotNetCore.Models.Entities;

namespace P3AddNewFunctionalityDotNetCore.Data
{
    public class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new P3Referential(
                serviceProvider.GetRequiredService<Microsoft.EntityFrameworkCore.DbContextOptions<P3Referential>>()))
            {
                if (context.Product.Any())
                {
                    return; 
                }

                context.Product.AddRange(
                     new Product
                     {
                         Name = "Echo Dot",
                         Description = "(2nd Generation) - Black",
                         Quantity = 10,
                         Price = 92.50
                     },

                     new Product
                     {
                         Name = "Anker 3ft / 0.9m Nylon Braided",
                         Description = "Tangle-Free Micro USB Cable",
                         Quantity = 20,
                         Price = 9.99
                     },

                     new Product
                     {
                         Name = "JVC HAFX8R Headphone",
                         Description = "Riptidz, In-Ear",
                         Quantity = 30,
                         Price = 69.99
                     },

                   new Product
                   {
                       Name = "VTech CS6114 DECT 6.0",
                       Description = "Cordless Phone",
                       Quantity = 40,
                       Price = 32.50
                   },

                   new Product
                   {
                       Name = "NOKIA OEM BL-5J",
                       Description = "Cell Phone",
                       Quantity = 50,
                       Price = 895.00
                   }
                );
                context.SaveChanges();
            }
        }
    }
}
