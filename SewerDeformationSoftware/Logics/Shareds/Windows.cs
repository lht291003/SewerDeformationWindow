namespace SewerDeformationSoftware.Logics.Shareds;

public static class Windows
{
    [DllImport("Dwmapi.dll")]
    private static extern Int32 DwmSetWindowAttribute(IntPtr Hwnd, Int32 Attr, ref Int32 Value, Int32 Size);
    public static void SetSquareCorners(Window Win)
    {
        Win.SourceInitialized += (Sender, Event) =>
        {
            Int32 Preference = 1;

            DwmSetWindowAttribute(new WindowInteropHelper(Win).Handle, 33, ref Preference, sizeof(Int32));
        };
    }

    public static Window TopmostWindow() => Application.Current.Windows.OfType<Window>().LastOrDefault(Window => Window.IsVisible)!;
}