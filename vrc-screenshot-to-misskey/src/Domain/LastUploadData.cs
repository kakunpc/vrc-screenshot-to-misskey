namespace vrc_screenshot_to_misskey.Domain;

public class LastUploadData
{
    public LastUploadData(DateTime lastUploadTime, string fileName)
    {
        LastUploadTime = lastUploadTime;
        FileName = fileName;
    }

    public DateTime LastUploadTime { get; }
    public string FileName { get; }
}
