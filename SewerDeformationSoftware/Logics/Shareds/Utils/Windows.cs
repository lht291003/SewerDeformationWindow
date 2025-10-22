namespace SewerDeformationSoftware.Logics.Shareds.Utils;

public static class Windows
{
    public static System.Windows.Window TopmostWindow()
    {
        return Application.Current.Windows.OfType<System.Windows.Window>().LastOrDefault(W => W.IsVisible)!;
    }
}