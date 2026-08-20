using Microsoft.Win32;

namespace ChainOSC.Windows;

internal static class StartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "ChainOSC for Windows";

    public static void SetEnabled(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true) ??
            throw new InvalidOperationException("The Windows startup registry could not be opened.");
        if (!enabled)
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
            return;
        }

        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
            throw new InvalidOperationException("The ChainOSC executable path is unavailable.");
        key.SetValue(ValueName, $"\"{executablePath}\" --startup",
                     RegistryValueKind.String);
    }
}
