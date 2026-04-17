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

public class FlightSearchPlugin
{
    private readonly string _apiKey;

    public FlightSearchPlugin(IConfiguration config)
    {
        _apiKey = config["SerpApi:ApiKey"] ?? string.Empty;
    }

    [KernelFunction, Description("Search for available flights between two airports")]
    public async Task<string> SearchFlightsAsync(
        [Description("IATA code of the origin airport, e.g. TUN")] string origin,
        [Description("IATA code of the destination airport, e.g. CDG")] string destination,
        [Description("ISO date of departure, e.g. 2026-05-18")] string departureDate,
        [Description("Number of adult passengers")] int adults = 1)
    {
        Console.WriteLine($"[Agent] Tool Call: FlightSearchPlugin.SearchFlightsAsync(origin: {origin}, destination: {destination}, departureDate: {departureDate})");
        
        if (string.IsNullOrEmpty(_apiKey))
        {
            return GetFallbackMockData();
        }

        // Using the official SerpApi SDK
        Hashtable ht = new Hashtable
        {
            { "engine", "google_flights" },
            { "departure_id", origin },
            { "arrival_id", destination },
            { "outbound_date", departureDate },
            { "type", "2" }, // 2 = One-way flight
            { "currency", "EUR" }, // SerpApi does not support TND directly
            { "hl", "en" }
        };

        try 
        {
            // Execute the GoogleSearch search
            GoogleSearch search = new GoogleSearch(ht, _apiKey);
            
            // Run on a background thread so we don't block the UI thread since GetJson is sync
            JObject data = await Task.Run(() => search.GetJson());

            // Sometimes there are no flights, so "best_flights" won't exist. Let's catch that safely!
            if (data["best_flights"] != null && data["best_flights"] is JArray bestFlights)
            {
                var simplifiedFlights = bestFlights.Take(2).Select(flight => {
                    var priceStr = flight["price"]?.ToString() ?? "0";
                    if (double.TryParse(priceStr.Replace("€", "").Trim(), out double eurPrice))
                    {
                        priceStr = (eurPrice * 3.4).ToString("F2"); // Convert EUR to TND
                    }

                    var flightLegs = flight["flights"] as JArray;
                    
                    string airlineName = "Unknown Airline";
                    if (flightLegs != null && flightLegs.Count > 0)
                    {
                        airlineName = flightLegs[0]["airline"]?.ToString() ?? "Unknown Airline";
                    }

                    return new { 
                        price = new { total = priceStr, currency = "TND" }, 
                        airline = airlineName 
                    };
                });
                
                var jsonResponse = JsonSerializer.Serialize(simplifiedFlights);
                System.IO.File.WriteAllText("flight_debug.json", jsonResponse);
                return jsonResponse;
            }

            Console.WriteLine("\n[Warning] No 'best_flights' in SerpApi response. Using mock data.");
            // Return mock if the API sends back 0 real results (keeps demo alive)
            return GetFallbackMockData();
        }
        catch(SerpApiSearchException ex)
        {
             Console.WriteLine($"\n[Warning] SerpApi Error: {ex.Message}. Using mock data.");
             return GetFallbackMockData();
        }
        catch(Exception ex)
        {
             Console.WriteLine($"\n[Warning] Flight API Error: {ex.Message}. Using mock data.");
             return GetFallbackMockData();
        }
    }

    private string GetFallbackMockData()
    {
        var mockJson = JsonSerializer.Serialize(new[] {
            new { id = "1", price = new { total = "350.00", currency = "TND" }, airline = "Air France" },
            new { id = "2", price = new { total = "280.00", currency = "TND" }, airline = "Tunisair" }
        });
        System.IO.File.WriteAllText("flight_debug.json", mockJson);
        return mockJson;
    }
}
