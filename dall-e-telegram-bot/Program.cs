using Telegram.Bot;

var bot = new TelegramBotClient(await System.IO.File.ReadAllTextAsync("telegramkey.txt"));
