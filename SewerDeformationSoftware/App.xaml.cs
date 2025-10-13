namespace SewerDeformationSoftware;

public partial class App : Application
{
    public App()
    {
        TaskScheduler.UnobservedTaskException += CatchGlobalUnhandledTaskException;

        Application.Current.DispatcherUnhandledException += CatchGlobalUnhandledUIThreadException;

        AppDomain.CurrentDomain.UnhandledException += CatchLastExceptionAnyWhereAfterShuttingDown;
    }

    private void CatchLastExceptionAnyWhereAfterShuttingDown(Object Sender, UnhandledExceptionEventArgs Event)
    {
        if (Application.Current.Dispatcher is not null && !Application.Current.Dispatcher.HasShutdownFinished)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Message.ShowErrors($"Phần mềm hiện đang xảy ra một ngoại lệ rất nghiêm trọng không thể cứu!");
            });
        }
    }

    private void CatchGlobalUnhandledTaskException(Object? Sender, UnobservedTaskExceptionEventArgs Event)
    {
        Event.SetObserved();

        Application.Current.Dispatcher.Invoke(() =>
        {
            String? ExceptionMessage = Event.Exception.InnerException?.Message;

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