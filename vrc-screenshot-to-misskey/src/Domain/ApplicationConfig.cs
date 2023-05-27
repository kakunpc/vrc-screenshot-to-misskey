namespace vrc_screenshot_to_misskey.Domain;

public class ApplicationConfig
{
    public string Domain { get; }
    public bool IsNotSecureServer { get; }
    public string Token { get; }
    public string UploadPath { get; }
    public string SrcDir { get; }
    public bool UseAvifConvert { get; }
    public int TimeToPreviousDay { get; }
    public bool AllowDuplicates { get; }
    public bool UseXSOverlay { get; }
    public int UploadDelay { get; }

    public ApplicationConfig(string domain,
        bool isNotSecureServer,
        string token,
        string uploadPath,
        string srcDir,
        bool useAvifConvert,
        int timeToPreviousDay, bool allowDuplicates,
        bool useXSOverlay, int uploadDelay)
    {
        Domain = domain;
        IsNotSecureServer = isNotSecureServer;
        Token = token;
        UploadPath = uploadPath;
        SrcDir = srcDir;
        UseAvifConvert = useAvifConvert;
        AllowDuplicates = allowDuplicates;
        UseXSOverlay = useXSOverlay;
        UploadDelay = uploadDelay;
        TimeToPreviousDay = (int) MathF.Max(0, MathF.Min(timeToPreviousDay, 24));
    }

    public ApplicationConfig(
        ApplicationConfig src, string? domain = null,
        bool? isNotSecureServer = null,
        string? token = null,
        string? uploadPath = null,
        string? srcDir = null,
        bool? useAvifConvert = null,
        int? timeToPreviousDay = null,
        bool? allowDuplicates = null,
        bool? useXSOverlay = null,
        int? uploadDelay = null) : this(
        domain: domain ?? src.Domain,
        isNotSecureServer: isNotSecureServer ?? src.IsNotSecureServer,
        token: token ?? src.Token,
        uploadPath: uploadPath ?? src.UploadPath,
        srcDir: srcDir ?? src.SrcDir,
        useAvifConvert: useAvifConvert ?? src.UseAvifConvert,
        timeToPreviousDay: timeToPreviousDay ?? src.TimeToPreviousDay,
        allowDuplicates: allowDuplicates ?? src.AllowDuplicates,
        useXSOverlay: useXSOverlay ?? src.UseXSOverlay,
        uploadDelay: uploadDelay ?? src.UploadDelay)
    {
    }
}