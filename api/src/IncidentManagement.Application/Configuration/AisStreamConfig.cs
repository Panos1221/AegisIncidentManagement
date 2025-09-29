public class AisStreamConfig
{
    public bool Enabled { get; set; } = true;
    public string ApiKey { get; set; } = string.Empty;
    public double[][][] BoundingBoxes { get; set; } = [];
}
