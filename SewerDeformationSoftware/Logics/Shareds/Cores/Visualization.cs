namespace SewerDeformationSoftware.Logics.Shareds.Cores;

public class Visualization
{
    public static void ShowDeChart(WpfPlot BChart, Dictionary<String, Object>?[] Data)
    {
        Double Step = 0.1;

        Int32 BinTotal = Convert.ToInt32(01 / 0.1);

        String[] Labels = new String[BinTotal + 1];

        for (Int32 Idx = 00; Idx < BinTotal; Idx++)
        {
            Labels[Idx] = $"{(Idx * Step * 100):F0}% - {((Idx + 1) * Step * 100):F0}%";
        }

        Labels[BinTotal] = "Undefined";

        Dictionary<String, Int32> FilterCounts = Data.Select(Item =>
        {
            if (Item != null && Item.TryGetValue("Deformation", out Object? GetObject))
            {
                Double Data = (Double)GetObject;

                Int32 GetIdx = (Int32)Math.Ceiling(Data / Step) - 1;

                if (GetIdx < 0) return Labels[0];

                if (GetIdx >= BinTotal) return Labels[BinTotal - 1];

                return Labels[GetIdx];
            }

            return "Undefined";

        }).GroupBy(Name => Name).ToDictionary(Gr => Gr.Key, Gr => Gr.Count());

        Dictionary<String, Int32> AllBarCounts = Labels.ToDictionary(Label => Label, Label => FilterCounts.TryGetValue(Label, out Int32 GetNumber) ? GetNumber : 00);

        Double[] Values = [.. AllBarCounts.Values.Select(Num => (Double)Num)];

        BChart.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual([.. Enumerable.Range(0, Labels.Length).Select(Id => (Double)Id)], Labels);

        List<Bar> Poles = AddColumns(Values);

        BChart.Plot.Clear();

        BChart.Plot.Add.Bars(Poles).Bars.ForEach(BC => BC.Label = BC.Value.ToString());

        BChart.Plot.XLabel("Range", 20);

        BChart.Plot.YLabel("Frame", 20);

        (IYAxis YAxis, IXAxis XAxis) = (BChart.Plot.Axes.Left, BChart.Plot.Axes.Bottom);

        YAxis.MajorTickStyle.Length = 0;

        YAxis.MinorTickStyle.Length = 0;

        XAxis.MajorTickStyle.Length = 0;

        XAxis.MinorTickStyle.Length = 0;

        BChart.Plot.Axes.AutoScale();

        BChart.Refresh();
    }

    public static void ShowSpChart(WpfPlot BChart, Dictionary<String, Object>?[] Data)
    {
        String[] Labels = ["Circle", "Ellipse", "Undefined", "No Detection"];

        Dictionary<String, Int32> FilterCounts = Data.GroupBy(X => X == null ? "No Detection" : X["Shape"].ToString()!).ToDictionary(Gr => Gr.Key, Gr => Gr.Count());

        Dictionary<String, Int32> AllBarCounts = Labels.ToDictionary(Label => Label, Label => FilterCounts.TryGetValue(Label, out Int32 GetNumber) ? GetNumber : 00);

        Double[] Values = [.. AllBarCounts.Values.Select(Num => (Double)Num)];

        BChart.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual([.. Enumerable.Range(0, Labels.Length).Select(Id => (Double)Id)], Labels);

        List<Bar> Poles = AddColumns(Values);

        BChart.Plot.Clear();

        BChart.Plot.Add.Bars(Poles).Bars.ForEach(BC => BC.Label = BC.Value.ToString());

        BChart.Plot.XLabel("Shape", 20);

        BChart.Plot.YLabel("Frame", 20);

        (IYAxis YAxis, IXAxis XAxis) = (BChart.Plot.Axes.Left, BChart.Plot.Axes.Bottom);

        YAxis.MajorTickStyle.Length = 0;

        YAxis.MinorTickStyle.Length = 0;

        XAxis.MajorTickStyle.Length = 0;

        XAxis.MinorTickStyle.Length = 0;

        BChart.Plot.Axes.AutoScale();

        BChart.Refresh();
    }

    public static List<Bar> AddColumns(Double[] Values)
    {
        List<Bar> Bars = [];

        for (Int32 Idx = 0; Idx < Values.Length; Idx++)
        {
            Bars.Add(new Bar() { Position = Idx, Value = Values.ElementAt(Idx), FillColor = ScottPlot.Colors.Category10[Idx % ScottPlot.Colors.Category10.Length] });
        }

        return Bars;
    }
}