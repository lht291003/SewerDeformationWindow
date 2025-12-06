namespace SewerDeformationSoftware.Logics.ViewModels;

public class Photo : Basis
{
    public String AspectRatio { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Orientation { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Deformation { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Shape { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String State { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String PhotoName { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String PhotoPath { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public BitmapSource? PlotImage { get; set => SetAndNotify(value, ref field); } = null;

    public BitmapSource? MaskImage { get; set => SetAndNotify(value, ref field); } = null;

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

        Boolean Choose = (Boolean)FileSelection.ShowDialog(Interop.TopmostWindow())!;

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
            (Mat, Mat, RotatedRect?, Dictionary<String, Object>?) Result = await Compute.Quantifies(YOLOSeg.Models.KeyYSModel, PhotoPath);

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

            AspectRatio = (GetAspectRatioValue == -1) ? String.Empty : $"{(GetAspectRatioValue * 100):F9}%";

            Deformation = (GetDeformationValue == -1) ? String.Empty : $"{(GetDeformationValue * 100):F9}%";

            if (SavedEllipse != null)
            {
                MaskImage = Compute.GetPlotMaskWithBitmapSource(BinaryMaskImage, SavedEllipse.Value, Shape);
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