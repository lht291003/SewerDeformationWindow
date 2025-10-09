namespace SewerDeformationSoftware.Logics.Shareds;

public static class Message
{
    public static void ShowErrors(String Message)

        => MessageBox.Show(Windows.TopmostWindow(), Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

    public static bool ShowConfirm(String Message)

        => MessageBox.Show(Windows.TopmostWindow(), Message, "Thông báo", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK;

    public static void ShowSuccess(String Message)

                                  => MessageBox.Show(Windows.TopmostWindow(), Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
}