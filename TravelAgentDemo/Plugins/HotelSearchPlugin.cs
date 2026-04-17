using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Newtonsoft.Json.Linq;
using SerpApi;

namespace TravelAgentDemo.Plugins;

public class HotelSearchPlugin
{
    private readonly string _apiKey;

    public HotelSearchPlugin(IConfiguration config)
    {
        _apiKey = config["SerpApi:ApiKey"] ?? string.Empty;
    }

    [KernelFunction, Description("Search for hotels in a city within a specific budget for certain dates")]
    public async Task<string> SearchHotelsAsync(
        [Description("City name to search for hotels in, e.g. Paris")] string city, 
        [Description("Check in date, e.g. 2026-05-15")] string checkInDate,
        [Description("Check out date, e.g. 2026-05-18. Optional, will default to 3 days after check in if missing")] string checkOutDate,
        [Description("Maximum budget per night in TND")] decimal maxPriceNightly)
    {
        if (string.IsNullOrEmpty(checkOutDate) && DateTime.TryParse(checkInDate, out DateTime ci))
        {
            checkOutDate = ci.AddDays(3).ToString("yyyy-MM-dd");
        }

        Console.WriteLine($"[Agent] Tool Call: HotelSearchPlugin.SearchHotelsAsync(city: \"{city}\", checkIn: {checkInDate}, checkOut: {checkOutDate}, maxPriceNightly: {maxPriceNightly})");
        
        if (string.IsNullOrEmpty(_apiKey))
        {
            return GetFallbackMockData(city);
        }

        Hashtable ht = new Hashtable
        {
            { "engine", "google_hotels" },
            { "q", city },
            { "check_in_date", checkInDate },
            { "check_out_date", checkOutDate },
            { "currency", "EUR" }, // SerpApi does not support TND directly
            { "hl", "en" }
        };

        try 
        {
            GoogleSearch search = new GoogleSearch(ht, _apiKey);
            JObject data = await Task.Run(() => search.GetJson());

            if (data["properties"] != null && data["properties"] is JArray properties)
            {
                var simplifiedHotels = properties
                    .Select(hotel => {
                        var priceObj = hotel["rate_per_night"];
                        string priceStr = priceObj?["lowest"]?.ToString() ?? "0";
                        
                        if (double.TryParse(priceStr.Replace("€", "").Trim(), out double eurPrice))
                        {
                            priceStr = (eurPrice * 3.4).ToString("F2"); // Convert EUR to TND
                        }

                        string hotelName = hotel["name"]?.ToString() ?? "Unknown Hotel";
                        double rating = 0;
                        if (double.TryParse(hotel["overall_rating"]?.ToString(), out double r)) rating = r;
                        
                        return new { 
                            name = hotelName,
                            price = priceStr, 
                            currency = "TND",
                            rating = rating
                        };
                    })
                    .Where(h => double.TryParse(h.price, out double p) && (decimal)p <= maxPriceNightly)
                    .Take(3);
                
                if (simplifiedHotels.Any())
                {
                    var jsonResponse = JsonSerializer.Serialize(simplifiedHotels);
                    System.IO.File.WriteAllText("hotel_debug.json", jsonResponse);
                    return jsonResponse;
                }
            }

            Console.WriteLine($"\n[Warning] No hotels found for {city} within budget via SerpApi. Using mock data.");
            return GetFallbackMockData(city);
        }
        catch(Exception ex)
        {
             Console.WriteLine($"\n[Warning] Hotel API Error: {ex.Message}. Using mock data.");
             return GetFallbackMockData(city);
        }
    }

    private string GetFallbackMockData(string city)
    {
        var hotels = new[] {
            new { name = $"Grand Hotel {city}", price = "150.00", currency = "TND", rating = 4.2 },
            new { name = $"Le Petit {city} Suites", price = "120.00", currency = "TND", rating = 3.9 }
        };
        var mockJson = JsonSerializer.Serialize(hotels);
        System.IO.File.WriteAllText("hotel_debug.json", mockJson);
        return mockJson;
    }
}