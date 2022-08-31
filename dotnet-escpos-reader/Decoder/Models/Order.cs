using System.Collections.Generic;
using System.Linq;

namespace Decoder.Models
{
    public class Order
    {
        public List<Item> Items;
        public Place Place;
        public string TotalAmount;
        public string MerchantId;

        public Order(List<Item> items, Place place, double? totalAmount, string merchantId)
        {
            Items = items;
            Place = place;
            TotalAmount = totalAmount != null ? $"{totalAmount:N2}" : "0.00";
            MerchantId = merchantId;
        }

        public override string ToString()
        {
            var itemsToString = Items != null && Items.Count > 0
                ? string.Join("; ", Items.Select(x => x.ToString()).ToArray())
                : "[]";
            return
                $"Merchant Id: {MerchantId}; Place: {Place?.Number}; Total: {TotalAmount}; Items: {itemsToString}";
        }
    }
}
