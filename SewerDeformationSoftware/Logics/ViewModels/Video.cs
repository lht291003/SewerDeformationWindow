namespace SewerDeformationSoftware.Logics.ViewModels;

public class Video : Basis
{
    String VIDEONAME = String.Empty;

    String VIDEOPATH = String.Empty;

    Int32 DISTANCE = 10;

    Boolean ISCOMPLETED;

    Boolean ISCOMMENCED;

    BitmapSource? NPLTIMAGE = null;

    BitmapSource? PPLTIMAGE = null;

    BitmapSource? MASKIMAGE = null;

    String SHAPE = String.Empty;

    String STATE = String.Empty;

    String PHASE = String.Empty;

    String DELTA = String.Empty;

    String ASPECTRATIO = String.Empty;

    String ORIENTATION = String.Empty;

    String DEFORMATION = String.Empty;

    String RESEMBLANCE = String.Empty;

    String DISTINCTION = String.Empty;

    List<ValueTuple<BitmapSource, BitmapSource, Mat, Dictionary<String, Object>?>> SegmentLogs = [];

    public BitmapSource? NPltImage { get => NPLTIMAGE; set => SetAndNotify(value, ref NPLTIMAGE); }

    public BitmapSource? PPltImage { get => PPLTIMAGE; set => SetAndNotify(value, ref PPLTIMAGE); }

    public BitmapSource? MaskImage { get => MASKIMAGE; set => SetAndNotify(value, ref MASKIMAGE); }

    public Boolean IsCompleted { get => ISCOMPLETED; set => SetAndNotify(value, ref ISCOMPLETED); }

    public Boolean IsCommenced { get => ISCOMMENCED; set => SetAndNotify(value, ref ISCOMMENCED); }

    public String VideoName { get => VIDEONAME; set => SetAndNotify(value, ref VIDEONAME); }

    public String VideoPath { get => VIDEOPATH; set => SetAndNotify(value, ref VIDEOPATH); }

    public Int32 Distance { get => DISTANCE; set => SetAndNotify(value, ref DISTANCE); }

    public String Shape { get => SHAPE; set => SetAndNotify(value, ref SHAPE); }

    public String State { get => STATE; set => SetAndNotify(value, ref STATE); }

    public String Phase { get => PHASE; set => SetAndNotify(value, ref PHASE); }

    public String Delta { get => DELTA; set => SetAndNotify(value, ref DELTA); }

    public String AspectRatio { get => ASPECTRATIO; set => SetAndNotify(value, ref ASPECTRATIO); }

    public String Orientation { get => ORIENTATION; set => SetAndNotify(value, ref ORIENTATION); }

    public String Deformation { get => DEFORMATION; set => SetAndNotify(value, ref DEFORMATION); }

    public String Resemblance { get => RESEMBLANCE; set => SetAndNotify(value, ref RESEMBLANCE); }

    public String Distinction { get => DISTINCTION; set => SetAndNotify(value, ref DISTINCTION); }

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
            ClearSegmentLog();

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

    Task ClearSegmentLog()
    {
        SegmentLogs.ForEach(Log => Log.Item3?.Dispose());

        SegmentLogs.Clear();

        return Task.CompletedTask;
    }

    async Task AnalyzeVideo()
    {
        if (YOLOSeg.Models.KeyYSModel != null)
        {
            using VideoCapture Captures = new(VideoPath);

            using Mat Frame = new();

            await ClearSegmentLog();

            if (Captures.IsOpened())
            {
                IsCompleted = false;

                IsCommenced = true;

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
                            PPltImage = SegmentLogs.ElementAtOrDefault(FrmCount - Distance - 1).Item1;
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
        }
    }
}