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
            StoreAsync(new ApplicationConfig(
                "misskey.io",
                "",
                "VRChat/{YYYY}-{MM}-{DD}",
                GetVRChatPictureDir(),
                false,
                5
            )).Wait();
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
        if (string.IsNullOrEmpty(dto.SrcDir))
        {
            dto.SrcDir = string.IsNullOrEmpty(dto.OldSrcDir) ? GetVRChatPictureDir() : dto.OldSrcDir;
        }

        return new ApplicationConfig(dto.Domain,
            dto.Token,
            dto.UploadPath,
            dto.SrcDir,
            dto.UseAvifConvert ?? false,
            dto.TimeToPreviousDay ?? 5);
    }

    public async Task StoreAsync(ApplicationConfig applicationConfig)
    {
        var dto = new ApplicationConfigDto
        {
            Domain = applicationConfig.Domain,
            Token = applicationConfig.Token,
            UploadPath = applicationConfig.UploadPath,
            SrcDir = applicationConfig.SrcDir,
            UseAvifConvert = applicationConfig.UseAvifConvert,
            TimeToPreviousDay = applicationConfig.TimeToPreviousDay,
        };
        var json = JsonConvert.SerializeObject(dto, Formatting.Indented,
            new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
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

    [JsonProperty("src_dir")]
    public string SrcDir;

    [JsonProperty("use_avif_convert")]
    public bool? UseAvifConvert;

    [JsonProperty("time_to_previous_day")]
    public int? TimeToPreviousDay;

    // 以下使用しない nullにすることでエクスポートから消す
    [JsonProperty("srcDir")]
    public string? OldSrcDir;
}