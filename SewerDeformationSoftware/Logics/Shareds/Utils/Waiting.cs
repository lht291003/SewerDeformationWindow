namespace SewerDeformationSoftware.Logics.Shareds.Utils;

public static class Waiting
{
    readonly static Lazy<WaitIndicator> CircleBar = new(() => new WaitIndicator());

    public static void ShowProgressRing()
    {
        WaitIndicator Instance = CircleBar.Value;

        Instance.Owner = Windows.TopmostWindow();

        Instance.Owner.IsEnabled = false;

        Instance.Show();
    }

    public static void HideProgressRing()
    {
        WaitIndicator Instance = CircleBar.Value;

        if (Instance.IsVisible)
        {
            if (Instance.Owner != null)
            {
                Instance.Owner.IsEnabled = true;
            }

            Instance.Hide();
        }
    }
}