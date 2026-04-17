using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using Resend;

namespace TravelAgentDemo.Plugins;

public class EmailPlugin
{
    private readonly string _apiKey;

    public EmailPlugin(IConfiguration config)
    {
        _apiKey = config["Resend:ApiKey"] ?? string.Empty;
    }

    [KernelFunction, Description("Draft and send the travel itinerary email to the user")]
    public async Task<string> SendItineraryEmailAsync(
        [Description("Email address to send to, e.g. zlt.majdi@gmail.com")] string toEmail,
        [Description("Subject line of the email")] string subject,
        [Description("Full HTML body of the travel itinerary")] string htmlBody)
    {
        Console.WriteLine($"[Agent] Tool Call: Drafting and Sending Email to \"{toEmail}\" with subject \"{subject}\"...");
        
        if (string.IsNullOrEmpty(_apiKey))
            return $"Email mock-sent to {toEmail} with subject '{subject}'";

        try
        {
            IResend resend = ResendClient.Create(_apiKey);

            var msg = new EmailMessage()
            {
                From = "onboarding@resend.dev",
                To = toEmail,
                Subject = subject,
                HtmlBody = htmlBody,
            };

            var resp = await resend.EmailSendAsync(msg);
            
            Console.WriteLine($"[Agent] ✅ Tool Success: Email delivered via Resend. ID: {resp}");
            return $"Email sent successfully to {toEmail} via Resend.";
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[Agent] ❌ Tool Email Failed: {ex.Message}");
            return $"Failed to send email. Error: {ex.Message}";
        }
    }
}