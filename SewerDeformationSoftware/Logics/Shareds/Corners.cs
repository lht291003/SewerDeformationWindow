namespace SewerDeformationSoftware.Logics.Shareds;

public static class Corners
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
}