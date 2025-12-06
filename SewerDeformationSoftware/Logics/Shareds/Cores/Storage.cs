namespace SewerDeformationSoftware.Logics.Shareds.Cores;

public class Storage
{
    public static String VideoPath { get; } = Path.Combine(WorkDir.Warehouse, "PipeVideo.MP4");

    public static String ExcelPath { get; } = Path.Combine(WorkDir.Warehouse, "PipeXLSX.XLSX");

    public static async Task<(String, String)> GetReport(List<(Mat, Mat, Mat, Dictionary<String, Object>?)> SegmentLogs)
    {
        while (Interop.IsFileLocked(VideoPath))
        {
            Message.ShowErrors($"Video {Path.GetFileName(VideoPath)} hiện đang được sử dụng bởi một chương trình khác!");

            await Task.Delay(500);
        }

        while (Interop.IsFileLocked(ExcelPath))
        {
            Message.ShowErrors($"Excel {Path.GetFileName(ExcelPath)} hiện đang được sử dụng bởi một chương trình khác!");

            await Task.Delay(500);
        }

        Task SuppliedTVideo = Task.Run(() => LoadVideoReport([.. SegmentLogs.Select(Item =>
        {
            (Mat, Mat, String) Data;

            Data.Item1 = Item.Item1;

            Data.Item2 = Item.Item2;

            Data.Item3 = Item.Item4?.GetValueOrDefault("State")!.ToString() ?? "Undefined";

           return Data;
        })]));

        Task SuppliedTExcel = Task.Run(() => LoadExcelReport([.. SegmentLogs.Select(R => (R.Item1, R.Item2, R.Item4))]));

        await Task.WhenAll([SuppliedTVideo, SuppliedTExcel]);

        return (VideoPath, ExcelPath);
    }

    public static String LoadVideoReport((Mat, Mat, String)[] SegmentRecords)
    {
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

            String Text = $"Frame {Idx + 1}: {Item.Item3}";

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

    public static String LoadExcelReport((Mat, Mat, Dictionary<String, Object>?)[] SegmentRecords)
    {
        ExcelPackage.License.SetNonCommercialPersonal("Lư Hoàng Tấn(Tan Dawson)");

        using ExcelPackage Package = new();

        ExcelWorksheet Main = Package.Workbook.Worksheets.Add("SegmentationLogs");

        ExcelWorksheet Plts = Package.Workbook.Worksheets.Add("StatisticsCharts");

        Task ExcelChartAsImageFromScottPlot(Plot Chart, ExcelWorksheet Page, Int32 Row, Int32 Col)
        {
            (Int32 W, Int32 H) = (850, 500);

            (Page.Row(Row).Height, Page.Column(Col).Width) = (PXToRowH(H), PXToColW(W));

            Byte[] GetImageByte = Chart.GetImageBytes(W, H, ScottPlot.ImageFormat.Jpeg);

            using MemoryStream GetImage = new(GetImageByte);

            String ExcelChartName = $"ScottPlotAsExcelCharrt{Guid.NewGuid()}".ToUpper();

            ExcelPicture PlotImage = Page.Drawings.AddPicture(ExcelChartName, GetImage);

            (PlotImage.Border.Width, PlotImage.Border.Fill.Color) = (1, System.Drawing.Color.Gray);

            PlotImage.SetPosition(Row - 1, 0, Col - 1, 0);

            return Task.CompletedTask;
        }

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

        Size SampleMat = SegmentRecords.FirstOrDefault().Item1.Size();

        Int32 IH = 175;

        Int32 IW = GetWAsRatio(SampleMat.Width, SampleMat.Height, IH);

        Double EW = PXToColW(IW);

        Double EH = PXToRowH(IH);

        Main.Cells["A1"].Value = "Plot Image";

        Main.Cells["B1"].Value = "Mask Image";

        Main.Cells["C1"].Value = "Frame";

        Main.Cells["D1"].Value = "Shape";

        Main.Cells["E1"].Value = "State";

        Main.Cells["F1"].Value = "AspectRatio";

        Main.Cells["G1"].Value = "Orientation";

        Main.Cells["H1"].Value = "Deformation";

        MemoryStream[] PlotImages = new MemoryStream[SegmentRecords.Length];

        MemoryStream[] MaskImages = new MemoryStream[SegmentRecords.Length];

        Parallel.For(0, SegmentRecords.Length, Idx =>
        {
            PlotImages[Idx] = CreateResizedImage(SegmentRecords[Idx].Item1, IW, IH);

            MaskImages[Idx] = CreateResizedImage(SegmentRecords[Idx].Item2, IW, IH);
        });

        (Int32 StartRow, Int32 BottomRow) = (2, -1);

        for (Int32 Row = StartRow; Row < (SegmentRecords.Length + StartRow); Row++)
        {
            BottomRow = Row;

            Int32 ArrayIdx = (BottomRow - StartRow);

            ExcelPicture PlotImage = Main.Drawings.AddPicture($"Plot{ArrayIdx + 1}", PlotImages[ArrayIdx]);

            ExcelPicture MaskImage = Main.Drawings.AddPicture($"Mask{ArrayIdx + 1}", MaskImages[ArrayIdx]);

            PlotImage.SetPosition(Row - 1, 0, 0, 0);

            MaskImage.SetPosition(Row - 1, 0, 1, 0);

            Main.Cells[Row, 3].Value = ArrayIdx + 1;

            Dictionary<String, Object>? Data = SegmentRecords[ArrayIdx].Item3;

            Main.Cells[Row, 4].Value = Data?["Shape"] ?? String.Empty;

            Main.Cells[Row, 5].Value = Data?["State"] ?? String.Empty;

            Main.Cells[Row, 6].Value = Data?["AspectRatio"] ?? String.Empty;

            Main.Cells[Row, 7].Value = Data?["Orientation"] ?? String.Empty;

            Main.Cells[Row, 8].Value = Data?["Deformation"] ?? String.Empty;

            Main.Row(Row).Height = EH;

            PlotImages[ArrayIdx].Dispose();

            MaskImages[ArrayIdx].Dispose();
        }

        ExcelRange GridRange = Main.Cells[StartRow - 1, 1, BottomRow, 8];

        ExcelRange EImgRange = Main.Cells[StartRow - 0, 1, BottomRow, 2];

        ExcelRange TextRange = Main.Cells[StartRow - 0, 3, BottomRow, 8];

        for (Int32 Col = EImgRange.Start.Column; Col <= EImgRange.End.Column; Col++)
        {
            Main.Column(Col).Width = EW;
        }

        for (Int32 Col = TextRange.Start.Column; Col <= TextRange.End.Column; Col++)
        {
            Main.Column(Col).AutoFit();

            Boolean IsNumColumn = true;

            Main.Column(Col).Style.VerticalAlignment = ExcelVerticalAlignment.Center;

            (Int32 TopRow, Int32 LastRow) = (TextRange.Start.Row, TextRange.End.Row);

            for (Int32 Row = TopRow; Row <= LastRow; Row++)
            {
                ExcelRange ExcelCell = Main.Cells[Row, Col];

                if (ExcelCell.Value != null && !String.IsNullOrEmpty(ExcelCell.Text))
                {
                    if (!Double.TryParse(ExcelCell.Value.ToString(), out Double Val))
                    {
                        IsNumColumn = false;

                        break;
                    }
                }
            }

            if (IsNumColumn)
            {
                ExcelRange SingleColumnRange = Main.Cells[TopRow, Col, LastRow, Col];

                SingleColumnRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
        }

        ExcelTable Table = Main.Tables.Add(GridRange, $"PipeSegmentationStorageResultTable");

        Table.TableStyle = TableStyles.Medium23;

        ExcelChartAsImageFromScottPlot(Video.SpPlotChart.Plot, Plts, 1, 1);

        ExcelChartAsImageFromScottPlot(Video.DePlotChart.Plot, Plts, 3, 1);

        Package.SaveAs(ExcelPath);

        return ExcelPath;
    }
}