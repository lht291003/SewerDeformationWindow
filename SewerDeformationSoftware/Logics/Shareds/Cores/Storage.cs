namespace SewerDeformationSoftware.Logics.Shareds.Cores;

public class Storage
{
    public static (String, String) GetReport(List<(Mat, Mat, Mat, Dictionary<String, Object>?)> SegmentLogs)
    {
        String VideoPath = GetVideoReport([.. SegmentLogs.Select(Item =>
        {
            (Mat, Mat, String) Data;

            Data.Item1 = Item.Item1;

            Data.Item2 = Item.Item2;

            Data.Item3 = Item.Item4?["State"].ToString()! ?? "Undefined";

            return Data;
        })]);

        String ExcelPath = GetExcelReport([.. SegmentLogs.Select(Item =>
        {
            (Mat, Mat, Dictionary<String, Object>?) Data;

            Data.Item1 = Item.Item1;

            Data.Item2 = Item.Item2;

            Data.Item3 = Item.Item4;

            return Data;
        })]);

        return (VideoPath, ExcelPath);
    }

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
        String XLSXPath = Path.Combine(DirPath.Warehouse, "PipeExcel.XLSX");

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

        ExcelPackage.License.SetNonCommercialPersonal("Lư Hoàng Tấn");

        using ExcelPackage Package = new();

        ExcelWorksheet Main = Package.Workbook.Worksheets.Add("Main");

        Size SampleMat = SegmentRecords.FirstOrDefault().Item1.Size();

        Int32 IH = 150;

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

            for (Int32 Line = TextRange.Start.Row; Line <= TextRange.End.Row; Line++)
            {
                ExcelRange ExcelCell = Main.Cells[Line, Col];

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
                Main.Column(Col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
            }
        }

        ExcelTable Table = Main.Tables.Add(GridRange, $"SewerSegmentationStorageDataTable");

        Table.TableStyle = TableStyles.Medium23;

        Table.ShowFilter = false;

        Package.SaveAs(XLSXPath);

        return XLSXPath;
    }
}