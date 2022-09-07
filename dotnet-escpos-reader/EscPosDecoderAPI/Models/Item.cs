using System.Text.RegularExpressions;

namespace EscPosDecoderApi.Models
{
    public class Item
    {
        public int Quantity;
        public string Name;
        public double Price;
        public double TotalPrice;
        public List<Topping>? Toppings;
        private static readonly Random rnd = new Random();
        private static readonly object syncLock = new object();

        public Item(int quantity, string name, double price)
        {
            Name = Regex.Replace(name, @"\s+", " "); ;
            Quantity = quantity;

            Price = price;
            TotalPrice = price;
            Toppings = new List<Topping>();
        }

        public void AddTopping(Topping topping)
        {
            Toppings?.Add(topping);
            TotalPrice = Price + topping.Quantity * topping.Price;
        }

        public override string ToString()
        {
            var toppingsToString = Toppings is { Count: > 0 }
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
