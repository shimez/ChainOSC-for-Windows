using System.Configuration;
using System.Data;
using System.Windows;

namespace ChainOSC.Windows;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private const string MutexName = @"Local\ChainOSC-for-Windows-SingleInstance";
    private const string ActivationEventName =
        @"Local\ChainOSC-for-Windows-ShowSettings";
    private Mutex? _instanceMutex;
    private EventWaitHandle? _activationEvent;
    private CancellationTokenSource? _activationCancellation;

    public static bool StartedByWindows { get; private set; }

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        StartedByWindows = e.Args.Any(arg =>
            string.Equals(arg, "--startup", StringComparison.OrdinalIgnoreCase));
        _instanceMutex = new Mutex(initiallyOwned: true, MutexName,
                                   out var isFirstInstance);
        _activationEvent = new EventWaitHandle(
            false, EventResetMode.AutoReset, ActivationEventName);
        if (!isFirstInstance)
        {
            _activationEvent.Set();
            Shutdown();
            return;
        }

        _activationCancellation = new CancellationTokenSource();
        _ = Task.Run(() => ListenForActivation(_activationCancellation.Token));
        base.OnStartup(e);
    }

    private void ListenForActivation(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (_activationEvent?.WaitOne(500) == true)
            {
                _ = Dispatcher.BeginInvoke(() =>
                {
                    if (MainWindow is ChainOSC.Windows.MainWindow window)
                        window.ShowFromExternalLaunch();
                });
            }
        }
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _activationCancellation?.Cancel();
        _activationEvent?.Set();
        _activationEvent?.Dispose();
        if (_instanceMutex is not null)
        {
            try { _instanceMutex.ReleaseMutex(); }
            catch (ApplicationException) { }
            _instanceMutex.Dispose();
        }
        _activationCancellation?.Dispose();
        base.OnExit(e);
    }
}

