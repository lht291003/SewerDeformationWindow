namespace SewerDeformationSoftware.Logics.Shareds.Utils;

public class SLImage
{
    public SLImage()
    {
        ContiguousImageConfigs.PreferContiguousImageBuffers = true;
    }

    public static Configuration ContiguousImageConfigs { get; set; } = Configuration.Default.Clone();
}