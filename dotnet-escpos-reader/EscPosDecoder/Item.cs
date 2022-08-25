namespace Decoder
{
    public class Item
    {
        public int Quantity;
        public string Name;
        public double Price;

        public Item(int quantity, string name, double price)
        {
            this.Name = name;
            this.Quantity = quantity;
            this.Price = price;
        }
    }
}
