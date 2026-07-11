namespace SewerDeformationSoftware.Logics.Cores;

public sealed class YOLOSeg
{
    public static Lazy<YOLOSeg> Instance { get; } = new(() => new YOLOSeg());

    public static YOLOSeg Models => Instance.Value;

    public String? DeviceType { get; set; } = null;

    public Boolean IsRunning { get; set; } = false;

    public YoloPredictor? KeyYSModel { get; set { field?.Dispose(); field = value; } } = null;

    public YoloPredictor? SecYSModel { get; set { field?.Dispose(); field = value; } } = null;
}