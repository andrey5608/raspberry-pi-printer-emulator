namespace EscPosDecoderApi.Models
{
    public class Order
    {
        public List<Item> Items;
        public Place Place;
        public double TotalAmount;
        public string MerchantId;

        public Order(List<Item> items, Place place, double? totalAmount, string merchantId)
        {
            Items = items;
            Place = place;
            TotalAmount = totalAmount ?? 0;
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
