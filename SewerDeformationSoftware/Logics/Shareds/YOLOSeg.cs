namespace SewerDeformationSoftware.Logics.Shareds;

public sealed class YOLOSeg
{
    readonly static Lazy<YOLOSeg> Instance = new(() => new YOLOSeg());

    public static YOLOSeg Models => Instance.Value;

    public Yolo CenterModel { get; set; } = null!;

    public Yolo SecondModel { get; set; } = null!;

    public Boolean IsWarnUp { get; set; } = false;

    public String Hardware { get; set; } = null!;

    YOLOSeg()
    {

    }
}