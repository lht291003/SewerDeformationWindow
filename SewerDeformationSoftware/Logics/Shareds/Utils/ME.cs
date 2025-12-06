namespace SewerDeformationSoftware.Logics.Shareds.Utils;

public class ME
{
    private static Mutex? NewMutexInstance;

    private static Boolean CreatedInstance;

    public static Boolean IsSecondProsess() => CreatedInstance == false;

    public static void CheckSingleInstance()
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

    public static void DisposeMutexInstance()
    {
        if (CreatedInstance)
        {
            NewMutexInstance?.ReleaseMutex();
        }

        NewMutexInstance?.Dispose();
    }
}