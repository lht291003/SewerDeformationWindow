namespace SewerDeformationSoftware.Logics.ViewModels;

public class Video : Basis
{
    List<ValueTuple<BitmapSource, BitmapSource, Mat, Dictionary<String, Object>?>> SegmentLogs { get; set; } = [];

    public String VideoName { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String VideoPath { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public BitmapSource? NPltImage { get; set => SetAndNotify(value, ref field); } = null;

    public BitmapSource? PPltImage { get; set => SetAndNotify(value, ref field); } = null;

    public BitmapSource? MaskImage { get; set => SetAndNotify(value, ref field); } = null;

    public Int32 Distance { get; set => SetAndNotify(value, ref field); } = 5;

    public Boolean IsCompleted { get; set => SetAndNotify(value, ref field); } = false;

    public Boolean IsCommenced { get; set => SetAndNotify(value, ref field); } = false;

    public String Shape { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String State { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Phase { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Delta { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String AspectRatio { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Orientation { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Deformation { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Resemblance { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Distinction { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public ICommand Browse { get; set; } = null!;

    public ICommand Remove { get; set; } = null!;

    public ICommand Accept { get; set; } = null!;

    public ICommand Dragop { get; set; } = null!;

    public ICommand CutOff { get; set; } = null!;

    public Video()
    {
        Dragop = new RelayCommand<Object>(Obj => Obj != null, ModelFile => DropVideo(ModelFile));

        Browse = new RelayCommand<Object>(Obj => Obj == null, Obj => BrowseVideo());

        Remove = new RelayCommand<Object>(Obj => Obj == null, Obj => RemoveVideo());

        CutOff = new RelayCommand<Object>(Obj => Obj == null, Obj => CutOffVideo());

        Accept = new RelayCommand<Object>(Obj => Obj == null, async Obj => await AnalyzeVideo());
    }

    Task BrowseVideo()
    {
        OpenFileDialog FileSelection = new() { Filter = "Video File | *.MP4;*.MOV;*.WEBM" };

        Boolean SelectedVideo = (Boolean)FileSelection.ShowDialog(Windows.TopmostWindow())!;

        if (SelectedVideo)
        {
            VideoPath = FileSelection.FileName;

            VideoName = Path.GetFileName(FileSelection.FileName);
        }

        return Task.CompletedTask;
    }

    Task RemoveVideo()
    {
        if (Message.ShowConfirm("Bạn có muốn gỡ bỏ video này?"))
        {
            NPltImage = null;

            PPltImage = null;

            MaskImage = null;

            VideoPath = String.Empty;

            VideoName = String.Empty;

            Shape = String.Empty;

            State = String.Empty;

            Phase = String.Empty;

            Delta = String.Empty;

            AspectRatio = String.Empty;

            Orientation = String.Empty;

            Deformation = String.Empty;

            Distinction = String.Empty;

            Resemblance = String.Empty;
        }

        return Task.CompletedTask;
    }

    Task DropVideo(Object ObjModel)
    {
        if (ObjModel is DragEventArgs Event && Event.Data.GetDataPresent(DataFormats.FileDrop))
        {
            String[] Files = (String[])Event.Data.GetData(DataFormats.FileDrop);

            if (Files.Length == 1)
            {
                StringComparison SameChars = StringComparison.OrdinalIgnoreCase;

                String[] Valid = [".MP4", ".MOV", ".WEBM"];

                if (Valid.Any(Object => Path.GetExtension(Files[0]).Equals(Object, SameChars)))
                {
                    VideoName = Path.GetFileName(Files[0]);

                    VideoPath = Files[0];
                }
                else
                {
                    Message.ShowErrors("Tệp tin không đúng định dạng video được yêu cầu");
                }
            }
            else
            {
                Message.ShowErrors("Chức năng này chỉ có thể nhận vào một video đầu vào");
            }
        }

        return Task.CompletedTask;
    }

    Task CutOffVideo()
    {
        if (IsCommenced)
        {
            IsCommenced = default;
        }

        return Task.CompletedTask;
    }

    async Task AnalyzeVideo()
    {
        if (YOLOSeg.Models.KeyYSModel != null)
        {
            SegmentLogs.ForEach(X => X.Item3?.Dispose());

            SegmentLogs.Clear();

            using VideoCapture Captures = new(VideoPath);

            using Mat Frame = new();

            if (Captures.IsOpened())
            {
                IsCompleted = false;

                IsCommenced = true;

                YOLOSeg.Models.IsRunning = IsCommenced;

                Int32 FrmCount = 0;

                while (IsCommenced)
                {
                    if (Captures.Read(Frame))
                    {
                        FrmCount++;

                        ValueTuple<Mat, Mat, RotatedRect?, Dictionary<String, Object>?> Result = await Analysis.Quantifies(YOLOSeg.Models.KeyYSModel, Frame);

                        using Mat DrawedPlotImage = Result.Item1;

                        Mat BinaryMaskImage = Result.Item2;

                        RotatedRect? SavedEllipse = Result.Item3;

                        Dictionary<String, Object>? Specifications = Result.Item4;

                        NPltImage = DrawedPlotImage.ToBitmapSource();

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

                        ValueTuple<BitmapSource, BitmapSource, Mat, Dictionary<String, Object>?> Record;

                        Record.Item1 = NPltImage;

                        Record.Item2 = MaskImage != null ? MaskImage : BinaryMaskImage.ToBitmapSource();

                        (Record.Item3, Record.Item4) = (BinaryMaskImage, Result.Item4);

                        SegmentLogs.Add(Record);

                        if (FrmCount > Distance)
                        {
                            ValueTuple<BitmapSource, BitmapSource, Mat, Dictionary<String, Object>?> PreLog = SegmentLogs.ElementAt(FrmCount - Distance - 1);

                            PPltImage = PreLog.Item1;
                        }
                    }
                    else
                    {
                        IsCommenced = false;
                    }
                }

                IsCompleted = true;
            }
            else
            {
                IsCommenced = false;

                IsCompleted = false;
            }

            YOLOSeg.Models.IsRunning = false;
        }
    }
}