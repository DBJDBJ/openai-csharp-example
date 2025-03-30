using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace openai_csharp_example;

using System;
using System.Net.Http;
using System.Threading.Tasks;

public struct OpenAIServiceKLIENT : IDisposable
{
    private HttpClient _httpClient;
    private string _apiKey;
    private bool _disposed;

    // Private constructor - we'll use the factory method instead
    private OpenAIServiceKLIENT(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _disposed = false;
    }

    // WTF is this "Factory Method"?
    // Because here you can:
    // A: make it async
    // B: make sure it is singleton
    // C: whatever else you desire... 
    public static OpenAIServiceKLIENT Create(string apiKey)
    {
        if (string.IsNullOrEmpty(apiKey))
            throw new ArgumentNullException(nameof(apiKey));

        var httpClient = new HttpClient();
        // Configure HttpClient as needed
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        return new OpenAIServiceKLIENT(httpClient, apiKey);
    }

    // Implementation of IDisposable
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }

    // NOTE: exceptions we silently pass, if any
    public async Task<string> SendPromptAndGetResponse(IEnumerable<object> messages)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(OpenAIServiceKLIENT));

        // ofc this should be in configuration
        const string requestUri = "https://api.openai.com/v1/chat/completions";
        var requestBody = new
        {
            temperature = 0.2,
            //model = "gpt-3.5-turbo",
            model = "gpt-4o",
            messages = messages.ToList()
            // tool_choice = "auto", // Optional: for function calling
            //max_tokens = 4096 // Optional: control response length
        };

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);

        var response = await _httpClient.PostAsync(
            requestUri,
            new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));

        response.EnsureSuccessStatusCode();

        var responseBody = JsonConvert.DeserializeObject<ResponseBody>(await response.Content.ReadAsStringAsync());
        return responseBody.Choices[0].Message.Content.Trim();
    }

}