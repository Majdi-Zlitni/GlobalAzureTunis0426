using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace TravelAgentDemo.Plugins;

public class WeatherPlugin
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public WeatherPlugin(IConfiguration config, HttpClient http)
    {
        _http = http;
        _apiKey = config["OpenWeatherMap:ApiKey"] ?? string.Empty;
    }

    [KernelFunction, Description("Get weather forecast for a city on specific dates")]
    public async Task<string> GetWeatherAsync(
        [Description("City name to search weather for, e.g. Paris")] string city, 
        [Description("ISO date for the forecast, e.g. 2026-05-18")] string date)
    {
        Console.WriteLine($"[Agent] Tool Call: WeatherPlugin.GetWeatherAsync(city: \"{city}\", date: \"{date}\")");
        
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Mock response if key is missing
            return $"{city}: Scattered clouds, 19°C (Mock Data)";
        }

        var url = $"https://api.openweathermap.org/data/2.5/forecast?q={city}&appid={_apiKey}&units=metric&cnt=5";
        
        try
        {
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return $"{city}: Weather unavailable";
                
            var result = await response.Content.ReadFromJsonAsync<JsonDocument>();
            var list = result?.RootElement.GetProperty("list");
            if (list == null || list.Value.GetArrayLength() == 0) return $"{city}: Weather unavailable";
            
            var first = list.Value[0];
            var temp = first.GetProperty("main").GetProperty("temp").GetDouble();
            var desc = first.GetProperty("weather")[0].GetProperty("description").GetString();

            return $"{city}: {desc}, {temp}°C";
        }
        catch(Exception)
        {
            return $"{city}: Weather unavailable";
        }
    }
}