namespace SewerDeformationSoftware.Logics.ViewModels;

public class Build : Basis
{
    String MODELNAME = String.Empty;

    String MODELPATH = String.Empty;

    String GETDIVICE = String.Empty;

    public String ModelName { get => MODELNAME; set { MODELNAME = value; OnPropertyChanged(); } }

    public String ModelPath { get => MODELPATH; set { MODELPATH = value; OnPropertyChanged(); } }

    public String GetDevice { get => GETDIVICE; set { GETDIVICE = value; OnPropertyChanged(); } }

    public ICommand Browse { get; set; } = null!;

    public ICommand Remove { get; set; } = null!;

    public ICommand Accept { get; set; } = null!;

    public ICommand Dragop { get; set; } = null!;
    public Build()
    {
        Dragop = new RelayCommand<Object>(Obj => Obj != null, ModelFile => DropModel(ModelFile));

        Browse = new RelayCommand<Object>(Obj => Obj == null, Obj => BrowseModel());

        Remove = new RelayCommand<Object>(Obj => Obj == null, Obj => RemoveModel());

        Accept = new RelayCommand<Object>(Obj => CanAccept(), async Obj => await AcceptModel());
    }

    Boolean CanAccept() => !String.IsNullOrEmpty(ModelPath) && !String.IsNullOrEmpty(GetDevice);

    Task DropModel(Object ObjModel)
    {
        if (ObjModel is DragEventArgs Event && Event.Data.GetDataPresent(DataFormats.FileDrop))
        {
            String[] Files = (String[])Event.Data.GetData(DataFormats.FileDrop);

            if (Files.Length == 1)
            {
                StringComparison SameChars = StringComparison.OrdinalIgnoreCase;

                if (Files.Length > 0 && Path.GetExtension(Files[0]).Equals(".ONNX", SameChars))
                {
                    ModelName = Path.GetFileName(Files[0]);

                    ModelPath = Files[0];
                }
                else
                {
                    Message.ShowErrors("Tệp tin không hợp lệ hoặc không đúng định dạng .ONNX");
                }
            }
            else
            {
                Message.ShowErrors("Chức năng này chỉ cho phép nhận vào một tệp tin duy nhất");
            }
        }

        return Task.CompletedTask;
    }

    Task BrowseModel()
    {
        OpenFileDialog FileSelection = new() { Filter = "YOLO Model ONNX | *.ONNX" };

        Boolean Choose = (Boolean)FileSelection.ShowDialog(Windows.TopmostWindow())!;

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
            ModelPath = String.Empty;

            ModelName = String.Empty;

            YOLOSeg.Models.CenterModel = null;

            YOLOSeg.Models.UseHardware = null;
        }

        return Task.CompletedTask;
    }

    async Task AcceptModel()
    {
        YoloPredictorOptions YOptions = new()
        {
            UseCuda = GetDevice.Equals("GPU")
        };

        Waiting.ShowProgressRing();

        await Task.Run(() => YOLOSeg.Models.CenterModel = new(ModelPath, YOptions));

        Waiting.HideProgressRing();
    }
}