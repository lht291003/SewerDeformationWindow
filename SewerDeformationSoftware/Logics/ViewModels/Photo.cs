namespace SewerDeformationSoftware.Logics.ViewModels;

public class Photo : Basis
{
    String PHOTONAME = String.Empty;

    String PHOTOPATH = String.Empty;

    BitmapSource? PLOTIMAGE = null;

    BitmapSource? MASKIMAGE = null;

    public BitmapSource? PlotImage { get => PLOTIMAGE; set { PLOTIMAGE = value; OnPropertyChanged(); } }

    public BitmapSource? MaskImage { get => MASKIMAGE; set { MASKIMAGE = value; OnPropertyChanged(); } }

    public String PhotoName { get => PHOTONAME; set { PHOTONAME = value; OnPropertyChanged(); } }

    public String PhotoPath { get => PHOTOPATH; set { PHOTOPATH = value; OnPropertyChanged(); } }

    public ICommand Browse { get; set; } = null!;

    public ICommand Remove { get; set; } = null!;

    public ICommand Accept { get; set; } = null!;

    public ICommand Dragop { get; set; } = null!;

    public Photo()
    {
        Dragop = new RelayCommand<Object>(Obj => Obj != null, ModelFile => DropModel(ModelFile));

        Browse = new RelayCommand<Object>(Obj => Obj == null, Obj => BrowseModel());

        Remove = new RelayCommand<Object>(Obj => Obj == null, Obj => RemoveModel());
    }

    Task BrowseModel()
    {
        OpenFileDialog FileSelection = new();

        FileSelection.Filter = "Image Files |*.PNG;*.JPG;*.JPEG;*.WEBP;*.BMP;*.TIFF";

        Boolean Choose = (Boolean)FileSelection.ShowDialog(Windows.TopmostWindow())!;

        if (Choose)
        {
            PhotoName = Path.GetFileName(FileSelection.FileName);

            PhotoPath = FileSelection.FileName;
        }

        return Task.CompletedTask;
    }

    Task RemoveModel()
    {
        if (Message.ShowConfirm("Bạn có muốn xóa hình ảnh này?"))
        {
            PlotImage = null;

            MaskImage = null;

            PhotoPath = String.Empty;

            PhotoName = String.Empty;
        }

        return Task.CompletedTask;
    }

    Task DropModel(Object ObjModel)
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
}