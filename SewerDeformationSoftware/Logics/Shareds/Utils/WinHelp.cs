namespace SewerDeformationSoftware.Logics.Shareds.Utils;

public partial class WinHelp
{
    public static System.Windows.Window TopmostWindow()
    {
        return Application.Current.Windows.OfType<System.Windows.Window>().LastOrDefault(W => W.IsVisible)!;
    }

    [LibraryImport("Dwmapi.dll")]
    private static partial void DwmSetWindowAttribute(IntPtr Hwnd, Int32 Attr, ref Int32 Value, Int32 Size);
    public static void Win10 (System.Windows.Window Win)
    {
        Win.SourceInitialized += (_, _) =>
        {
            Int32 Preference = 1;

            DwmSetWindowAttribute(new WindowInteropHelper(Win).Handle, 33, ref Preference, sizeof(Int32));
        };
    }
}