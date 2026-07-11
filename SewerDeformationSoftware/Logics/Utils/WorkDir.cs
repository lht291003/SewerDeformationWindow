namespace SewerDeformationSoftware.Logics.Utils;

public class WorkDir
{
    private static FileStream? Keep;

    public static String Warehouse { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SewerFolder");

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
    {
        ResetOriginalPermissions(FolderPath);

        ClearSpace(FolderPath);
    }

    public static void PrepareSpaceAfterLoaded(String FolderPath)
    {
        if (Directory.Exists(FolderPath)) ClearSpace(FolderPath);

        Directory.CreateDirectory(FolderPath);

        EnforceFolderPermissions(FolderPath);
    }

    public static String LockFileNameAsFolder(String FolderPath)
    {
        String LockName = $"{Path.GetFileName(FolderPath)}.Lock";

        String LockFilePath = Path.Combine(FolderPath, LockName);

        return LockFilePath;
    }

    public static void EnforceFolderPermissions(String FolderPath)
    {
        String GetLockFilePath = LockFileNameAsFolder(FolderPath);

        Keep = new(GetLockFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);

        File.SetAttributes(GetLockFilePath, FileAttributes.ReadOnly | FileAttributes.Hidden);
    }

    public static void ResetOriginalPermissions(String FolderPath)
    {
        String GetLockFilePath = LockFileNameAsFolder(FolderPath);

        Keep?.Dispose();

        File.SetAttributes(GetLockFilePath, FileAttributes.Normal);

        File.Delete(GetLockFilePath);
    }
}