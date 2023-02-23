namespace vrc_screenshot_to_misskey.Domain;

public class ApplicationConfig
{
    public string Domain { get; }
    public string Token { get; }
    public string UploadPath { get; }
    public string SrcDir { get; }
    public bool UseAvifConvert { get; }
    public int TimeToPreviousDay { get; }
    public bool AllowDuplicates { get; }

    public ApplicationConfig(string domain,
        string token,
        string uploadPath,
        string srcDir,
        bool useAvifConvert,
        int timeToPreviousDay, bool allowDuplicates)
    {
        Domain = domain;
        Token = token;
        UploadPath = uploadPath;
        SrcDir = srcDir;
        UseAvifConvert = useAvifConvert;
        AllowDuplicates = allowDuplicates;
        TimeToPreviousDay = (int) MathF.Max(0, MathF.Min(timeToPreviousDay, 24));
    }

    public ApplicationConfig(
        ApplicationConfig src, string? domain = null,
        string? token = null,
        string? uploadPath = null,
        string? srcDir = null,
        bool? useAvifConvert = null,
        int? timeToPreviousDay = null,
        bool? allowDuplicates = null) : this(
        domain: domain ?? src.Domain,
        token: token ?? src.Token,
        uploadPath: uploadPath ?? src.UploadPath,
        srcDir: srcDir ?? src.SrcDir,
        useAvifConvert: useAvifConvert ?? src.UseAvifConvert,
        timeToPreviousDay: timeToPreviousDay ?? src.TimeToPreviousDay,
        allowDuplicates: allowDuplicates ?? src.AllowDuplicates)
    {
    }
}