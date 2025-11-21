namespace SewerDeformationSoftware.Logics.Shareds.Cores;

public class Accuracy
{
    public static (String, String, String) CreateDirectoryTree()
    {
        String TmpDir = Path.Combine(DirPath.Warehouse, "Temp");

        String StgDir = Path.Combine(DirPath.Warehouse, "Eval");

        String ImageDir = Path.Combine(StgDir, "Images");

        String BmaskDir = Path.Combine(StgDir, "BMasks");

        if (Directory.Exists(TmpDir)) Directory.Delete(TmpDir, 1 > 0);

        if (Directory.Exists(StgDir)) Directory.Delete(StgDir, 1 > 0);

        Directory.CreateDirectory(TmpDir);

        Directory.CreateDirectory(StgDir);

        Directory.CreateDirectory(ImageDir);

        Directory.CreateDirectory(BmaskDir);

        return (ImageDir, BmaskDir, TmpDir);
    }

    public static Dictionary<String, (String, String)> GetFilePairs(String Dir, String[] Imgs, String[] Txts)
    {
        Dictionary<String, (String, String)> AllPath = [];

        Dictionary<String, String> ImagePaths = [];

        Dictionary<String, String> LabelPaths = [];

        foreach (String ElementPathInDir in Directory.EnumerateFiles(Dir, "*", SearchOption.AllDirectories))
        {
            String Name = Path.GetFileNameWithoutExtension(ElementPathInDir);

            String? GetTail = Path.GetExtension(ElementPathInDir)?.ToUpper();

            if (String.IsNullOrEmpty(GetTail)) continue;

            if (Imgs.Contains(GetTail)) ImagePaths[Name] = ElementPathInDir;

            if (Txts.Contains(GetTail)) LabelPaths[Name] = ElementPathInDir;
        }

        String[] AllKeys = [.. ImagePaths.Keys.Union(LabelPaths.Keys)];

        foreach (String Key in AllKeys)
        {
            ImagePaths.TryGetValue(Key, out String? Img);

            LabelPaths.TryGetValue(Key, out String? Txt);

            AllPath[Key] = (Img ?? String.Empty, Txt ?? String.Empty);
        }

        return AllPath;
    }

    public static void CopyAndCompressFiles(String Dir, String[] FileNames, String[] Zips)
    {
        Boolean IsCompressedFile(String FPath)

                                     => Zips.Contains(Path.GetExtension(FPath).ToUpper());

        Queue<String> WaitingZipList = new();

        foreach (String FilePath in FileNames)
        {
            String NewPath = Path.Combine(Dir, Path.GetFileName(FilePath));

            File.Copy(FilePath, NewPath, true);

            if (IsCompressedFile(NewPath)) WaitingZipList.Enqueue(NewPath);
        }

        while (WaitingZipList.Count != 0)
        {
            String DequeuePath = WaitingZipList.Dequeue();

            String ZDirPath = Path.ChangeExtension(DequeuePath, null);

            using IArchive Archive = ArchiveFactory.Open(DequeuePath);

            Archive.WriteToDirectory(ZDirPath, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true });

            foreach (String ElementPathInZip in Directory.EnumerateFiles(ZDirPath, "*", SearchOption.AllDirectories))
            {
                if (IsCompressedFile(ElementPathInZip)) WaitingZipList.Enqueue(ElementPathInZip);
            }

            File.Delete(DequeuePath);
        }
    }

    //public static (String, String, String)[] PrepareData((String[], String[], String[]) Types, String[] FileNames)
    //{
    //    (String ImageDir, String BmaskDir, String TempDir) = CreateDirectoryTree();

    //    CopyAndCompressFiles(TempDir, FileNames, Types.Item1);

    //    Dictionary<String, (String, String)> FindFilePairsInDir = GetFilePairs(TempDir, Types.Item2, Types.Item3);
    //}
}