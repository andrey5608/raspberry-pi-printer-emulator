using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Decoder.Models
{
    public class Item
    {
        public int Quantity;
        public string Name;
        public double Price;
        public double TotalPrice;
        public List<Topping> Toppings;
        private static readonly Random rnd = new Random();
        private static readonly object syncLock = new object();

        public Item(int quantity, string name, double price, List<Topping> toppingList = null)
        {
            var sumOfToppings = 0.0;
            Name = Regex.Replace(name, @"\s+", " "); ;
            Quantity = quantity;

            if (toppingList != null)
            {
                sumOfToppings = toppingList.Sum(topping => topping.Quantity * topping.Price);
            }

            Price = price;
            TotalPrice = price + sumOfToppings;
            Toppings = toppingList;
        }

        public void AddTopping(Topping topping)
        {
            Toppings?.Add(topping);
            TotalPrice = Price + topping.Price;
        }

        public override string ToString()
        {
            var toppingsToString = Toppings != null && Toppings.Count > 0
                ? string.Join("; ", Toppings.Select(x => x.Name).ToArray())
                : "[]";
            return
                $"'{Name}': Quantity: {Quantity}; Price: {Price}; Total price: {TotalPrice}; Toppings: {toppingsToString} ";
        }

        public string ToBodyString()
        {
            // TODO add toppings
            return $"{{\"id\": \"{RandomNumber(1, 999)}\", \"posId\": {RandomNumber(1, 999)}, \"name\": \"{Name}\", \"price\": {Utils.ConvertToCents(Price)}, \"toppings\": []}}";
        }

        private static int RandomNumber(int min, int max)
        {
            lock (syncLock)
            { // synchronize
                return rnd.Next(min, max);
            }
        }
    }
}
