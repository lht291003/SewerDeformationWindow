namespace SewerDeformationSoftware.Logics.Shareds.Utils;

public partial class WinHelp
{
    public static System.Windows.Window TopmostWindow()
    {
        return Application.Current.Windows.OfType<System.Windows.Window>().LastOrDefault(W => W.IsVisible)!;
    }

    [LibraryImport("Dwmapi.dll")]
    private static partial void DwmSetWindowAttribute(IntPtr Hwnd, Int32 Attr, ref Int32 Value, Int32 Size);
    public static void Win10(System.Windows.Window Win)
    {
        Win.SourceInitialized += (_, _) =>
        {
            Int32 Preference = 1;

            DwmSetWindowAttribute(new WindowInteropHelper(Win).Handle, 33, ref Preference, sizeof(Int32));
        };
    }

    public static Boolean IsFileLocked(String FilePath)
    {
        if (File.Exists(FilePath))
        {
            IntPtr CreateFileHandle = CreateFile(FilePath, 0xC0000000, 0, IntPtr.Zero, 3, 0, IntPtr.Zero);

            IntPtr Value = new(-1);

            if (CreateFileHandle == Value)
            {
                return true;
            }

            CloseHandle(CreateFileHandle);

            return false;
        }

        return false;
    }

    [LibraryImport("Kernel32.dll", EntryPoint = "CreateFileW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr CreateFile(String FN,
                                             UInt32 DA,
                                             UInt32 SM,
                                             IntPtr SA,
                                             UInt32 CD,
                                             UInt32 FAA, IntPtr TF);

    [LibraryImport("Kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial Boolean CloseHandle(IntPtr HandleObject);
}