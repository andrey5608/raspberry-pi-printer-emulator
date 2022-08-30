using System.Collections.Generic;

namespace Decoder
{
    public class Order
    {
        public List<Item> Items;
        public Place Place;
        public string TotalAmount;

        public Order(List<Item> items, Place place, double totalAmount)
        {
            Items = items;
            Place = place;
            TotalAmount = $"{totalAmount:0.00}";
        }
    }
}
