using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using ChainOSC.Core;
using Microsoft.Web.WebView2.Core;

namespace ChainOSC.Windows;

public partial class MainWindow : Window
{
    private readonly GlobalHotkeyService _hotkey = new();
    private readonly OscUdpSender _oscSender = new();
    private AppConfiguration _configuration = AppConfiguration.Default;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
        _hotkey.HotkeyChanged += OnHotkeyChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await Browser.EnsureCoreWebView2Async();
            Browser.CoreWebView2.Settings.AreDevToolsEnabled = true;
            Browser.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "chainosc.local", Path.Combine(AppContext.BaseDirectory, "WebUI"),
                CoreWebView2HostResourceAccessKind.DenyCors);
            Browser.Source = new Uri("https://chainosc.local/index.html");
            _hotkey.Configure(_configuration.Hotkey, _configuration.Ctrl,
                              _configuration.Alt, _configuration.Shift,
                              _configuration.Win);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ChainOSC could not start.\n\n{ex.Message}",
                            "ChainOSC for Windows", MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }

    private async void OnWebMessageReceived(object? sender,
                                            CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var request = JsonSerializer.Deserialize<UiRequest>(e.WebMessageAsJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (request is null) return;
            if (request.Action == "apply")
            {
                var configuration = AppConfiguration.From(request);
                Validate(configuration);
                _hotkey.Configure(configuration.Hotkey, configuration.Ctrl,
                                  configuration.Alt, configuration.Shift,
                                  configuration.Win);
                _configuration = configuration;
                await LogAsync($"Ready: {configuration.HotkeyDisplay} → " +
                               $"{configuration.Host}:{configuration.Port}", "ok");
            }
            else if (request.Action == "testPress") await SendAsync(true);
            else if (request.Action == "testRelease") await SendAsync(false);
        }
        catch (Exception ex) { await LogAsync(ex.Message, "error"); }
    }

    private void OnHotkeyChanged(object? sender, HotkeyEventArgs e) =>
        _ = Dispatcher.InvokeAsync(() => SendAsync(e.Pressed));

    private async Task SendAsync(bool pressed)
    {
        try
        {
            var value = pressed ? _configuration.PressValue : _configuration.ReleaseValue;
            var message = new OscMessage(_configuration.Address, _configuration.Type, value);
            await _oscSender.SendAsync(_configuration.Host, _configuration.Port, message);
            await LogAsync($"{(pressed ? "PRESSED" : "RELEASED")}: " +
                           $"{message.Address} {message.Type} {value}", "sent");
        }
        catch (Exception ex) { await LogAsync($"Send failed: {ex.Message}", "error"); }
    }

    private async Task LogAsync(string message, string level)
    {
        if (Browser.CoreWebView2 is null) return;
        await Browser.CoreWebView2.ExecuteScriptAsync(
            $"window.chainOscLog({JsonSerializer.Serialize(message)}," +
            $"{JsonSerializer.Serialize(level)})");
    }

    private static void Validate(AppConfiguration value)
    {
        if (string.IsNullOrWhiteSpace(value.Host))
            throw new InvalidOperationException("OSC host is required.");
        if (value.Port is < 1 or > 65535)
            throw new InvalidOperationException("UDP port must be 1–65535.");
        if (!value.Address.StartsWith('/'))
            throw new InvalidOperationException("OSC Address must start with '/'.");
        _ = OscPacketBuilder.Build(new OscMessage(value.Address, value.Type, value.PressValue));
        _ = OscPacketBuilder.Build(new OscMessage(value.Address, value.Type, value.ReleaseValue));
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _hotkey.Dispose();
        _oscSender.Dispose();
        Browser.Dispose();
    }

    private sealed record UiRequest(
        string Action = "", string Host = "127.0.0.1", int Port = 9000,
        string Hotkey = "F8", bool Ctrl = false, bool Alt = false,
        bool Shift = false, bool Win = false,
        string Address = "/avatar/parameters/ChainOSCKey", string Type = "int",
        string PressValue = "1", string ReleaseValue = "0");

    private sealed record AppConfiguration(
        string Host, int Port, Key Hotkey, bool Ctrl, bool Alt, bool Shift,
        bool Win, string Address, OscValueType Type, string PressValue,
        string ReleaseValue)
    {
        public static AppConfiguration Default => new(
            "127.0.0.1", 9000, Key.F8, false, false, false, false,
            "/avatar/parameters/ChainOSCKey", OscValueType.Int, "1", "0");

        public string HotkeyDisplay =>
            $"{(Ctrl ? "Ctrl+" : "")}{(Alt ? "Alt+" : "")}" +
            $"{(Shift ? "Shift+" : "")}{(Win ? "Win+" : "")}{Hotkey}";

        public static AppConfiguration From(UiRequest request)
        {
            if (!Enum.TryParse<Key>(request.Hotkey, true, out var hotkey))
                throw new InvalidOperationException("Unsupported hotkey.");
            if (!Enum.TryParse<OscValueType>(request.Type, true, out var type))
                throw new InvalidOperationException("Unsupported OSC value type.");
            return new(request.Host.Trim(), request.Port, hotkey, request.Ctrl,
                       request.Alt, request.Shift, request.Win,
                       request.Address.Trim(), type, request.PressValue,
                       request.ReleaseValue);
        }
    }
}
