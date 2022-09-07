﻿using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using EscPosDecoderApi.Models;

namespace EscPosDecoderApi
{
    public static class Parser
    {
        private static Settings _settings = new ();
        public static void ParseReceiptData(string result, string merchantId, Settings settings)
        {
            try
            {
                _settings = settings;
                var items = new List<Item>();
                Place? place = null;
                double? total = null;
                var itemsSectionStart = false;
                var itemsSectionEnd = false;

                using (var reader = new StringReader(result))
                {
                    while (reader.ReadLine() is { } line)
                    {
                        place ??= HandleIfItIsAPlace(line);
                        // items parsing
                        var matchedItemSection = MatchItemsSection(line).Success;
                        switch (itemsSectionStart)
                        {
                            case true when matchedItemSection && items.Count > 0:
                                itemsSectionEnd = true;
                                continue;
                            case false:
                                itemsSectionStart = matchedItemSection;
                                break;
                            case true when !itemsSectionEnd:
                                items = AddIfItIsAnItem(line, items);
                                break;
                        }

                        total = total ?? HandleIfItIsATotal(line);
                    }
                    // here we can send the request
                }

                var order = new Order(items, place, total, merchantId);
                SendToDiditApi(order);
                // TODO send the order to DIDIT SQS
                Console.WriteLine(order.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);
            }
        }

        private static void SendToDiditApi(Order order)
        {
            try
            {
                var billAmount = Utils.ConvertToCents(order.TotalAmount);
                var parameters = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("merchantId", order.MerchantId),
                    new KeyValuePair<string, string>("placeUuid", _settings.PlaceUuid),
                    new KeyValuePair<string, string>("orderTime", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")),
                    new KeyValuePair<string, string>("billAmount", billAmount.ToString())
                };

                var i = 0;
                order.Items.ForEach(item =>
                {
                    parameters.Add(new KeyValuePair<string, string>($"items[{i}]",
                        item.ToBodyString()));
                    i++;
                });

                using var client = new HttpClient();
                var message = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{_settings.ApiHost}/order/test"),
                    Headers = {
                        { HttpRequestHeader.Authorization.ToString(), _settings.AuthToken },
                        { HttpRequestHeader.Accept.ToString(), "application/json" },
                        { HttpRequestHeader.ContentType.ToString(), "application/x-www-form-urlencoded"}
                    },
                    Content = new FormUrlEncodedContent(parameters)
                };


                var result = client.SendAsync(message).Result;
                var resultContent = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(resultContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);
            }

        }

        private static Place? HandleIfItIsAPlace(string line)
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
                Console.WriteLine(e.Message, e);
            }

            return null;
        }

        private static double? HandleIfItIsATotal(string line)
        {
            try
            {
                var itemMatch = MatchTotal(line);
                if (itemMatch.Success)
                {
                    var value = itemMatch.Groups[1].Value;
                    value = value.Contains(',') ? value.Replace(",", ".") : value;
                    double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var total);// TODO move out
                    Console.WriteLine($"Total: {value}");
                    return total;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);
            }

            return null;
        }

        private static List<Item> AddIfItIsAnItem(string line, List<Item> existingItems)
        {
            try
            {
                var matchItemsSection = MatchItemsSection(line);
                if (matchItemsSection.Success) { Console.WriteLine("Items section separator."); }
                var itemMatch = MatchItem(line);
                var toppingMatch = MatchTopping(line);
                switch (itemMatch.Success)
                {
                    case false when !toppingMatch.Success:
                        return existingItems;
                    case true when !toppingMatch.Success:
                        existingItems = MapItem(itemMatch, existingItems);
                        break;
                    case false when toppingMatch.Success:
                    case true when toppingMatch.Success:
                        existingItems = MapTopping(itemMatch, existingItems);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);
            }

            return existingItems;
        }

        private static List<Item> MapItem(Match itemMatch, List<Item> existingItems)
        {
            var itemName = string.Empty;
            var quantity = 1;
            var price = 0.00;
            try
            {
                Console.WriteLine($"Found item: {itemMatch.Groups[0].Value}");

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
                        case 3:
                            if (value.Contains(','))
                            {
                                value = value.Replace(",", ".");
                            }

                            if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out price))
                            {
                                value = price.ToString(CultureInfo.InvariantCulture);
                            }
                            break;
                    }

                    Console.WriteLine("Value: " + value);
                }

                Console.WriteLine($"Quantity: {quantity}; Item: {itemName}; Price: {price}");
                // here we can add to the list
                existingItems.Add(new Item(quantity, itemName, price));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);
            }

            return existingItems;
        }

        private static List<Item> MapTopping(Match toppingMatch, List<Item> existingItems)
        {
            var toppingName = string.Empty;
            var quantity = 1;
            var price = 0.00;
            try
            {
                Console.WriteLine($"Found topping: {toppingMatch.Groups[0].Value}");

                for (var i = 0; i < toppingMatch.Groups.Count; i++)
                {
                    var toppingMatchGroup = toppingMatch.Groups[i];
                    var value = toppingMatchGroup.Value.Trim();

                    switch (i)
                    {
                        // TODO adjust
                        case 1 when int.TryParse(value, out quantity):
                            value = quantity.ToString(CultureInfo.InvariantCulture);
                            break;
                        case 3 when double.TryParse(value, out price):
                            value = price.ToString(CultureInfo.InvariantCulture); // price
                            break;
                        default:
                        case 2 when value.StartsWith("x", true, CultureInfo.InvariantCulture):
                            value = toppingName =
                                value.Length > 1
                                    ? value[1..].Trim()
                                    : value; // remove x from the item name - TODO fix regexp
                            break;
                    }

                    Console.WriteLine("Value: " + value);
                }

                Console.WriteLine($"Quantity: {quantity}; Item: {toppingName}; Price: {price}");
                // here we can add to the list
                existingItems.LastOrDefault()?.AddTopping(new Topping(quantity, toppingName, price));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e);
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

        private static Match MatchTopping(string itemToCheck)
        {
            const string regExp = @"^[a-zA-Z\s]+$";// todo adjust regexp
            return Regex.Match(itemToCheck, regExp);
        }

        private static Match MatchItemsSection(string itemToCheck)
        {
            const string regExp = @"^[-]{25,}$";
            return Regex.Match(itemToCheck, regExp);
        }

        private static Match MatchTotal(string itemToCheck)
        {
            const string regExp = @"^.*Rechnungsbetrag[a-zA-Z\s]+(\d{1,}[\.,]\d{1,})$";
            return Regex.Match(itemToCheck, regExp);
        }
    }
}
