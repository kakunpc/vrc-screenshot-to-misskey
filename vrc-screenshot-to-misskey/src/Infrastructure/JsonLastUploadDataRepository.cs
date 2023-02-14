using System.Text;
using Newtonsoft.Json;
using vrc_screenshot_to_misskey.Domain;

namespace vrc_screenshot_to_misskey.Infrastructure;

public sealed class JsonLastUploadDataRepository : ILastUploadDataRepository
{
    private readonly string _dataPath;

    public JsonLastUploadDataRepository()
    {
        _dataPath = "lastupdate.json";
        // 存在しない場合はサンプルを作成
        if (!File.Exists(_dataPath))
        {
            StoreAsync(new LastUploadData(DateTime.Now, "")).Wait();
        }
    }

    public async Task<LastUploadData> FindAsync()
    {
        // 存在しない場合は現在時刻のものを返す
        if (!File.Exists(_dataPath))
        {
            return new LastUploadData(DateTime.Now, "");
        }

        var json = await File.ReadAllTextAsync(_dataPath, Encoding.UTF8);
        var dto = JsonConvert.DeserializeObject<LastUploadDataDto>(json);
        if (long.TryParse(dto.LastUpdateString, out var t))
        {
            return new LastUploadData(new DateTime(t), dto.FileName);
        }
        // 失敗したら今の時刻を返す
        return new LastUploadData(DateTime.Now, "");
    }

    public async Task StoreAsync(LastUploadData lastUploadData)
    {
        var dto = new LastUploadDataDto
        {
            LastUpdateString = lastUploadData.LastUploadTime.Ticks.ToString(),
            FileName = lastUploadData.FileName
        };
        var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
        await File.WriteAllTextAsync(_dataPath, json, Encoding.UTF8);
    }
}

sealed class LastUploadDataDto
{
    [JsonProperty("lastUpdate")]
    public string LastUpdateString;

    [JsonProperty("fileName")]
    public string FileName;
}
