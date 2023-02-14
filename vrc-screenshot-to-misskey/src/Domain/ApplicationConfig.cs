namespace vrc_screenshot_to_misskey.Domain;

public class ApplicationConfig
{
    public string Domain { get; }
    public string Token { get; }
    public string UploadPath { get; }
    public string SrcDir { get; }

    public ApplicationConfig(string domain, string token, string uploadPath, string srcDir)
    {
        Domain = domain;
        Token = token;
        UploadPath = uploadPath;
        SrcDir = srcDir;
    }
}
