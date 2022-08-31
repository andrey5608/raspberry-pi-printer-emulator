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
    }
}
