using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ChainOSC.Windows;

public sealed record HotkeyBinding(string Id, int VirtualKey, bool Ctrl,
                                   bool Alt, bool Shift, bool Win);

public sealed class HotkeyEventArgs(string keyId, bool pressed) : EventArgs
{
    public string KeyId { get; } = keyId;
    public bool Pressed { get; } = pressed;
}

public sealed class GlobalHotkeyService : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100, WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104, WmSysKeyUp = 0x0105;
    private readonly LowLevelKeyboardProc _callback;
    private readonly object _sync = new();
    private List<BindingState> _bindings = [];
    private IntPtr _hook;
    public bool Suspended { get; set; }

    public event EventHandler<HotkeyEventArgs>? HotkeyChanged;

    public GlobalHotkeyService()
    {
        _callback = HookCallback;
        using var process = Process.GetCurrentProcess();
        using var module = process.MainModule;
        _hook = SetWindowsHookEx(WhKeyboardLl, _callback,
            GetModuleHandle(module?.ModuleName), 0);
        if (_hook == IntPtr.Zero)
            throw new InvalidOperationException("Global keyboard hook could not be installed.");
    }

    public void Configure(IEnumerable<HotkeyBinding> bindings)
    {
        lock (_sync)
            _bindings = bindings.Select(binding => new BindingState(binding)).ToList();
    }

    private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        HotkeyEventArgs? eventArgs = null;
        if (code >= 0 && !Suspended)
        {
            var data = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
            var message = wParam.ToInt32();
            var isDown = message is WmKeyDown or WmSysKeyDown;
            var isUp = message is WmKeyUp or WmSysKeyUp;
            lock (_sync)
            {
                foreach (var state in _bindings.Where(
                             item => item.Binding.VirtualKey == data.VirtualKeyCode))
                {
                    if (isDown && !state.Pressed && ModifiersMatch(state.Binding))
                    {
                        state.Pressed = true;
                        eventArgs = new HotkeyEventArgs(state.Binding.Id, true);
                        break;
                    }
                    if (isUp && state.Pressed)
                    {
                        state.Pressed = false;
                        eventArgs = new HotkeyEventArgs(state.Binding.Id, false);
                        break;
                    }
                }
            }
        }
        if (eventArgs is not null) HotkeyChanged?.Invoke(this, eventArgs);
        return CallNextHookEx(_hook, code, wParam, lParam);
    }

    private static bool ModifiersMatch(HotkeyBinding binding) =>
        IsPressed(0x11) == binding.Ctrl && IsPressed(0x12) == binding.Alt &&
        IsPressed(0x10) == binding.Shift &&
        (IsPressed(0x5B) || IsPressed(0x5C)) == binding.Win;
    private static bool IsPressed(int key) => (GetAsyncKeyState(key) & 0x8000) != 0;

    public void Dispose()
    {
        if (_hook == IntPtr.Zero) return;
        UnhookWindowsHookEx(_hook); _hook = IntPtr.Zero; GC.SuppressFinalize(this);
    }

    private sealed class BindingState(HotkeyBinding binding)
    {
        public HotkeyBinding Binding { get; } = binding;
        public bool Pressed { get; set; }
    }

    private delegate IntPtr LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam);
    [StructLayout(LayoutKind.Sequential)]
    private struct KbdLlHookStruct
    {
        public int VirtualKeyCode, ScanCode, Flags, Time;
        public IntPtr ExtraInfo;
    }
    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr module, uint threadId);
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hook);
    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hook, int code, IntPtr wParam, IntPtr lParam);
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? moduleName);
    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);
}
