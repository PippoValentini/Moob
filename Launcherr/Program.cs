using System;
using System.Diagnostics;
using System.IO;

class Launcher
{
    static void Main(string[] args)
    {
        // Percorso assoluto di Watchdog.exe a partire dalla posizione di Launcherr.exe
        string pathToBotExe = Path.GetFullPath(
    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Watchdog", "bin", "Debug", "net8.0", "Watchdog.exe")
);

        Console.WriteLine("🔍 Verifica percorso BotExe:");
        Console.WriteLine(pathToBotExe);

        // Controllo che il file esista
        if (!File.Exists(pathToBotExe))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ ERRORE: File Watchdog.exe non trovato.");
            Console.ResetColor();
            return;
        }

        // Configurazione processo
        var psi = new ProcessStartInfo
        {
            FileName = pathToBotExe,
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden
        };

        try
        {
            var process = Process.Start(psi);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✅ Bot avviato con PID: {process.Id}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("❌ Errore durante l'avvio del bot:");
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
    }
}
