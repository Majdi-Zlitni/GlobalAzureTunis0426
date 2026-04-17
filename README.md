![Banner](./Goodbye%20Chatbots,%20Hello%20Agents%20Building%20The%20Doer%20AI%20on%20Azure%20Banner.png)

# Travel Agent Demo: Building "Doer" AI Agents with Semantic Kernel

Welcome to the companion code for the Global Azure session **"Goodbye Chatbots, Hello Agents."** This repository contains a fully functional, autonomous AI Travel Agent built in C# using Microsoft Semantic Kernel and Azure AI Foundry.

## 📋 Prerequisites & Requirements

To run this demo locally, you will need:
1. **.NET 8 SDK** installed on your machine.
2. **Azure AI Account** with a valid subscription and access to **Azure OpenAI**. You need a deployed Chat Completion model (like `gpt-4o`).
3. **Free Accounts** for external API integrations (SerpApi, OpenWeatherMap, Resend).

---

## 🔑 Step-by-Step: Getting Your API Keys

### 1. Azure OpenAI (The Agent's Brain)
1. Go to the [Azure Portal](https://portal.azure.com).
2. Start by creating an **Azure OpenAI** resource. 
3. Open **Azure AI Studio** and navigate to **Deployments**. Deploy a chat model (e.g., `gpt-4o`). Note down the **Deployment Name**.
4. Navigate back to your Azure OpenAI resource -> **Keys and Endpoint**. Copy `KEY 1` and the `Endpoint` URL.

### 2. SerpApi (Google Flights & Hotels)
1. Go to [SerpApi.com](https://serpapi.com/) and register for a free account.
2. Verify your email and log in to your **Dashboard**.
3. Copy your **Your Private API Key**. (This free tier allows up to 100 searches a month, perfect for the demo!)

### 3. OpenWeatherMap (Weather Data)
1. Go to [OpenWeatherMap.org](https://openweathermap.org/api) and sign up for a free account.
2. Go to your user profile dropdown -> **My API Keys**.
3. Generate a new API key and copy the resulting string.

### 4. Resend (Email Dispatching)
1. Go to [Resend.com](https://resend.com/) and sign up.
2. Navigate to **API Keys** on the sidebar.
3. Click **Create API Key**, grant it sending access, and copy the key.
*(Note: On the free tier, you can only dispatch emails to the email address you originally registered with!)*

---

## ⚙️ Setup & Configuration

1. Clone this repository and navigate to the project directory:
   ```bash
   cd TravelAgentDemo
   ```
2. You will find a file named `appsettings.example.json` inside this folder.
3. **Rename or copy** this file to `appsettings.json`.
4. Open your new `appsettings.json` and paste all your freshly acquired API keys:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://<your-resource-name>.openai.azure.com/",
    "Key": "<your-azure-openai-key>",
    "DeploymentName": "gpt-4o"
  },
  "SerpApi": {
    "ApiKey": "<your-serpapi-key>"
  },
  "OpenWeatherMap": {
    "ApiKey": "<your-openweathermap-key>"
  },
  "Resend": {
    "ApiKey": "<your-resend-key>"
  }
}
```

---

## 🚀 How to Run the App

1. Open your terminal (PowerShell, CMD, or bash).
2. Ensure you are in the directory containing `TravelAgentDemo.csproj` (e.g. `cd TravelAgentDemo`).
3. Build and launch the console application using the .NET CLI:
   ```bash
   dotnet run
   ```
4. Wait a few seconds for the kernel to initialize. You will be greeted by the `[You] To Travel Agent:` prompt!

---

## 💬 Example Prompt

To see the multi-step orchestration (Flights -> Weather -> Hotels -> Email) in action, try copying and pasting this query into the terminal:

> *"I'm planning a trip from Tunis (TUN) to Paris (CDG) leaving on May 15, 2026. Can you check flight options and find a hotel under 350 TND per night? Check the weather to see what I should pack, and please email the final itinerary to zlt.majdi@gmail.com."*
