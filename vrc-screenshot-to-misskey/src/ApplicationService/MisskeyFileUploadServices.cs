using System.Diagnostics;
using System.Net.Http.Headers;
using MisskeyDotNet;
using vrc_screenshot_to_misskey.Domain;

namespace vrc_screenshot_to_misskey.ApplicationService;

public sealed class MisskeyFileUploadServices
{
    private readonly ILastUploadDataRepository _lastUploadDataRepository;
    private readonly IApplicationConfigRepository _applicationConfigRepository;

    public MisskeyFileUploadServices(ILastUploadDataRepository lastUploadDataRepository, IApplicationConfigRepository applicationConfigRepository)
    {
        _lastUploadDataRepository = lastUploadDataRepository;
        _applicationConfigRepository = applicationConfigRepository;
    }

    public async Task UploadScreenShot(Misskey misskey, string filePath)
    {
        try
        {
            var applicationConfig = await _applicationConfigRepository.FindAsync();

            // ファイルの状態を取得
            var fi = new FileInfo(filePath);

            // アップロード先ディレクトリの確認
            var now = fi.CreationTime;

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
                    var res = await misskey.ApiAsync<DriveFolder>("drive/folders/create", string.IsNullOrEmpty(pathRoot) ? new { name = path } : new { name = path, parentId = pathRoot });
                    // IDを登録
                    pathRoot = res.Id;
                }
                else
                {
                    pathRoot = find.Id;
                }
            }

            // folderId

            var url = "https://" + applicationConfig.Domain + "/api/drive/files/create";

            var filename = Path.GetFileName(filePath);
            var ext = Path.GetExtension(filePath);

            var body = new MultipartFormDataContent();
            body.Add(new StringContent(misskey.Token!), "i");
            body.Add(new StringContent(filename), "name");
            if (!string.IsNullOrEmpty(pathRoot)) body.Add(new StringContent(pathRoot), "folderId");

            await using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamContent streamContent = new StreamContent(fs);
            streamContent.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("form-data")
                {
                    Name = "file",
                    FileName = filename
                };
            switch (ext)
            {
                case ".png":
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
                    break;
                default:
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                    break;
            }

            body.Add(streamContent, "file");

            using var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(url, body);
            var m = response.EnsureSuccessStatusCode();
            Debug.WriteLine(m.ToString());
            // 更新完了（ログに残す）
            await _lastUploadDataRepository.StoreAsync(new LastUploadData(now, filePath));
        }
        catch (Exception e)
        {
            // TODO: ログ
        }
    }
}
