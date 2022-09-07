using System.Text.RegularExpressions;

namespace EscPosDecoderApi.Models
{
    public class Topping
    {
        public int Quantity;
        public string Name;
        public double Price;

        public Topping(int quantity, string name, double price)
        {
            Name = Regex.Replace(name, @"\s+", " ");
            Quantity = quantity;
            Price = price;
        }
    }
}
