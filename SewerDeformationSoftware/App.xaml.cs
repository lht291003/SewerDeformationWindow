namespace SewerDeformationSoftware;

public partial class App : Application
{
    private void Begin(Object Sender, StartupEventArgs Evt)
    {
        ME.CheckSingleInstance();

        if (ME.IsSecondProsess())
        {
            return;
        }

        WorkDir.PrepareSpaceAfterLoaded(WorkDir.Warehouse);
    }

    private void CleanupTheMess()
    {
        ME.DisposeMutexInstance();

        if (ME.IsSecondProsess())
        {
            return;
        }

        WorkDir.CleanupSpaceAfterClosed(WorkDir.Warehouse);
    }

    private void Terminal(Object Sender, ExitEventArgs Evt)

                                       => CleanupTheMess();

    public App()
    {
        AppDomain.CurrentDomain.UnhandledException += CatchFinalExceptionAnywhereBeforeTheAppEnds;

        Application.Current.DispatcherUnhandledException += CatchGlobalUnhandledUIThreadException;

        TaskScheduler.UnobservedTaskException += CatchGlobalUnhandledTaskException;
    }

    private void CatchFinalExceptionAnywhereBeforeTheAppEnds(Object Sender, UnhandledExceptionEventArgs Event)
    {
        CleanupTheMess();

        if (Application.Current.Dispatcher is not null && !Application.Current.Dispatcher.HasShutdownFinished)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                String? ExceptionMessage = (Event.ExceptionObject as Exception)?.Message ?? "Undetermined";

                Message.ShowErrors($"Phần mềm hiện đang xảy ra ngoại lệ nghiêm trọng: {ExceptionMessage}");
            });
        }
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