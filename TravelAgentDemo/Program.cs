using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TravelAgentDemo.Plugins;

namespace TravelAgentDemo;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=========================================================");
        Console.WriteLine("✈️  Welcome to the Azure AI Travel Agent");
        Console.WriteLine("=========================================================\n");

        // 1. Load Configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets<Program>() // Optional: For local dev safety
            .Build();

        // 2. Initialize Semantic Kernel
        var builder = Kernel.CreateBuilder();

        
        // Ensure configuration is valid before adding Azure OpenAI Service
        if(string.IsNullOrEmpty(config["AzureOpenAI:Endpoint"]) || string.IsNullOrEmpty(config["AzureOpenAI:Key"]))
        {
            Console.WriteLine("❌ API Keys not found. Please fill in appsettings.json.");
            return;
        }

        builder.AddAzureOpenAIChatCompletion(
            deploymentName: config["AzureOpenAI:DeploymentName"]!,
            endpoint: config["AzureOpenAI:Endpoint"]!,
            apiKey: config["AzureOpenAI:Key"]!
        );

        // 3. Register Plugins
        var http = new HttpClient();
        builder.Plugins.AddFromObject(new FlightSearchPlugin(config));
        builder.Plugins.AddFromObject(new WeatherPlugin(config, http));
        builder.Plugins.AddFromObject(new HotelSearchPlugin(config));
        builder.Plugins.AddFromObject(new EmailPlugin(config));

        var kernel = builder.Build();
        var chatService = kernel.GetRequiredService<IChatCompletionService>();

        Console.WriteLine("✅ Kernel ready!");
        
        // 4. Set Execution Settings for Function Calling
        var settings = new OpenAIPromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var history = new ChatHistory();
        
       
        Console.WriteLine("---------------------------------------------------------");

        while (true)
        {
            Console.Write("\n[You] To Travel Agent: ");
            var userRequest = Console.ReadLine();
            
            // Exit if user types empty string or "exit"
            if (string.IsNullOrWhiteSpace(userRequest) || userRequest.ToLower() == "exit") 
                break;
                
            history.AddUserMessage(userRequest);

            Console.WriteLine("\n[Agent is working...]");
            var result = await chatService.GetChatMessageContentAsync(history, settings, kernel);

            Console.WriteLine("\n📩 Agent Reply:");
            Console.WriteLine(result.Content);
            
            history.AddAssistantMessage(result.Content);
            Console.WriteLine("---------------------------------------------------------");
        }
    }
}
