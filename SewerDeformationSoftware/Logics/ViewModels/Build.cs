
namespace SewerDeformationSoftware.Logics.ViewModels;

public class Build : Basis
{
    String MODELNAME = String.Empty;

    String MODELPATH = String.Empty;

    public String ModelName { get => MODELNAME; set { MODELNAME = value; OnPropertyChanged(); } }

    public String ModelPath { get => MODELPATH; set { MODELPATH = value; OnPropertyChanged(); } }

    public ICommand Browse { get; set; } = null!;

    public ICommand Remove { get; set; } = null!;

    public Build()
    {
        Browse = new RelayCommand<Object>(Obj => Obj == null, Obj => BrowseModel());

        Remove = new RelayCommand<Object>(Obj => Obj == null, Obj => RemoveModel());
    }

    Task BrowseModel()
    {
        OpenFileDialog FileSelection = new() { Filter = "YOLO File .ONNX | *.ONNX" };

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
        ModelPath = String.Empty;

        ModelName = String.Empty;

        return Task.CompletedTask;

    }
}