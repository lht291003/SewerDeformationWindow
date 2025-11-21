namespace SewerDeformationSoftware.Logics.ViewModels;

public class Evaln : Basis
{
    (String[] Zips, String[] Imgs, String[] Txts) Types = ([".ZIP", ".RAR", ".TAR", ".7Z"], [".PNG", ".JPG", ".JPEG", ".WEBP", ".BMP", ".TIFF"], [".TXT"]);

    public (String, String, String)[] TestData { get; set => SetAndNotify(value, ref field); } = null!;

    public ObservableCollection<String> FileNames { get; set => SetAndNotify(value, ref field); } = [];

    public String ModelName { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String ModelPath { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public ICommand BrowseDatasets { get; set; } = null!;

    public ICommand BrowseSegModel { get; set; } = null!;

    public ICommand Accept { get; set; } = null!;

    public ICommand DragopDatasets { get; set; } = null!;

    public ICommand DragopSegModel { get; set; } = null!;

    public ICommand RemoveDatasets { get; set; } = null!;

    public ICommand RemoveSegModel { get; set; } = null!;

    public Evaln()
    {
        BrowseDatasets = new RelayCommand<Object>(Obj => Obj == null, Obj => BrowseFiles());

        BrowseSegModel = new RelayCommand<Object>(Obj => Obj == null, Obj => BrowseModel());

        RemoveDatasets = new RelayCommand<String>(Obj => Obj != null, S => RemoveFiles(S));

        RemoveSegModel = new RelayCommand<Object>(Obj => Obj == null, Obj => RemoveModel());

        Accept = new RelayCommand<Object>(Obj => Obj == null, async Obj => await CompareAndEvaluate());
    }

    Task BrowseFiles()
    {
        String Exts = "Dataset | *.ZIP;*.RAR;*.TAR;*.7Z;*.TXT;*.PNG;*.JPG;*.JPEG;*.WEBP;*.BMP;*.TIFF";

        OpenFileDialog FileSelections = new() { Filter = Exts, Multiselect = true };

        Boolean IsOK = (Boolean)FileSelections.ShowDialog(Interop.TopmostWindow())!;

        if (IsOK)
        {
            for (Int32 Index = 0; Index < FileSelections.FileNames.Length; Index++)
            {
                if (!FileNames.Contains(FileSelections.FileNames.ElementAt(Index)))
                {
                    FileNames.Add(FileSelections.FileNames.ElementAt(Index));
                }
            }
        }

        return Task.CompletedTask;
    }

    Task RemoveFiles(String Image)
    {
        FileNames.Remove(Image);

        return Task.CompletedTask;
    }

    Task BrowseModel()
    {
        OpenFileDialog FileSelection = new() { Filter = "YOLO Model ONNX | *.ONNX" };

        Boolean Choose = (Boolean)FileSelection.ShowDialog(Interop.TopmostWindow())!;

        if (Choose)
        {
            ModelName = Path.GetFileName(FileSelection.FileName);

            ModelPath = FileSelection.FileName;
        }

        return Task.CompletedTask;
    }

    Task RemoveModel()
    {
        if (Message.ShowConfirm("Bạn có muốn xóa mô hình này?"))
        {
            if (YOLOSeg.Models.IsRunning)
            {
                Message.ShowErrors("Không thể xóa, mô hình này đang được suy luận!");
            }
            else
            {
                ModelPath = String.Empty;

                ModelName = String.Empty;

                YOLOSeg.Models.SecYSModel = null;

                YOLOSeg.Models.DeviceType = null;
            }
        }

        return Task.CompletedTask;
    }

    async Task CompareAndEvaluate()
    {

    }
}