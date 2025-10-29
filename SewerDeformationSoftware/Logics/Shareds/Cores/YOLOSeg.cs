namespace SewerDeformationSoftware.Logics.Shareds.Cores;

public sealed class YOLOSeg
{
    public static Lazy<YOLOSeg> Instance { get; } = new(() => new YOLOSeg());

    public static YOLOSeg Models => Instance.Value;

    public String? DeviceType { get; set; } = null;

    public Boolean IsRunning { get; set; } = false;

    YoloPredictor? KEYYSMODEL = null;

    YoloPredictor? SECYSMODEL = null;

    public YoloPredictor? KeyYSModel { get => KEYYSMODEL; set { KEYYSMODEL?.Dispose(); KEYYSMODEL = value; } }

    public YoloPredictor? SecYSModel { get => SECYSMODEL; set { SECYSMODEL?.Dispose(); SECYSMODEL = value; } }
}