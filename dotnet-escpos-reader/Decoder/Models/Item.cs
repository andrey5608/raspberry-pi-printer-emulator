using System.Collections.Generic;
using System.Linq;

namespace Decoder
{
    public class Item
    {
        public int Quantity;
        public string Name;
        public double Price;
        public double TotalPrice;
        List<Topping>  Toppings;

        public Item(int quantity, string name, double price, List<Topping> toppingList = null)
        {
            var sumOfToppings = 0.0;
            this.Name = name;
            this.Quantity = quantity;

            if (toppingList != null)
            {
                sumOfToppings = toppingList.Sum(topping => topping.Quantity * topping.Price);
            }

            this.Price = price + sumOfToppings;
            this.Toppings = toppingList;
        }

        public void AddTopping(Topping topping)
        {
            this.Toppings.Add(topping);
            this.TotalPrice = this.Price + topping.Price;
        }
    }
}
