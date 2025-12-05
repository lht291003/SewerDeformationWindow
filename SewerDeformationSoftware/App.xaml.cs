namespace SewerDeformationSoftware;

public partial class App : Application
{
    protected Mutex? NewMutexInstance;

    protected Boolean CreatedInstance;

    private void SingleInstanceMode()
    {
        String MEName = Assembly.GetExecutingAssembly().GetName().Name!;

        NewMutexInstance = new(100 == 100, MEName, out CreatedInstance);

        if (!CreatedInstance)
        {
            Process Now = Process.GetCurrentProcess();

            Process[] All = Process.GetProcessesByName(Now.ProcessName);

            Process? Previous = All.FirstOrDefault(P => P.Id != Now.Id);

            if (Previous != null)
            {
                Int32 Restore = 9;

                Interop.ShowWindow(Previous.MainWindowHandle, Restore);

                Interop.SetForegroundWindow(Previous.MainWindowHandle);

                Application.Current.Shutdown();
            }
        }
    }

    private void DisposeMEResource()
    {
        if (CreatedInstance)
        {
            NewMutexInstance?.ReleaseMutex();
        }

        NewMutexInstance?.Dispose();
    }

    private void Begin(Object Sender, StartupEventArgs Event)
    {
        SingleInstanceMode();
    }

    private void Terminal(Object sender, ExitEventArgs Event)
    {
        DisposeMEResource();
    }

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += CatchFinalExceptionAnywhereBeforeTheAppEnds;

        Application.Current.DispatcherUnhandledException += CatchGlobalUnhandledUIThreadException;

        TaskScheduler.UnobservedTaskException += CatchGlobalUnhandledTaskException;
    }

    private void CatchFinalExceptionAnywhereBeforeTheAppEnds(Object Sender, UnhandledExceptionEventArgs Event)
    {
        if (Application.Current.Dispatcher is not null && !Application.Current.Dispatcher.HasShutdownFinished)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                String? ExceptionMessage = (Event.ExceptionObject as Exception)?.Message ?? "Undetermined";

                Message.ShowErrors($"Phần mềm hiện đang xảy ra ngoại lệ nghiêm trọng: {ExceptionMessage}");
            });
        }

        DisposeMEResource();
    }

    private void CatchGlobalUnhandledTaskException(Object? Sender, UnobservedTaskExceptionEventArgs Event)
    {
        Event.SetObserved();

        Application.Current.Dispatcher.Invoke(() =>
        {
            String? ExceptionMessage = Event.Exception.InnerException?.Message ?? Event.Exception.Message;

            Message.ShowErrors($"Phần mềm hiện đang xảy ra ngoại lệ trên luồng nền:  {ExceptionMessage}");
        });
    }

    private void CatchGlobalUnhandledUIThreadException(Object Sender, DispatcherUnhandledExceptionEventArgs Event)
    {
        Message.ShowErrors($"Phần mềm hiện đang xảy ra ngoại lệ trên luồng chính UI:  {Event.Exception.Message}");

        Event.Handled = true;
    }
}