namespace SewerDeformationSoftware.Logics.Shareds.Utils;

public class WorkDir
{
    public static String Warehouse { get; } = Path.Combine(Path.GetTempPath(), "PipeSewerWorkDir");

    public static void ClearSpace(String FolderPath)
    {
        Directory.EnumerateDirectories(FolderPath).ToList().ForEach(SubPath => ClearSpace(SubPath));

        Directory.EnumerateFiles(FolderPath).ToList().ForEach(SubFilePath =>
        {
            if (!Interop.IsFileLocked(SubFilePath)) File.Delete(SubFilePath);
        });

        if (!Directory.EnumerateFileSystemEntries(FolderPath).Any()) Directory.Delete(FolderPath);
    }

    public static void CleanupSpaceAfterClosed(String FolderPath)

                                       => ClearSpace(FolderPath);

    public static void PrepareSpaceAfterLoaded(String FolderPath)
    {
        if (Directory.Exists(FolderPath)) ClearSpace(FolderPath);

        Directory.CreateDirectory(FolderPath);
    }
}