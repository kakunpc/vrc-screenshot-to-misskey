using System.Text;
using Newtonsoft.Json;
using vrc_screenshot_to_misskey.Domain;

namespace vrc_screenshot_to_misskey.Infrastructure;

public class JsonApplicationConfigRepository : IApplicationConfigRepository
{
    private readonly string _dataPath;

    public JsonApplicationConfigRepository()
    {
        _dataPath = "config.json";

        // 存在しない場合はサンプルを作成
        if (!File.Exists(_dataPath))
        {
            StoreAsync(new ApplicationConfig("misskey.io", "", "VRChat/{YYYY}-{MM}-{DD}", GetVRChatPictureDir())).Wait();
        }
    }

    private string GetVRChatPictureDir()
    {
        var myPicture = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        var vrcPath = Path.Combine(myPicture, "VRChat");
        return vrcPath;
    }

    public async Task<ApplicationConfig> FindAsync()
    {
        var json = await File.ReadAllTextAsync(_dataPath, Encoding.UTF8);
        var dto = JsonConvert.DeserializeObject<ApplicationConfigDto>(json);

        if (string.IsNullOrEmpty(dto.Domain)) dto.Domain = "misskey.io";
        if (string.IsNullOrEmpty(dto.UploadPath)) dto.UploadPath = "VRChat/{YYYY}-{MM}-{DD}";
        if (string.IsNullOrEmpty(dto.SrcDir)) dto.SrcDir = GetVRChatPictureDir();

        return new ApplicationConfig(dto.Domain, dto.Token, dto.UploadPath, dto.SrcDir);
    }

    public async Task StoreAsync(ApplicationConfig applicationConfig)
    {
        var dto = new ApplicationConfigDto
        {
            Domain = applicationConfig.Domain,
            Token = applicationConfig.Token,
            UploadPath = applicationConfig.UploadPath,
            SrcDir = applicationConfig.SrcDir,
        };
        var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
        await File.WriteAllTextAsync(_dataPath, json, Encoding.UTF8);
    }
}

class ApplicationConfigDto
{
    [JsonProperty("domain")]
    public string Domain;

    [JsonProperty("token")]
    public string Token;

    [JsonProperty("upload_path")]
    public string UploadPath;

    [JsonProperty("srcDir")]
    public string SrcDir;
}
