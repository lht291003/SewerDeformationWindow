namespace SewerDeformationSoftware.Logics.Shareds.Cores;

public sealed class YOLOSeg
{
    readonly static Lazy<YOLOSeg> Instance = new(() => new YOLOSeg());

    public static YOLOSeg Models => Instance.Value;

    YoloPredictor? CENTERMODEL = null;

    YoloPredictor? SECONDMODEL = null;

    public String? UseHardware { get; set; } = null;

    public YoloPredictor? CenterModel { get => CENTERMODEL; set { CENTERMODEL?.Dispose(); CENTERMODEL = value; } }

    public YoloPredictor? SecondModel { get => SECONDMODEL; set { SECONDMODEL?.Dispose(); SECONDMODEL = value; } }
}