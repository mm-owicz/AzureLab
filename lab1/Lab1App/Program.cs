using System;
using System.Linq;
using Lab1App.Data;
using Lab1App.Models;

namespace Lab1App
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new AppDBContext())
            {
                var products = context.Products.ToList();

                foreach (var product in products)
                {
                    Console.WriteLine($"ID: {product.Id}, Name: {product.Name}, Price: {product.Price}");
                }
            }
        }
    }
}
