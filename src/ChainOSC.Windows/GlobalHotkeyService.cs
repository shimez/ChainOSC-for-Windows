using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace ChainOSC.Windows;

public sealed class HotkeyEventArgs(bool pressed) : EventArgs
{
    public bool Pressed { get; } = pressed;
}

public sealed class GlobalHotkeyService : IDisposable
{
    private const int WhKeyboardLl = 13;
    private const int WmKeyDown = 0x0100, WmKeyUp = 0x0101;
    private const int WmSysKeyDown = 0x0104, WmSysKeyUp = 0x0105;
    private readonly LowLevelKeyboardProc _callback;
    private IntPtr _hook;
    private int _virtualKey = KeyInterop.VirtualKeyFromKey(Key.F8);
    private bool _ctrl, _alt, _shift, _win, _pressed;

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

    public void Configure(Key key, bool ctrl, bool alt, bool shift, bool win)
    {
        _virtualKey = KeyInterop.VirtualKeyFromKey(key);
        _ctrl = ctrl; _alt = alt; _shift = shift; _win = win; _pressed = false;
    }

    private IntPtr HookCallback(int code, IntPtr wParam, IntPtr lParam)
    {
        if (code >= 0)
        {
            var data = Marshal.PtrToStructure<KbdLlHookStruct>(lParam);
            var message = wParam.ToInt32();
            var isDown = message is WmKeyDown or WmSysKeyDown;
            var isUp = message is WmKeyUp or WmSysKeyUp;
            if (data.VirtualKeyCode == _virtualKey)
            {
                if (isDown && !_pressed && ModifiersMatch())
                {
                    _pressed = true;
                    HotkeyChanged?.Invoke(this, new HotkeyEventArgs(true));
                }
                else if (isUp && _pressed)
                {
                    _pressed = false;
                    HotkeyChanged?.Invoke(this, new HotkeyEventArgs(false));
                }
            }
        }
        return CallNextHookEx(_hook, code, wParam, lParam);
    }

    private bool ModifiersMatch() =>
        IsPressed(0x11) == _ctrl && IsPressed(0x12) == _alt &&
        IsPressed(0x10) == _shift && (IsPressed(0x5B) || IsPressed(0x5C)) == _win;
    private static bool IsPressed(int key) => (GetAsyncKeyState(key) & 0x8000) != 0;

    public void Dispose()
    {
        if (_hook == IntPtr.Zero) return;
        UnhookWindowsHookEx(_hook); _hook = IntPtr.Zero; GC.SuppressFinalize(this);
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
