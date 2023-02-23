using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using MisskeyDotNet;
using vrc_screenshot_to_misskey.Domain;

namespace vrc_screenshot_to_misskey.ApplicationService;

public sealed class MisskeyFileUploadServices
{
    public enum MisskeyFileUploadResult
    {
        Success,
        Skip,
        Failure
    }
    
    private readonly ILastUploadDataRepository _lastUploadDataRepository;
    private readonly IApplicationConfigRepository _applicationConfigRepository;
    readonly Regex _regex = new Regex(@".*(\d\d\d\d)-(\d\d)-(\d\d)_(\d\d)-(\d\d)-(\d\d)");
    private readonly ILogger _logger;

    public MisskeyFileUploadServices(ILastUploadDataRepository lastUploadDataRepository,
        IApplicationConfigRepository applicationConfigRepository, ILogger logger)
    {
        _lastUploadDataRepository = lastUploadDataRepository;
        _applicationConfigRepository = applicationConfigRepository;
        _logger = logger;
    }

    public async Task<MisskeyFileUploadResult> UploadScreenShot(Misskey misskey, string filePath, string fileName,
        DateTime originalDateTime)
    {
        try
        {
            var applicationConfig = await _applicationConfigRepository.FindAsync();

            // 生成された時刻を採用する
            var now = originalDateTime;

            // ファイル名に日付があったらそれを採用する　例）VRChat_2023-02-15_02-28-41.435_7680x4320
            var matches = _regex.Match(fileName);
            if (matches.Success)
            {
                now = DateTime.Parse(
                    $"{matches.Groups[1]}/{matches.Groups[2]}/{matches.Groups[3]} {matches.Groups[4]}:{matches.Groups[5]}:{matches.Groups[6]}");
            }

            // 
            now = now.AddHours(-applicationConfig.TimeToPreviousDay);

            // アップロード先ディレクトリの確認
            var uploadPath = applicationConfig.UploadPath
                // \を/に変える
                .Replace("\\", "/")
                // YYYY を今日の年に置き換え
                .Replace("{YYYY}", now.Year.ToString("0000"))
                // 今月
                .Replace("{MM}", now.Month.ToString("00"))
                // 今日
                .Replace("{DD}", now.Day.ToString("00"));

            var paths = uploadPath.Split("/");

            string pathRoot = "";
            foreach (var path in paths)
            {
                var folders = await misskey.ApiAsync<List<DriveFolder>>("drive/folders",
                    string.IsNullOrEmpty(pathRoot)
                        ? null
                        : new
                        {
                            folderId = pathRoot
                        });

                var find = folders.Find(x => x.Name == path);

                // フォルダが有るか
                if (find == null)
                {
                    // 存在しないなら作成
                    var res = await misskey.ApiAsync<DriveFolder>("drive/folders/create",
                        string.IsNullOrEmpty(pathRoot)
                            ? new {name = path}
                            : new {name = path, parentId = pathRoot});
                    // IDを登録
                    pathRoot = res.Id;
                }
                else
                {
                    pathRoot = find.Id;
                }
            }

            var ext = Path.GetExtension(filePath);

            // 同名ファイルが存在するかチェック
            if (!applicationConfig.AllowDuplicates)
            {
                var files = await misskey.ApiAsync<List<DriveFIle>>("drive/files/find", new
                {
                    name = fileName + ext,
                    folderId = pathRoot
                });

                if (files.Count > 0)
                {
                    _logger.Info($"{fileName + ext} Exists. Skip uploading.");
                    await _lastUploadDataRepository.StoreAsync(new LastUploadData(originalDateTime, filePath));
                    return MisskeyFileUploadResult.Skip;
                }
            }

            var url = "https://" + applicationConfig.Domain + "/api/drive/files/create";

            var body = new MultipartFormDataContent();
            body.Add(new StringContent(misskey.Token!), "i");
            body.Add(new StringContent(fileName + ext), "name");
            if (!string.IsNullOrEmpty(pathRoot)) body.Add(new StringContent(pathRoot), "folderId");

            await using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamContent streamContent = new StreamContent(fs);
            streamContent.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = fileName + ext
                };
            switch (ext.ToLower())
            {
                case ".png":
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    break;

                case ".heic":
                    // image/heic の方がいい？  https://mimetype.io/image/heic
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/heif-sequence");
                    break;

                case ".avif":
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/avif");
                    break;

                case ".jpg":
                case ".jpeg":
                default:
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    break;
            }

            body.Add(streamContent, "file");

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(url, body);
            var m = response.EnsureSuccessStatusCode();
            var content = await m.Content.ReadAsStringAsync();
            _logger.Info("UPLOAD OK\n\t" + content);
            await _lastUploadDataRepository.StoreAsync(new LastUploadData(originalDateTime, filePath));
            return MisskeyFileUploadResult.Success;
        }
        catch (Exception e)
        {
            _logger.Error("Upload Error.");
            _logger.Error(e);
            return MisskeyFileUploadResult.Failure;
        }
    }
}