namespace SewerDeformationSoftware.Logics.Cores;

public class Compute
{
    public static async Task<(Mat, Mat, RotatedRect?, Dictionary<String, Object>?)> Quantifies(YoloPredictor Model, Mat MatImageData)
    {
        using Image LoadImage = Mat2SLImageSharp(MatImageData);

        return await Quantifies(Model, LoadImage);
    }

    public static async Task<(Mat, Mat, RotatedRect?, Dictionary<String, Object>?)> Quantifies(YoloPredictor Model, String ImagePath)
    {
        using Image LoadImage = await Image.LoadAsync(ImagePath);

        return await Quantifies(Model, LoadImage);
    }

    public static async Task<(Mat, Mat, RotatedRect?, Dictionary<String, Object>?)> Quantifies(YoloPredictor Model, Stream ImageData)
    {
        using Image LoadImage = await Image.LoadAsync(ImageData);

        return await Quantifies(Model, LoadImage);
    }

    private static async Task<(Mat, Mat, RotatedRect?, Dictionary<String, Object>?)> Quantifies(YoloPredictor Model, Image LoadImage)
    {
        YoloResult<Segmentation> GetSegResult = await Model.SegmentAsync(LoadImage);

        Mat Drawed = await GetMatImage(GetSegResult, LoadImage);

        if (GetSegResult.Count != 0)
        {
            Segmentation FirstMask = GetSegResult.MaxBy(Obj => (Obj.Bounds.Width * Obj.Bounds.Height))!;

            using Mat CreateBMask = CreateBinaryMaskImage(FirstMask, Drawed.Size());

            (Mat ProcessedMask, RotatedRect Ellipse, Dictionary<String, Object> Specifications)? Data = GetAnalizedMask(CreateBMask);

            if (Data != null == true)
            {
                return (Drawed, Data.Value.ProcessedMask, Data.Value.Ellipse, Data.Value.Specifications);
            }
        }

        return (Drawed, CreateZeroMask(Drawed.Size(), MatType.CV_8UC1), null, null);
    }

    public static Mat CreateBinaryMaskImage(Segmentation SegObject, Size ImageSize)
    {
        Mat GrayMask = CreateZeroMask(ImageSize, MatType.CV_8UC1);

        BitmapBuffer MaskData = SegObject.Mask;

        Rectangle BoxFrame = SegObject.Bounds;

        (Int32 BBW, Int32 BBH) = (BoxFrame.Width, BoxFrame.Height);

        (Int32 GMW, Int32 GMH) = (GrayMask.Width, GrayMask.Height);

        (Int32 MBW, Int32 MBH) = (MaskData.Width, MaskData.Height);

        Single[] Arr1D = new Single[MBW * MBH];

        Int32 I = 0;

        for (Int32 IY = 0; IY < MBH; IY++)
        {
            for (Int32 IX = 0; IX < MBW; IX++)
            {
                Arr1D[I++] = MaskData[IY, IX];
            }
        }

        using Mat SmallMask = Mat.FromPixelData(MBH, MBW, MatType.CV_32FC1, Arr1D);

        using Mat TempMask = new();

        using Mat Mask8Bit = new();

        Cv2.Resize(SmallMask, TempMask, new Size(BBW, BBH));

        Cv2.ConvertScaleAbs(TempMask, Mask8Bit, 255);

        Int32 SrcRoiX = (BoxFrame.X < 0) ? -BoxFrame.X : 0;

        Int32 SrcRoiY = (BoxFrame.Y < 0) ? -BoxFrame.Y : 0;

        Int32 DesRoiX = Math.Max(0, BoxFrame.X);

        Int32 DesRoiY = Math.Max(0, BoxFrame.Y);

        Int32 DesRoiW = Math.Min(BBW, GMW - DesRoiX);

        Int32 DesRoiH = Math.Min(BBH, GMH - DesRoiY);

        (Int32 CopyW, Int32 CopyH) = (Math.Min(DesRoiW, Mask8Bit.Width - SrcRoiX), Math.Min(DesRoiH, Mask8Bit.Height - SrcRoiY));

        if (CopyW > 0 && CopyH > 0)
        {
            using Mat SrcRoi = new(Mask8Bit, new Rect(SrcRoiX, SrcRoiY, CopyW, CopyH));

            using Mat DesRoi = new(GrayMask, new Rect(DesRoiX, DesRoiY, CopyW, CopyH));

            SrcRoi.CopyTo(DesRoi);
        }

        return GrayMask;
    }

    public static (RotatedRect, Mat) GetFittedEllipseMask(Mat Mask, Point[] Contour)
    {
        Mat EM = CreateZeroMask(Mask.Size(), Mask.Type());

        RotatedRect NewEllipse = Cv2.FitEllipse(Contour);

        Cv2.Ellipse(EM, NewEllipse, new Scalar(255), -1);

        return (NewEllipse, EM);
    }

    public static Point[]? GetMaxContour(Mat Mask)
    {
        Cv2.FindContours(Mask, out Point[][] Contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        if (Contours.Length != 0)
        {
            Point[]? MaxCnt = Contours.MaxBy(Cnt => Cv2.ContourArea(Cnt));

            return (MaxCnt != null && MaxCnt.Length >= 5) ? MaxCnt : null;
        }

        return null;
    }

    public static Double GetIoU(Mat Mask1, Mat Mask2)
    {
        Double GetIoU(Mat Mask1, Mat Mask2)
        {
            Double M1Area = Cv2.CountNonZero(Mask1);

            Double M2Area = Cv2.CountNonZero(Mask2);

            if (M1Area > 0 && M2Area > 0)
            {
                using Mat IMask = new();

                Cv2.BitwiseAnd(Mask1, Mask2, IMask);

                Double IArea = Cv2.CountNonZero(IMask);

                Double UArea = M1Area + M2Area - IArea;

                if (UArea != 0) return (IArea / UArea);
            }

            return 0.0;
        }

        if (Mask1.Size() == Mask2.Size())
        {
            return GetIoU(Mask1, Mask2);
        }
        else
        {
            using Mat M1 = Mask1.Clone();

            using Mat M2 = Mask2.Clone();

            Int64 Area1 = (Int64)M1.Width * M1.Height;

            Int64 Area2 = (Int64)M2.Width * M2.Height;

            if (Area1 <= Area2)
            {
                Cv2.Resize(M1, M1, M2.Size(), 0, 0, InterpolationFlags.Nearest);
            }
            else
            {
                Cv2.Resize(M2, M2, M1.Size(), 0, 0, InterpolationFlags.Nearest);
            }

            return GetIoU(M1, M2);
        }
    }

    public static Double GetRealAngle(Size2f Ax, Double Angle)
    {
        if (Ax.Width < Ax.Height)
        {
            return (Angle > 90) ? (Angle - 90) : (Angle + 90);
        }

        return Angle;
    }

    public static String GetConclude(String Shape)
    {
        if (String.IsNullOrEmpty(Shape) == false)
        {
            return Shape == "Circle" ? "Normal" : "Deformed";
        }

        return String.Empty;
    }

    public static String GetReformattedTheta(Double OrgAngle)
    {
        Double Angle = Math.Round(OrgAngle, 2);

        return (Angle >= 80 && Angle <= 110) ? $"Vertical {Angle}" : $"Horizontal {Angle}";
    }

    public async static Task<Mat> GetMatImage(YoloResult<Segmentation> Result, Image Data)
    {
        using Image PlottedImage = await Result.PlotImageAsync(Data);

        using Image<Bgr24> BGR24Img = PlottedImage.CloneAs<Bgr24>();

        if (BGR24Img.DangerousTryGetSinglePixelMemory(out Memory<Bgr24> Bgr24PixelMemory))
        {
            unsafe
            {
                using MemoryHandle PinMemory = Bgr24PixelMemory.Pin();

                using Mat Wrapper = Mat.FromPixelData(BGR24Img.Height, BGR24Img.Width, MatType.CV_8UC3, (IntPtr)PinMemory.Pointer);

                return Wrapper.Clone();
            }
        }

        Byte[] Pixels = new Byte[BGR24Img.Width * BGR24Img.Height * 3];

        BGR24Img.CopyPixelDataTo(Pixels);

        return Mat.FromPixelData(BGR24Img.Height, BGR24Img.Width, MatType.CV_8UC3, Pixels);
    }

    public static unsafe Image Mat2SLImageSharp(Mat Data)
    {
        if (!Data.IsContinuous())
        {
            using Mat ContinuousMat = Data.Clone();

            return Mat2SLImageSharp(ContinuousMat);
        }

        Int64 Len = Data.Total() * Data.ElemSize();

        ReadOnlySpan<Byte> MatSpan = new(Data.Data.ToPointer(), (Int32)Len);

        return Image.LoadPixelData<Bgr24>(MatSpan, Data.Width, Data.Height);
    }

    public static (Mat, RotatedRect, Dictionary<String, Object>)? GetAnalizedMask(Mat Mask)
    {
        String Shape, Orientation = String.Empty;

        Double AspectRatio = -1;

        Double Deformation = -1;

        Point[]? MaxContour = GetMaxContour(Mask);

        if (MaxContour != null)
        {
            ValueTuple<RotatedRect, Mat> VEllipse = GetFittedEllipseMask(Mask, MaxContour);

            RotatedRect Ellipse = VEllipse.Item1;

            using Mat StdElMask = VEllipse.Item2;

            (Size2f Axes, Double Angle) = (Ellipse.Size, Ellipse.Angle);

            Mat ProcessedMask = new();

            Cv2.BitwiseAnd(Mask, StdElMask, ProcessedMask);

            if (GetIoU(ProcessedMask, StdElMask) >= 0.9575)
            {
                Double MajorAxe = Math.Max(Axes.Width, Axes.Height);

                Double MinorAxe = Math.Min(Axes.Width, Axes.Height);

                AspectRatio = (MinorAxe / MajorAxe);

                if (AspectRatio < 0.99)
                {
                    Double ReadAngle = GetRealAngle(Axes, Angle);

                    Deformation = (1 - AspectRatio);

                    Orientation = GetReformattedTheta(ReadAngle);

                    Shape = "Ellipse";
                }
                else
                {
                    Shape = "Circle";
                }
            }
            else
            {
                Shape = "Undefined";
            }

            Dictionary<String, Object> Specifications = new()
            {
                { "Shape", Shape },

                {"State", GetConclude(Shape) },

                { "AspectRatio", AspectRatio },

                { "Orientation", Orientation },

                { "Deformation", Deformation },
            };

            return (ProcessedMask, Ellipse, Specifications);
        }
        else
        {
            return null;
        }
    }

    public static BitmapSource GetPlotMaskAsWPFBitmapImage(Mat Mask, RotatedRect? Ellipse, String Shape)
    {
        using Mat VisualMask = GetPlotMask(Mask, Ellipse, Shape);

        return VisualMask.ToBitmapSource();
    }

    public static Mat GetPlotMask(Mat Mask, RotatedRect? EllipseData, String Shape)
    {
        Mat VMask = Mask.Clone();

        Cv2.CvtColor(VMask, VMask, ColorConversionCodes.GRAY2BGR);

        if (Shape != "Undefined" && EllipseData.HasValue)
        {
            RotatedRect Ellipse = EllipseData.GetValueOrDefault();

            Cv2.Ellipse(VMask, Ellipse, new Scalar(0, 255, 0), 2);

            (Point2f Center, Size2f Axes, Double Angle) = (Ellipse.Center, Ellipse.Size, Ellipse.Angle);

            Double DXMajor = 0.0, DYMajor = 0.0, DXMinor = 0.0, DYMinor = 0.00;

            (Double XC, Double YC) = (Center.X, Center.Y);

            (Double SA, Double SB) = ((Axes.Width / 2.0), (Axes.Height / 2.0));

            if (Shape == "Ellipse")
            {
                Double Theta = Cv2.PI * Angle / 180;

                DXMajor = SA * Math.Cos(Theta);

                DYMajor = SA * Math.Sin(Theta);

                DXMinor = SB * Math.Cos(Theta + Cv2.PI / 2);

                DYMinor = SB * Math.Sin(Theta + Cv2.PI / 2);
            }

            if (Shape == "Circle")
            {
                (DXMajor, DXMinor) = (SA, 0);

                (DYMajor, DYMinor) = (0, SB);
            }

            Point PT1Major = new((Int32)(XC - DXMajor), (Int32)(YC - DYMajor));

            Point PT2Major = new((Int32)(XC + DXMajor), (Int32)(YC + DYMajor));

            Point PT1Minor = new((Int32)(XC - DXMinor), (Int32)(YC - DYMinor));

            Point PT2Minor = new((Int32)(XC + DXMinor), (Int32)(YC + DYMinor));

            Cv2.Line(VMask, PT1Major, PT2Major, new Scalar(0, 0, 255), 2);

            Cv2.Line(VMask, PT1Minor, PT2Minor, new Scalar(255, 0, 0), 2);

            Cv2.Line(VMask, new Point(0, (Int32)YC), new Point(VMask.Cols, (Int32)YC), new Scalar(0, 99, 0), 2);

            Cv2.Line(VMask, new Point((Int32)XC, 0), new Point((Int32)XC, VMask.Rows), new Scalar(0, 99, 0), 2);
        }

        return VMask;
    }

    public static Mat CreateZeroMask(Size ImageSize, MatType Type)

                                    => Mat.Zeros(ImageSize, Type);

    public static String GetDelta(String OldOrientation, String NowOrientation)
    {
        Double ExtractAngle(String Orientation)
        {
            if (String.IsNullOrWhiteSpace(Orientation)) return 0.0;

            return Convert.ToDouble(Orientation.Split(" ").Last());
        }

        Double OldAngle = ExtractAngle(OldOrientation);

        Double NowAngle = ExtractAngle(NowOrientation);

        Double Delta = Math.Round(Math.Abs(NowAngle - OldAngle), 2);

        if (Delta == 0)
        {
            return "Unchanged Angle";
        }
        else
        {
            if (NowAngle > OldAngle)
            {
                return $"Increased Angle {Delta}";
            }
            else
            {
                return $"Decreased Angle {Delta}";
            }
        }
    }

    public static (String, String, String, String) Quantifies((Mat, Dictionary<String, Object>?) Now, (Mat, Dictionary<String, Object>?) Old)
    {
        Double IoU = GetIoU(Now.Item1, Old.Item1);

        Double Deformation = 1 - IoU;

        String Phase = (IoU < 0.98) ? "Deformed" : "Normal";

        String Delta = String.Empty;

        if (Now.Item2 != null && Old.Item2 != null)
        {
            Boolean GetOldShapeCond = Old.Item2["Shape"].ToString() != "Undefined";

            Boolean GetNowShapeCond = Now.Item2["Shape"].ToString() != "Undefined";

            if (GetOldShapeCond && GetNowShapeCond)
            {
                String OldOrientation = Old.Item2["Orientation"].ToString()!;

                String NowOrientation = Now.Item2["Orientation"].ToString()!;

                Delta = GetDelta(OldOrientation, NowOrientation);
            }
        }

        return ($"{(IoU * 100):F9} %", $"{(Deformation * 100):F9} %", Delta, Phase);
    }
}