public class EmailService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailService> _logger;
    private readonly string _domain;
    private readonly string _from;
    private readonly string _apiKey;
    public EmailService(IConfiguration configuration, HttpClient httpClient, ILogger<EmailService> logger)
    {
        _domain = configuration["Domain"];
        _from = configuration["EmailFrom"];
        _apiKey = configuration["APIKey"];
        _logger = logger;
        _httpClient = httpClient;
    }
    public async Task SendEmail(EmailMessage emailMessage)
    {
        // Set the base URL to the Mailgun API
        _httpClient.BaseAddress = new Uri($"https://api.mailgun.net/v3/{_domain}/messages");

        // Add basic authentication header
        var byteArray = Encoding.ASCII.GetBytes($"api:{_apiKey}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        // Create the content for the request
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("from", emailMessage.From ?? _from),
            new KeyValuePair<string, string>("to", emailMessage.To),
            new KeyValuePair<string, string>("subject", emailMessage.Subject),
            new KeyValuePair<string, string>("html", emailMessage.Body)
        });

        // Send the POST request
        var response = await _httpClient.PostAsync(string.Empty, content);

        // Check for success
        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email sent successfully.");
        }
        else
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Failed to send email: {response.StatusCode} - {errorMessage}");
        }
    }
