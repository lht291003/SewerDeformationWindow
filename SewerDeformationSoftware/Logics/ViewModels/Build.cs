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

    public Build()
    {
        Browse = new RelayCommand<Object>(Obj => Obj == null, Obj => BrowseModel());

        Remove = new RelayCommand<Object>(Obj => Obj == null, Obj => RemoveModel());

        Accept = new RelayCommand<Object>(Obj => CanAccept(), Obj => AcceptModel());
    }

    Boolean CanAccept()

            => !String.IsNullOrEmpty(ModelPath) && !String.IsNullOrEmpty(GetDevice);

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
        }

        return Task.CompletedTask;
    }

    Task AcceptModel()
    {
        return Task.CompletedTask;
    }
}