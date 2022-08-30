using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Decoder
{
    public static class Parser
    {
        public static void ParseEntities(string result)
        {
            var items = new List<Item>();
            var place = new Place();
            var total = 0.00;
            using (var reader = new StringReader(result))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    items = AddIfItIsAnItem(line, items);
                    place = HandleIfItIsAPlace(line);
                    // todo parse total
                }
                // here we can send the request
            }

            var order = new Order(items, place, total); // todo send order to DIDIT
        }

        private static Place HandleIfItIsAPlace(string line)
        {
            try
            {
                var itemMatch = MatchPlace(line);
                if (itemMatch.Success)
                {
                    var placeNumber = itemMatch.Groups[1].Value;
                    Console.WriteLine($"Place: {placeNumber}");
                    return new Place(placeNumber);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        private static List<Item> AddIfItIsAnItem(string line, List<Item> existingItems)
        {
            var itemName = string.Empty;
            var quantity = 1;
            var price = 0.00;
            try
            {
                var itemMatch = MatchItem(line);
                if (!itemMatch.Success) return existingItems;
                Console.WriteLine($"Found item: {line}");

                for (var i = 0; i < itemMatch.Groups.Count; i++)
                {
                    var itemMatchGroup = itemMatch.Groups[i];
                    var value = itemMatchGroup.Value.Trim();

                    switch (i)
                    {
                        case 1 when int.TryParse(value, out quantity):
                            value = quantity.ToString(CultureInfo.InvariantCulture);
                            break;
                        case 2 when value.StartsWith("x", true, CultureInfo.InvariantCulture):
                            value = itemName = value.Substring(1).Trim(); // remove x from the item name - TODO fix regexp
                            break;
                        case 3 when double.TryParse(value, out price):
                            value = price.ToString(CultureInfo.InvariantCulture); // price
                            break;
                    }

                    //Console.WriteLine("Value: " + value);
                }

                Console.WriteLine($"Quantity: {quantity}; Item: {itemName}; Price: {price}");
                // here we can add to the list
                existingItems.Add(new Item(quantity, itemName, price));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return existingItems;
        }

        private static Match MatchPlace(string itemToCheck)
        {
            const string regExp = @"^Tisch:\s{0,}([\d]+).*$";
            return Regex.Match(itemToCheck, regExp);
        }

        private static Match MatchItem(string itemToCheck)
        {
            const string regExp = @"^[.*\s]{0,}([\d]{1,2})[.*]{0,}\s{0,}([a-zA-Z]{1,}.*)\s{1,}(\d{1,}[\.,]\d{1,}).*$";
            return Regex.Match(itemToCheck, regExp);
        }
    }
}
