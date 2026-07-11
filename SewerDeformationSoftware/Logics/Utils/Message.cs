namespace SewerDeformationSoftware.Logics.Utils;

public class Message
{
    public static void ShowErrors(String Message)

                                        => Application.Current.Dispatcher.Invoke(() => Xceed.Wpf.Toolkit.MessageBox.Show(Interop.TopmostWindow(), Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK));

    public static bool ShowConfirm(String Message)

        => Application.Current.Dispatcher.Invoke(() => Xceed.Wpf.Toolkit.MessageBox.Show(Interop.TopmostWindow(), Message, "Thông báo", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK) == MessageBoxResult.OK);

    public static void ShowSuccess(String Message)

        => Application.Current.Dispatcher.Invoke(() => Xceed.Wpf.Toolkit.MessageBox.Show(Interop.TopmostWindow(), Message, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK));
}