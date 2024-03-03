using Amazon.Lambda.Core;
using Amazon.Runtime;
using buybot_lambdas_api.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddSingleton<IS3Service, S3Service>()
    .AddSingleton<UrlGenerator>()
    .AddHttpClient()
    .AddLogging(logging =>
    {
        logging.AddLambdaLogger();
        logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
    })
    .AddControllers();

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var app = builder.Build();

string ipn = Environment.GetEnvironmentVariable("IPN") ?? string.Empty;
app.MapPost("/notify", async(HttpContext context, ILogger<Program> logger, IHttpClientFactory httpClientFactory) =>
{
    // Retrieve the body of the POST request
    using var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();

    logger.LogInformation($"Received POST request body: {body}");

    var telegramBotToken = Environment.GetEnvironmentVariable("BOT_TOKEN");
    var chatId = Environment.GetEnvironmentVariable("CHAT_ID");
    var apiUrl = $"https://api.telegram.org/bot{telegramBotToken}/sendMessage?chat_id={chatId}&text={body}";
 

    var httpClient = httpClientFactory.CreateClient();

    var response = await httpClient.PostAsync(apiUrl, null);

    if (response.IsSuccessStatusCode)
    {
        // Log success or handle further actions
        logger.LogInformation("Telegram notification sent successfully.");
    }
    else
    {
        // Log failure or handle error
        logger.LogInformation($"Failed to send Telegram notification. Status code: {response.StatusCode}");
    }

    return "Notification processed";
});


app.MapGet("/download", async (HttpContext context, ILogger<Program> logger, UrlGenerator urlGenerator) =>
{
  
    string id = context.Request.Query["id"];
    logger.LogInformation($"Received id: {id}");

    return await urlGenerator.GetDownloadUrl(id!);
});

app.Run();
