using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;

var bot = new TelegramBotClient(await File.ReadAllTextAsync("./telegramkey.txt"));
var client = new HttpClient();

var contentType = new MediaTypeWithQualityHeaderValue("application/json");
var baseAddress = "https://api.openai.com";
var api = "/v1/images/generations";
client.BaseAddress = new Uri(baseAddress);
client.DefaultRequestHeaders.Accept.Add(contentType);
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", await File.ReadAllTextAsync("./openaikey.txt"));

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
};
        
bot.StartReceiving(
    updateHandler: HandleUpdateAsync,
    pollingErrorHandler: HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);

var me = await bot.GetMeAsync(cancellationToken: cts.Token);

Console.WriteLine($"Start listening for @{me.Username}");
Console.ReadLine();
        
// Send cancellation request to stop bot
cts.Cancel();

async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.telegram.org/bots/api#message
    if (update.Message is not { } message)
        return;

    if (message.Type is MessageType.Text && message.Text.ToLower().Contains("/create"))
    {
        if (message.Text.Length >= 8)
        {
            string prompt = message.Text[8..];
            
            var reqData = new DataModel
            {
                prompt = prompt
            };
            
            var jsonData = JsonConvert.SerializeObject(reqData);
            var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(api, contentData);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }
    }
}

Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception,
    CancellationToken cancellationToken)
{
    var ErrorMessage = exception switch
    {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
    };

    Console.WriteLine(ErrorMessage);
    return Task.CompletedTask;
}

class DataModel
{
    public string prompt;
}