#define FATAL2CONSOLE
namespace openai_csharp_example;
using System;
using System.IO;
using Microsoft.Extensions.Configuration;

internal class Program
{

    // Add this method somewhere in your class
    private static void LogFatalError(Exception ex)
    {
        try
        {
            // Create or append to a log file
            const string logPath = "error_log.txt";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timestamp}] FATAL ERROR: {ex.Message}\r\n";
            logMessage += $"Stack Trace: {ex.StackTrace}\r\n";

            if (ex.InnerException != null)
            {
                logMessage += $"Inner Exception: {ex.InnerException.Message}\r\n";
            }

            logMessage += new string('-', 80) + "\r\n";

            // Write to log file
            File.AppendAllText(logPath, logMessage);

#if FATAL2CONSOLE
            Console.Out.WriteLineAsync(logMessage);
#endif

            // Optionally log to EventLog for Windows applications
            // System.Diagnostics.EventLog.WriteEntry("YourApplicationName", logMessage, 
            //                                        System.Diagnostics.EventLogEntryType.Error);
        }
        catch
        {
            // If logging itself fails, at least try to write to console
            Console.WriteLine("Failed to write to error log.");
        }
    }
    private static async Task Main(string[] args)
    {
        try
        {
            // Your application code herBe
            var builder = new ConfigurationBuilder()
                    .AddUserSecrets<Program>();

            var configuration = builder.Build();
            var apiKey = configuration["OpenAI:ApiKey"];

            if (apiKey is null)
            {
                throw new Exception(
                    @"[OpenAI: ApiKey] not found in configuration
                      before building must do once:
                      dotnet user-secrets set'OpenAI:ApiKey' <Your OpenAI API Key>"
                    );
            }

            { using var openAIService = OpenAIServiceKLIENT.Create(apiKey);

                var chatSession = new ChatSession();

                while (true)
                {
                    Console.Write("You: ");
                    var userInput = Console.ReadLine();

                    if (string.IsNullOrWhiteSpace(userInput))
                    {
                        Console.WriteLine("Input can't be empty. Please try again.");
                        continue;
                    }

                    chatSession.AddMessage("user", userInput);

                    var response = await openAIService.SendPromptAndGetResponse(chatSession.GetMessages());
                    Console.WriteLine($"OpenAI: {response}");
                    chatSession.AddMessage("assistant", response);

                }
            }
        }
        catch (Exception ex)
        {
            // Log the error
            LogFatalError(ex);

            // Display error message to user
            //Console.WriteLine($"Fatal error: {ex.Message}");

            // Exit with error code
            Environment.Exit(1);
        }
    }
}