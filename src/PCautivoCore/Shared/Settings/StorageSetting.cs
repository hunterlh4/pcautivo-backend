namespace PCautivoCore.Shared.Settings;

public class StorageSetting
{
    public required string Path { get; set; }
    public required string Bucket { get; set; }
    public required string Url { get; set; }
    public required StorageImageSetting Image { get; set; }
}

public class StorageImageSetting
{
    public int SizeWith { get; set; }
    public int SizeHeight { get; set; }
    public required string AllowedExts { get; set; }
}