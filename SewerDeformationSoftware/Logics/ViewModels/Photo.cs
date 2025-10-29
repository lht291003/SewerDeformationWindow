namespace SewerDeformationSoftware.Logics.ViewModels;

public class Photo : Basis
{
    String PHOTONAME = String.Empty;

    String PHOTOPATH = String.Empty;

    BitmapSource? PLOTIMAGE = null;

    BitmapSource? MASKIMAGE = null;

    String SHAPE = String.Empty;

    String STATE = String.Empty;

    String ASPECTRATIO = String.Empty;

    String ORIENTATION = String.Empty;

    String DEFORMATION = String.Empty;

    public BitmapSource? PlotImage { get => PLOTIMAGE; set { PLOTIMAGE = value; OnPropertyChanged(); } }

    public BitmapSource? MaskImage { get => MASKIMAGE; set { MASKIMAGE = value; OnPropertyChanged(); } }

    public String PhotoName { get => PHOTONAME; set { PHOTONAME = value; OnPropertyChanged(); } }

    public String PhotoPath { get => PHOTOPATH; set { PHOTOPATH = value; OnPropertyChanged(); } }

    public String Shape { get => SHAPE; set { SHAPE = value; OnPropertyChanged(); } }

    public String State { get => STATE; set { STATE = value; OnPropertyChanged(); } }

    public String AspectRatio { get => ASPECTRATIO; set { ASPECTRATIO = value; OnPropertyChanged(); } }

    public String Orientation { get => ORIENTATION; set { ORIENTATION = value; OnPropertyChanged(); } }

    public String Deformation { get => DEFORMATION; set { DEFORMATION = value; OnPropertyChanged(); } }

    public ICommand Browse { get; set; } = null!;

    public ICommand Remove { get; set; } = null!;

    public ICommand Accept { get; set; } = null!;

    public ICommand Dragop { get; set; } = null!;

    public Photo()
    {
        Dragop = new RelayCommand<Object>(Obj => Obj != null, ModelFile => DropPhoto(ModelFile));

        Browse = new RelayCommand<Object>(Obj => Obj == null, Obj => BrowsePhoto());

        Remove = new RelayCommand<Object>(Obj => Obj == null, Obj => RemovePhoto());

        Accept = new RelayCommand<Object>(Obj => Obj == null, async Obj => await AnalyzePhoto());
    }

    Task BrowsePhoto()
    {
        OpenFileDialog FileSelection = new();

        FileSelection.Filter = "Image File | *.PNG;*.JPG;*.JPEG;*.WEBP;*.BMP;*.TIFF";

        Boolean Choose = (Boolean)FileSelection.ShowDialog(Windows.TopmostWindow())!;

        if (Choose)
        {
            PhotoPath = FileSelection.FileName;

            PhotoName = Path.GetFileName(FileSelection.FileName);
        }

        return Task.CompletedTask;
    }

    Task RemovePhoto()
    {
        if (Message.ShowConfirm("Bạn có muốn xóa hình ảnh này?"))
        {
            PhotoPath = String.Empty;

            PhotoName = String.Empty;

            Shape = String.Empty;

            State = String.Empty;

            PlotImage = null;

            MaskImage = null;

            AspectRatio = String.Empty;

            Orientation = String.Empty;

            Deformation = String.Empty;
        }

        return Task.CompletedTask;
    }

    Task DropPhoto(Object ObjModel)
    {
        if (ObjModel is DragEventArgs Event && Event.Data.GetDataPresent(DataFormats.FileDrop))
        {
            String[] Files = (String[])Event.Data.GetData(DataFormats.FileDrop);

            if (Files.Length == 1)
            {
                StringComparison SameChars = StringComparison.OrdinalIgnoreCase;

                String[] ValidExtensions = [".PNG", ".JPG", ".JPEG", ".WEBP", ".BMP", ".TIFF"];

                if (ValidExtensions.Any(E => Path.GetExtension(Files[0]).Equals(E, SameChars)))
                {
                    PhotoName = Path.GetFileName(Files[0]);

                    PhotoPath = Files[0];
                }
                else
                {
                    Message.ShowErrors("Tệp tin không đúng định dạng hình ảnh được yêu cầu");
                }
            }
            else
            {
                Message.ShowErrors("Chức năng này chỉ có thể nhận vào một hình ảnh đầu vào");
            }
        }

        return Task.CompletedTask;
    }

    async Task AnalyzePhoto()
    {
        if (YOLOSeg.Models.KeyYSModel != null)
        {
            ValueTuple<Mat, Mat, RotatedRect?, Dictionary<String, Object>?> Result = await Analysis.Quantifies(YOLOSeg.Models.KeyYSModel, PhotoPath);

            using Mat DrawedPlotImage = Result.Item1;

            using Mat BinaryMaskImage = Result.Item2;

            RotatedRect? SavedEllipse = Result.Item3;

            Dictionary<String, Object>? Specifications = Result.Item4;

            PlotImage = DrawedPlotImage.ToBitmapSource();

            Shape = Specifications?["Shape"].ToString()!;

            State = Specifications?["State"].ToString()!;

            Orientation = Specifications?["Orientation"].ToString()!;

            Double GetAspectRatioValue = Convert.ToDouble(Specifications?["AspectRatio"]);

            Double GetDeformationValue = Convert.ToDouble(Specifications?["Deformation"]);

            AspectRatio = (GetAspectRatioValue == -1) ? String.Empty : $"{(GetAspectRatioValue * 100):F9} %";

            Deformation = (GetDeformationValue == -1) ? String.Empty : $"{(GetDeformationValue * 100):F9} %";

            if (SavedEllipse != null)
            {
                MaskImage = Analysis.GetVisualMaskAsBitmapSource(BinaryMaskImage, SavedEllipse.Value, Shape);
            }
            else
            {
                MaskImage = default;
            }
        }
        else
        {
            Message.ShowErrors("Không thể thực hiện phân tích hình ảnh do mô hình hiện chưa được tải lên!");
        }
    }
}