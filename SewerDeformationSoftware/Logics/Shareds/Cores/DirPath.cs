namespace SewerDeformationSoftware.Logics.Shareds.Cores;

public class DirPath
{
    public static String Warehouse { get; } = Path.Combine(Path.GetTempPath(), "SewerStorage");
}