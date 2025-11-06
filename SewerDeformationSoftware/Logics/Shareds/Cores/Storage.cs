namespace SewerDeformationSoftware.Logics.Shareds.Cores;

public class Storage
{
    public static String GetVideoReport((Mat, Mat, String)[] SegmentRecords)
    {
        String VideoPath = Path.Combine(DirPath.Warehouse, "PipeVideo.MP4");

        Size SizeFrame = SegmentRecords.First().Item1.Size();

        Size VideoSize = new((SizeFrame.Width * 2), (SizeFrame.Height + 40));

        using VideoWriter Writer = new(VideoPath, FourCC.H264, 3, VideoSize);

        Mat[] SavedFrames = new Mat[SegmentRecords.Length];

        Parallel.For(0, SegmentRecords.Length, Idx =>
        {
            (Mat, Mat, String) Item = SegmentRecords[Idx];

            Mat Combined = new();

            Cv2.HConcat(Item.Item1, Item.Item2, Combined);

            Cv2.CopyMakeBorder(Combined, Combined, 0, 40, 0, 0, BorderTypes.Constant, new Scalar(0, 0, 0));

            String Text = $"Frame (#{Idx}): {Item.Item3}";

            Size TextSize = Cv2.GetTextSize(Text, HersheyFonts.HersheySimplex, 1.0, 2, out Int32 BaseLine);

            (Int32 X, Int32 Y) = ((Combined.Width - TextSize.Width) / 2, SizeFrame.Height + (40 + TextSize.Height) / 2);

            Cv2.PutText(Combined, Text, new Point(X, Y), HersheyFonts.HersheySimplex, 01, new Scalar(255, 255, 255), 2);

            SavedFrames[Idx] = Combined;
        });

        for (Int32 Idx = 0; Idx < SavedFrames.Length; Idx++)
        {
            Mat GetElement = (Mat)SavedFrames.GetValue(Idx)!;

            Writer.Write(GetElement);

            if (!GetElement.IsDisposed) GetElement.Dispose();
        }

        return VideoPath;
    }

    public static String GetExcelReport((Mat, Mat, Dictionary<String, Object>?)[] SegmentRecords)
    {
        Int32 GetWAsRatio(Double OldW, Double OldH, Double NewH)

                       => Convert.ToInt32(OldW * (NewH / OldH));

        Double PXToRowH(Double Pixel) => (Pixel * 0.750);

        Double PXToColW(Double Pixel) => (Pixel - 5) / 7;

        MemoryStream CreateResizedImage(Mat Image, Double NewW, Double NewH)
        {
            using Mat NImage = new();

            Size S = new(NewW, NewH);

            Cv2.Resize(Image, NImage, S, 0, 0, InterpolationFlags.Lanczos4);

            return NImage.ToMemoryStream();
        }

        String XLSXPath = Path.Combine(DirPath.Warehouse, "PipeExcel.XLSX");

        ExcelPackage.License.SetNonCommercialPersonal("Lư Hoàng Tấn");

        using ExcelPackage Package = new();

        ExcelWorksheet Main = Package.Workbook.Worksheets.Add("Main");

        Main.Cells["A1"].Value = "Plot Image";

        Main.Cells["B1"].Value = "Mask Image";

        Main.Cells["C1"].Value = "Frame";

        Main.Cells["D1"].Value = "Shape";

        Main.Cells["E1"].Value = "State";

        Main.Cells["F1"].Value = "AspectRatio";

        Main.Cells["G1"].Value = "Orientation";

        Main.Cells["H1"].Value = "Deformation";


        Size SampleMat = SegmentRecords.FirstOrDefault().Item1.Size();

        Int32 IH = 525;

        Int32 IW = GetWAsRatio(SampleMat.Width, SampleMat.Height, IH);

        Double EW = PXToColW(IW);

        Double EH = PXToRowH(IH);

        MemoryStream[] PlotImages = new MemoryStream[SegmentRecords.Length];

        MemoryStream[] MaskImages = new MemoryStream[SegmentRecords.Length];

        Parallel.For(0, SegmentRecords.Length, Idx =>
        {
            PlotImages[Idx] = CreateResizedImage(SegmentRecords[Idx].Item1, IW, IH);

            MaskImages[Idx] = CreateResizedImage(SegmentRecords[Idx].Item2, IW, IH);
        });

        Int32 StartRow = 2;

        for (Int32 Row = StartRow; Row < (SegmentRecords.Length + StartRow); Row++)
        {
            Int32 ArrayIdx = Row - StartRow;

            ExcelPicture PlotImage = Main.Drawings.AddPicture($"Plot{ArrayIdx + 1}", PlotImages[ArrayIdx]);

            ExcelPicture MaskImage = Main.Drawings.AddPicture($"Mask{ArrayIdx + 1}", MaskImages[ArrayIdx]);

            PlotImage.SetPosition(Row - 1, 0, 0, 0);

            MaskImage.SetPosition(Row - 1, 0, 1, 0);

            Main.Cells[Row, 3].Value = ArrayIdx + 1;

            Dictionary<String, Object>? Data = SegmentRecords.ElementAt(ArrayIdx).Item3;

            Main.Cells[Row, 4].Value = Data?["Shape"].ToString() ?? String.Empty;

            Main.Cells[Row, 5].Value = Data?["State"].ToString() ?? String.Empty;

            Main.Cells[Row, 6].Value = Data?["AspectRatio"].ToString() ?? String.Empty;

            Main.Cells[Row, 7].Value = Data?["Orientation"].ToString() ?? String.Empty;

            Main.Cells[Row, 8].Value = Data?["Deformation"].ToString() ?? String.Empty;

            Main.Row(Row).Height = EH;

            PlotImages[ArrayIdx].Dispose();

            MaskImages[ArrayIdx].Dispose();
        }

        Main.Column(1).Width = EW;

        Main.Column(2).Width = EW;

        Main.Cells["C:H"].AutoFitColumns();

        FileInfo ExcelInfo = new(XLSXPath);

        Package.SaveAs(ExcelInfo);

        return XLSXPath;
    }
}