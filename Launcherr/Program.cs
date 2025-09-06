using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Win32;

class Launcher
{
    private const string AppName = "MoobLauncher";
    private const string RunKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    static void Main()
    {
        // Evita doppie esecuzioni del launcher
        using var mutex = new Mutex(true, "MoobLauncherMutex", out bool createdNew);
        if (!createdNew)
        {
            Console.WriteLine("⛔ Launcher già in esecuzione.");
            return;
        }

        // Se non siamo in avvio automatico, registriamoci ora
        if (!IsInStartup())
        {
            if (TryAddToStartup())
                Console.WriteLine("✅ Registrato in avvio automatico.");
            else
                Console.WriteLine("⚠️ Impossibile registrarsi in avvio automatico.");
        }

        // Trova Watchdog.exe
        string? watchdogPath = ResolveWatchdogPath();
        if (watchdogPath is null || !File.Exists(watchdogPath))
        {
            Console.WriteLine("❌ ERRORE: Watchdog.exe non trovato!");
            return;
        }

        // Evita doppie istanze di Watchdog
        if (Process.GetProcessesByName("Watchdog").Any())
        {
            Console.WriteLine("⛔ Watchdog è già in esecuzione.");
            return;
        }

        // Avvia Watchdog in modo nascosto
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = watchdogPath,
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Path.GetDirectoryName(watchdogPath)!
            };

            var process = Process.Start(psi);
            Console.WriteLine($"✅ Watchdog avviato (PID {process?.Id})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Errore durante l'avvio: {ex.Message}");
        }
    }

    // ---- Avvio automatico ----
    private static bool IsInStartup()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            return key?.GetValue(AppName) is string;
        }
        catch { return false; }
    }

    private static bool TryAddToStartup()
    {
        try
        {
            string exePath = Process.GetCurrentProcess().MainModule!.FileName!;
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true)
                            ?? Registry.CurrentUser.CreateSubKey(RunKeyPath, true);
            key!.SetValue(AppName, $"\"{exePath}\"");
            return true;
        }
        catch { return false; }
    }

    // ---- Risoluzione percorso Watchdog ----
    private static string? ResolveWatchdogPath()
    {
        string baseDir = AppContext.BaseDirectory;

        // 1) Stessa cartella del launcher (publish distribuito)
        string sameDir = Path.Combine(baseDir, "Watchdog.exe");
        if (File.Exists(sameDir)) return sameDir;

        // 2) Percorso tipico Visual Studio (Debug/Release)
        string config = Directory.Exists(Path.Combine(baseDir, "..", "..", "..", "..", "Watchdog", "bin", "Debug"))
            ? "Debug"
            : "Release";

        string fromSolution = Path.GetFullPath(
            Path.Combine(baseDir, "..", "..", "..", "..", "Watchdog", "bin", config, "net8.0", "Watchdog.exe")
        );

        if (File.Exists(fromSolution)) return fromSolution;

        return null;
    }
}