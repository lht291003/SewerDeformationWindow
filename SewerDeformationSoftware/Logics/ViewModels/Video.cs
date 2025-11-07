namespace SewerDeformationSoftware.Logics.ViewModels;

public class Video : Basis
{
    public String VideoName { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String VideoPath { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public BitmapSource? NPltImage { get; set => SetAndNotify(value, ref field); } = null;

    public BitmapSource? PPltImage { get; set => SetAndNotify(value, ref field); } = null;

    public BitmapSource? MaskImage { get; set => SetAndNotify(value, ref field); } = null;

    public Boolean IsCompleted { get; set => SetAndNotify(value, ref field); } = false;

    public Boolean IsCommenced { get; set => SetAndNotify(value, ref field); } = false;

    public String Shape { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String State { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Phase { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Delta { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public Int32 Distance { get; set => SetAndNotify(value, ref field); } = 10;

    public String AspectRatio { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Orientation { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Deformation { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Resemblance { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String Distinction { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String VReportPath { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public String EReportPath { get; set => SetAndNotify(value, ref field); } = String.Empty;

    public WpfPlot DePlotChart { get; } = new WpfPlot();

    public WpfPlot SpPlotChart { get; } = new WpfPlot();

    public ICommand Browse { get; set; } = null!;

    public ICommand Remove { get; set; } = null!;

    public ICommand Accept { get; set; } = null!;

    public ICommand Dragop { get; set; } = null!;

    public ICommand CutOff { get; set; } = null!;

    List<ValueTuple<Mat, Mat, Mat, Dictionary<String, Object>?>> SegmentLogs { get; set; } = [];

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

        Boolean SelectedVideo = (Boolean)FileSelection.ShowDialog(WinHelp.TopmostWindow())!;

        if (SelectedVideo)
        {
            VideoPath = FileSelection.FileName;

            VideoName = Path.GetFileName(FileSelection.FileName);
        }

        return Task.CompletedTask;
    }

    Task RefeshDatas()
    {
        Shape = String.Empty;

        State = String.Empty;

        Phase = String.Empty;

        Delta = String.Empty;

        NPltImage = null;

        PPltImage = null;

        MaskImage = null;

        AspectRatio = String.Empty;

        Orientation = String.Empty;

        Deformation = String.Empty;

        Distinction = String.Empty;

        Resemblance = String.Empty;

        VReportPath = String.Empty;

        EReportPath = String.Empty;

        return Task.CompletedTask;
    }

    Task RemoveVideo()
    {
        if (Message.ShowConfirm("Bạn có muốn gỡ bỏ video này?"))
        {
            VideoPath = String.Empty;

            VideoName = String.Empty;
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
            using VideoCapture Captures = new(VideoPath);

            using Mat Frame = new();

            if (Captures.IsOpened())
            {
                await RefeshDatas();

                IsCompleted = false;

                YOLOSeg.Models.IsRunning = !IsCompleted;

                IsCommenced = true;

                Int32 FrmCount = 0;

                while (IsCommenced)
                {
                    if (Captures.Read(Frame))
                    {
                        FrmCount++;

                        (Mat, Mat, RotatedRect?, Dictionary<String, Object>?) Result = await Analysis.Quantifies(YOLOSeg.Models.KeyYSModel, Frame);

                        Mat DrawedPlotImage = Result.Item1;

                        Mat BinaryMaskImage = Result.Item2;

                        Mat DrawedMaskImage = Result.Item2;

                        RotatedRect? Ellipse = Result.Item3;

                        Dictionary<String, Object>? Specifications = Result.Item4;

                        NPltImage = DrawedPlotImage.ToBitmapSource();

                        Shape = Specifications?["Shape"].ToString()!;

                        State = Specifications?["State"].ToString()!;

                        Orientation = Specifications?["Orientation"].ToString()!;

                        Double GetAspectRatioValue = Convert.ToDouble(Specifications?["AspectRatio"]);

                        Double GetDeformationValue = Convert.ToDouble(Specifications?["Deformation"]);

                        AspectRatio = (GetAspectRatioValue == -1) ? String.Empty : $"{(GetAspectRatioValue * 100):F9} %";

                        Deformation = (GetDeformationValue == -1) ? String.Empty : $"{(GetDeformationValue * 100):F9} %";

                        if (Ellipse == null)
                        {
                            MaskImage = null;
                        }
                        else
                        {
                            DrawedMaskImage = Analysis.GetPlotMask(BinaryMaskImage, Ellipse.GetValueOrDefault(), Shape);

                            MaskImage = DrawedMaskImage.ToBitmapSource();
                        }

                        (Mat, Mat, Mat, Dictionary<String, Object>?) Record = (DrawedPlotImage, DrawedMaskImage, BinaryMaskImage, Specifications);

                        SegmentLogs.Add(Record);

                        if (FrmCount > Distance)
                        {
                            (Mat Display, Mat, Mat, Dictionary<String, Object>?) PreLog = SegmentLogs.ElementAtOrDefault(FrmCount - Distance - 1);

                            PPltImage = PreLog.Display.ToBitmapSource();

                            (String, String, String, String) Pp = Analysis.Quantifies((Result.Item2, Result.Item4), (PreLog.Item3, PreLog.Item4));

                            Resemblance = Pp.Item1;

                            Distinction = Pp.Item2;

                            Delta = Pp.Item3;

                            Phase = Pp.Item4;
                        }

                        NPltImage?.Freeze();

                        PPltImage?.Freeze();

                        MaskImage?.Freeze();
                    }
                    else
                    {
                        IsCommenced = false;
                    }
                }

                IsCompleted = true;

                Dictionary<String, Object>?[] SpecificationList = [.. SegmentLogs.Select(X => X.Item4)];

                Visualization.ShowSpChart(SpPlotChart, SpecificationList);

                Visualization.ShowDeChart(DePlotChart, SpecificationList);

                await Task.Run(() => { (VReportPath, EReportPath) = Storage.GetReport(SegmentLogs); });
            }
            else
            {
                IsCommenced = false;

                IsCompleted = false;

            }

            YOLOSeg.Models.IsRunning = false;

            SegmentLogs.ForEach(PerRecord =>
            {
                PerRecord.Item1?.Dispose();

                PerRecord.Item2?.Dispose();

                PerRecord.Item3?.Dispose();
            });

            SegmentLogs.Clear();
        }
        else
        {
            Message.ShowErrors("Không thể thực hiện suy luận video do mô hình hiện chưa được tải lên!");
        }
    }
}