using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ChainOSC.Core;
using Microsoft.Web.WebView2.Core;

namespace ChainOSC.Windows;

public partial class MainWindow : Window
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private readonly GlobalHotkeyService _hotkeys = new();
    private readonly OscUdpSender _oscSender = new();
    private ChainOscSettings _settings;
    private readonly string? _loadWarning;

    public MainWindow()
    {
        _settings = SettingsStore.Load(out _loadWarning);
        InitializeComponent();
        Loaded += OnLoaded;
        Closed += OnClosed;
        _hotkeys.HotkeyChanged += OnHotkeyChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ConfigureHotkeys(_settings);
            await Browser.EnsureCoreWebView2Async();
            Browser.CoreWebView2.Settings.AreDevToolsEnabled = true;
            Browser.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            Browser.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "chainosc.local", Path.Combine(AppContext.BaseDirectory, "WebUI"),
                CoreWebView2HostResourceAccessKind.DenyCors);
            Browser.Source = new Uri("https://chainosc.local/index.html");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ChainOSC could not start.\n\n{ex.Message}",
                            "ChainOSC for Windows", MessageBoxButton.OK,
                            MessageBoxImage.Error);
        }
    }

    private void OnNavigationCompleted(object? sender,
                                       CoreWebView2NavigationCompletedEventArgs e)
    {
        if (!e.IsSuccess) return;
        PostSettings();
        if (_loadWarning is not null) _ = LogAsync(_loadWarning, "error");
        else _ = LogAsync($"Loaded { _settings.Keys.Count } Key setting(s) from " +
                          SettingsStore.FilePath, "ok");
    }

    private void PostSettings()
    {
        var envelope = new { action = "load", settings = _settings };
        Browser.CoreWebView2.PostWebMessageAsJson(
            JsonSerializer.Serialize(envelope, JsonOptions));
    }

    private async void OnWebMessageReceived(object? sender,
                                            CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var request = JsonSerializer.Deserialize<UiRequest>(e.WebMessageAsJson,
                                                                 JsonOptions);
            if (request is null) return;
            if (request.Action == "save" && request.Settings is not null)
            {
                Validate(request.Settings);
                request.Settings.Version = "0.2.0";
                ConfigureHotkeys(request.Settings);
                SettingsStore.Save(request.Settings);
                _settings = request.Settings;
                await LogAsync($"Saved {_settings.Keys.Count} Key setting(s).", "ok");
            }
            else if (request.Action == "test" && request.KeyId is not null)
            {
                var testSettings = request.Settings ?? _settings;
                Validate(testSettings);
                await SendAsync(request.KeyId, request.Pressed, testSettings);
            }
        }
        catch (Exception ex) { await LogAsync(ex.Message, "error"); }
    }

    private void OnHotkeyChanged(object? sender, HotkeyEventArgs e) =>
        _ = Dispatcher.InvokeAsync(() => SendAsync(e.KeyId, e.Pressed));

    private async Task SendAsync(string keyId, bool pressed,
                                 ChainOscSettings? sourceSettings = null)
    {
        sourceSettings ??= _settings;
        var key = sourceSettings.Keys.FirstOrDefault(item => item.Id == keyId);
        if (key is null) return;
        try
        {
            var value = pressed ? key.PressValue : key.ReleaseValue;
            var message = new OscMessage(key.Address, key.Type, value);
            await _oscSender.SendAsync(sourceSettings.Host, sourceSettings.Port,
                                       message);
            await LogAsync($"{key.Name} {(pressed ? "PRESSED" : "RELEASED")}: " +
                           $"{message.Address} {message.Type} {value}", "sent");
        }
        catch (Exception ex) { await LogAsync($"Send failed: {ex.Message}", "error"); }
    }

    private static void Validate(ChainOscSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Host))
            throw new InvalidOperationException("OSC host is required.");
        if (settings.Port is < 1 or > 65535)
            throw new InvalidOperationException("UDP port must be 1–65535.");
        if (settings.Keys.Count == 0)
            throw new InvalidOperationException("Add at least one Key.");

        var identities = new HashSet<string>(StringComparer.Ordinal);
        var hotkeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in settings.Keys)
        {
            if (string.IsNullOrWhiteSpace(key.Id) || !identities.Add(key.Id))
                throw new InvalidOperationException("Each Key must have a unique identity.");
            if (string.IsNullOrWhiteSpace(key.Name))
                throw new InvalidOperationException("Key name is required.");
            if (!TryGetVirtualKey(key.Hotkey, out _))
                throw new InvalidOperationException($"{key.Name}: unsupported hotkey.");
            var signature = $"{key.Ctrl}:{key.Alt}:{key.Shift}:{key.Win}:{key.Hotkey}";
            if (!hotkeys.Add(signature))
                throw new InvalidOperationException($"Duplicate hotkey: {key.HotkeyDisplay}");
            _ = OscPacketBuilder.Build(new OscMessage(key.Address, key.Type, key.PressValue));
            _ = OscPacketBuilder.Build(new OscMessage(key.Address, key.Type, key.ReleaseValue));
        }
    }

    private void ConfigureHotkeys(ChainOscSettings settings)
    {
        Validate(settings);
        _hotkeys.Configure(settings.Keys.Select(key => new HotkeyBinding(
            key.Id, GetVirtualKey(key.Hotkey), key.Ctrl, key.Alt, key.Shift, key.Win)));
    }

    private static int GetVirtualKey(string text)
    {
        if (!TryGetVirtualKey(text, out var virtualKey))
            throw new InvalidOperationException($"Unsupported hotkey: {text}");
        return virtualKey;
    }

    private static bool TryGetVirtualKey(string text, out int virtualKey)
    {
        virtualKey = 0;
        if (!Enum.TryParse<Key>(text, true, out var key)) return false;
        virtualKey = KeyInterop.VirtualKeyFromKey(key);
        return virtualKey != 0;
    }

    private async Task LogAsync(string message, string level)
    {
        if (Browser.CoreWebView2 is null) return;
        await Browser.CoreWebView2.ExecuteScriptAsync(
            $"window.chainOscLog({JsonSerializer.Serialize(message)}," +
            $"{JsonSerializer.Serialize(level)})");
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _hotkeys.Dispose();
        _oscSender.Dispose();
        Browser.Dispose();
    }

    private sealed class UiRequest
    {
        public string Action { get; set; } = "";
        public ChainOscSettings? Settings { get; set; }
        public string? KeyId { get; set; }
        public bool Pressed { get; set; }
    }
}
