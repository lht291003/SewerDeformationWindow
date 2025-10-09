namespace SewerDeformationSoftware.Logics.Shareds;

public static class Windows
{
    public static Window TopmostWindow() => Application.Current.Windows.OfType<Window>().LastOrDefault(Window => Window.IsVisible)!;
}