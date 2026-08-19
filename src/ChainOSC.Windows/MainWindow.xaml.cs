using System.IO;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Input;
using ChainOSC.Core;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;
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
    private readonly Dictionary<string, double> _sequenceValues = [];
    private readonly System.Windows.Forms.NotifyIcon _trayIcon;
    private bool _exitRequested;
    private bool _trayNoticeShown;

    public MainWindow()
    {
        _settings = SettingsStore.Load(out _loadWarning);
        InitializeComponent();
        _trayIcon = CreateTrayIcon();
        Loaded += OnLoaded;
        StateChanged += OnStateChanged;
        Closing += OnClosing;
        Closed += OnClosed;
        System.Windows.Application.Current.SessionEnding += (_, _) =>
            _exitRequested = true;
        _hotkeys.HotkeyChanged += OnHotkeyChanged;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            ConfigureHotkeys(_settings);
            ResetSequences(_settings);
            await Browser.EnsureCoreWebView2Async();
            Browser.CoreWebView2.Settings.AreDevToolsEnabled = true;
            Browser.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
            Browser.CoreWebView2.NavigationCompleted += OnNavigationCompleted;
            Browser.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "chainosc.local", Path.Combine(AppContext.BaseDirectory, "WebUI"),
                CoreWebView2HostResourceAccessKind.DenyCors);
            Browser.Source = new Uri("https://chainosc.local/index.html");
            if (App.StartedByWindows && _settings.StartMinimized)
                HideToTray(showNotice: false);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"ChainOSC could not start.\n\n{ex.Message}",
                "ChainOSC for Windows", System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
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
                request.Settings.Version = "0.5.0";
                ConfigureHotkeys(request.Settings);
                StartupManager.SetEnabled(request.Settings.StartWithWindows);
                SettingsStore.Save(request.Settings);
                _settings = request.Settings;
                ResetSequences(_settings);
                await LogAsync($"Saved {_settings.Keys.Count} Key setting(s).", "ok");
            }
            else if (request.Action == "test" && request.KeyId is not null)
            {
                var testSettings = request.Settings ?? _settings;
                Validate(testSettings);
                await SendAsync(request.KeyId, request.Pressed, testSettings);
            }
            else if (request.Action == "exportPreset" && request.Settings is not null &&
                     request.KeyId is not null)
            {
                var key = FindKey(request.Settings, request.KeyId);
                ValidateKey(key);
                var dialog = new SaveFileDialog
                {
                    Title = "Export ChainOSC Key preset",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    DefaultExt = ".json",
                    AddExtension = true,
                    FileName = "ChainOSC-Key-preset.json",
                };
                if (dialog.ShowDialog(this) == true)
                {
                    await File.WriteAllTextAsync(dialog.FileName,
                                                 KeyPresetCodec.Export(key));
                    await LogAsync($"Exported Key preset: {dialog.FileName}", "ok");
                    PostOperationResult(request.KeyId, "Preset exported.", true);
                }
            }
            else if (request.Action == "importPreset" && request.Settings is not null &&
                     request.KeyId is not null)
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Import ChainOSC Key preset",
                    Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
                    CheckFileExists = true,
                    Multiselect = false,
                };
                if (dialog.ShowDialog(this) != true) return;
                if (new FileInfo(dialog.FileName).Length > 16 * 1024)
                    throw new InvalidOperationException(
                        "The preset file exceeds 16 KiB.");
                var presetJson = await File.ReadAllTextAsync(dialog.FileName);
                var key = FindKey(request.Settings, request.KeyId);
                KeyPresetCodec.Apply(presetJson, key);
                // A device preset contains only Key behavior. Hotkey conflicts and
                // other application-wide settings are checked by Save All Settings.
                ValidateKey(key);
                request.Settings.Version = "0.5.0";
                _settings = request.Settings;
                ResetSequences(_settings);
                PostSettings();
                await LogAsync(
                    $"Imported Key preset into {key.Name}. Use Save All Settings to keep it.",
                    "ok");
                PostOperationResult(
                    request.KeyId,
                    "Preset imported. Use Save All Settings to keep it.", true);
            }
        }
        catch (Exception ex)
        {
            await LogAsync(ex.Message, "error");
            PostOperationResult(null, ex.Message, false);
        }
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
            if (key.Mode == KeyMode.Sequence)
            {
                if (!pressed) return;
                var sequence = NormalizeSequence(key.Sequence);
                if (!_sequenceValues.TryGetValue(key.Id, out var value))
                    value = sequence.Start;
                var valueText = sequence.Type switch
                {
                    OscValueType.Int => Math.Round(value, MidpointRounding.AwayFromZero)
                        .ToString(CultureInfo.InvariantCulture),
                    OscValueType.String => value.ToString("0.000", CultureInfo.InvariantCulture),
                    _ => value.ToString("R", CultureInfo.InvariantCulture),
                };
                await SendMessageAsync(sourceSettings, key,
                    new OscMessageConfiguration
                    {
                        Address = sequence.Address,
                        Type = sequence.Type,
                        Value = valueText,
                    }, "SEQUENCE");
                var next = value + sequence.Step;
                if ((sequence.Step >= 0 && next > sequence.End + 1e-6) ||
                    (sequence.Step < 0 && next < sequence.End - 1e-6))
                    next = sequence.Start;
                _sequenceValues[key.Id] = next;
                return;
            }

            var messages = pressed ? key.Press : key.Release;
            var eventName = pressed ? "PRESSED" : "RELEASED";
            foreach (var message in messages)
                await SendMessageAsync(sourceSettings, key, message, eventName);
        }
        catch (Exception ex) { await LogAsync($"Send failed: {ex.Message}", "error"); }
    }

    private async Task SendMessageAsync(ChainOscSettings settings,
                                        KeyConfiguration key,
                                        OscMessageConfiguration configuration,
                                        string eventName)
    {
        var message = new OscMessage(configuration.Address, configuration.Type,
                                     configuration.Value);
        await _oscSender.SendAsync(settings.Host, settings.Port, message);
        await LogAsync($"{key.Name} {eventName}: {message.Address} " +
                       $"{message.Type} {message.Value}", "sent");
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
            ValidateKey(key);
            if (!TryGetVirtualKey(key.Hotkey, out _))
                throw new InvalidOperationException($"{key.Name}: unsupported hotkey.");
            var signature = $"{key.Ctrl}:{key.Alt}:{key.Shift}:{key.Win}:{key.Hotkey}";
            if (!hotkeys.Add(signature))
                throw new InvalidOperationException($"Duplicate hotkey: {key.HotkeyDisplay}");
        }
    }

    private static void ValidateKey(KeyConfiguration key)
    {
        if (string.IsNullOrWhiteSpace(key.Name))
            throw new InvalidOperationException("Key name is required.");
        if (Utf8Length(key.Name) > 64)
            throw new InvalidOperationException($"{key.Name}: Key name is too long.");
        if (key.Press is null || key.Release is null ||
            key.Press.Count + key.Release.Count > 8)
            throw new InvalidOperationException(
                $"{key.Name}: Press and Release messages must total 8 or fewer.");
        foreach (var message in key.Press.Concat(key.Release))
            ValidateMessage(key, message);
        _ = NormalizeSequence(key.Sequence);
    }

    private static void ValidateMessage(KeyConfiguration key,
                                        OscMessageConfiguration message)
    {
        if (Utf8Length(message.Address) > 192)
            throw new InvalidOperationException($"{key.Name}: OSC Address is too long.");
        if (Utf8Length(message.Value) > 128)
            throw new InvalidOperationException($"{key.Name}: OSC value is too long.");
        _ = OscPacketBuilder.Build(new OscMessage(message.Address, message.Type,
                                                   message.Value));
    }

    private static SequenceConfiguration NormalizeSequence(
        SequenceConfiguration sequence)
    {
        if (sequence is null || string.IsNullOrWhiteSpace(sequence.Address) ||
            !sequence.Address.StartsWith('/'))
            throw new InvalidOperationException("Sequence OSC Address must start with '/'.");
        if (Utf8Length(sequence.Address) > 192)
            throw new InvalidOperationException("Sequence OSC Address is too long.");
        if (!double.IsFinite(sequence.Start) || !double.IsFinite(sequence.End) ||
            !double.IsFinite(sequence.Step) || Math.Abs(sequence.Step) < 1e-9)
            throw new InvalidOperationException("Sequence values or Step are invalid.");
        if (sequence.Start <= sequence.End && sequence.Step < 0)
            sequence.Step = -sequence.Step;
        if (sequence.Start > sequence.End && sequence.Step > 0)
            sequence.Step = -sequence.Step;
        return sequence;
    }

    private static int Utf8Length(string value) =>
        System.Text.Encoding.UTF8.GetByteCount(value ?? "");

    private static KeyConfiguration FindKey(ChainOscSettings settings,
                                            string keyId) =>
        settings.Keys.FirstOrDefault(key => key.Id == keyId) ??
        throw new InvalidOperationException("The selected Key was not found.");

    private void ResetSequences(ChainOscSettings settings)
    {
        _sequenceValues.Clear();
        foreach (var key in settings.Keys)
            _sequenceValues[key.Id] = key.Sequence.Start;
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

    private void PostMessage(object message) =>
        Browser.CoreWebView2.PostWebMessageAsJson(
            JsonSerializer.Serialize(message, JsonOptions));

    private void PostOperationResult(string? keyId, string message, bool success) =>
        PostMessage(new { action = "operationResult", keyId, message, success });

    private System.Windows.Forms.NotifyIcon CreateTrayIcon()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();
        menu.Items.Add("Open Settings", null, (_, _) =>
            Dispatcher.Invoke(ShowFromTray));
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add("Exit", null, (_, _) =>
            Dispatcher.Invoke(ExitFromTray));

        var icon = new System.Windows.Forms.NotifyIcon
        {
            Text = "ChainOSC for Windows",
            Icon = System.Drawing.SystemIcons.Application,
            ContextMenuStrip = menu,
            Visible = true,
        };
        icon.DoubleClick += (_, _) => Dispatcher.Invoke(ShowFromTray);
        return icon;
    }

    private void OnStateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized) HideToTray();
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_exitRequested) return;
        e.Cancel = true;
        HideToTray();
    }

    private void HideToTray(bool showNotice = true)
    {
        ShowInTaskbar = false;
        Hide();
        if (!showNotice || _trayNoticeShown) return;
        _trayNoticeShown = true;
        _trayIcon.BalloonTipTitle = "ChainOSC is still running";
        _trayIcon.BalloonTipText =
            "Global hotkeys remain active. Use the tray icon to open settings or exit.";
        _trayIcon.ShowBalloonTip(4000);
    }

    private void ShowFromTray()
    {
        ShowInTaskbar = true;
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    internal void ShowFromExternalLaunch()
    {
        ShowFromTray();
        Topmost = true;
        Topmost = false;
        Focus();
    }

    private void ExitFromTray()
    {
        _exitRequested = true;
        Close();
        System.Windows.Application.Current.Shutdown();
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
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
        public string? PresetJson { get; set; }
    }
}
