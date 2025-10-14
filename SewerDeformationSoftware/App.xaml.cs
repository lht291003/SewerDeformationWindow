namespace SewerDeformationSoftware;

public partial class App : Application
{
    public App()
    {
        TaskScheduler.UnobservedTaskException += CatchGlobalUnhandledTaskException;

        Application.Current.DispatcherUnhandledException += CatchGlobalUnhandledUIThreadException;

        AppDomain.CurrentDomain.UnhandledException += CatchFinalExceptionAnywhereBeforeTheAppEnds;
    }

    private void CatchFinalExceptionAnywhereBeforeTheAppEnds(Object Sender, UnhandledExceptionEventArgs Event)
    {
        if (Application.Current.Dispatcher is not null && !Application.Current.Dispatcher.HasShutdownFinished)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                String? ExceptionMessage = (Event.ExceptionObject as Exception)?.Message ?? "Không xác định";

                Message.ShowErrors($"Phần mềm hiện đang xảy ra ngoại lệ nghiêm trọng: '{ExceptionMessage}'");
            });
        }
    }

    private void CatchGlobalUnhandledTaskException(Object? Sender, UnobservedTaskExceptionEventArgs Event)
    {
        Event.SetObserved();

        Application.Current.Dispatcher.Invoke(() =>
        {
            String? ExceptionMessage = Event.Exception.InnerException?.Message ?? Event.Exception.Message;

            Message.ShowErrors($"Phần mềm hiện đang xảy ra ngoại lệ trên luồng nền: '{ExceptionMessage}'");
        });
    }

    private void CatchGlobalUnhandledUIThreadException(Object Sender, DispatcherUnhandledExceptionEventArgs Event)
    {
        Message.ShowErrors($"Phần mềm hiện đang xảy ra ngoại lệ trên luồng chính UI: '{Event.Exception.Message}'");

        Event.Handled = true;
    }

    private void UnSelectedRowsWhenClickEmptyArea(Object Obj, MouseButtonEventArgs E)
    {
        if (Obj is DataGrid DG && VisualTreeHelper.HitTest(DG, Mouse.GetPosition(DG)).VisualHit is not DataGridRow)
        {
            DG.UnselectAll();
        }
    }
}