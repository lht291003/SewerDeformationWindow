namespace SewerDeformationSoftware.Logics.Shareds;

public sealed class YOLOSeg
{
    readonly static Lazy<YOLOSeg> Instance = new(() => new YOLOSeg());

    public static YOLOSeg Models => Instance.Value;

    public String? UseHardware { get; set; } = null;

    public YoloPredictor? CenterModel { get; set; } = null;

    public YoloPredictor? SecondModel { get; set; } = null;

    YOLOSeg()
    {

    }
}