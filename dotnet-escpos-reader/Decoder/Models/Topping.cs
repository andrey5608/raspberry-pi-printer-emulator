using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Decoder
{
    public class Topping
    {
        public int Quantity;
        public string Name;
        public double Price;

        public Topping(int quantity, string name, double price)
        {
            this.Name = name;
            this.Quantity = quantity;
            this.Price = price;
        }
    }
}
