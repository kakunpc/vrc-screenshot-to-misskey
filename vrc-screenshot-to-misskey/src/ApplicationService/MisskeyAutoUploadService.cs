using System.Diagnostics;
using MisskeyDotNet;
using vrc_screenshot_to_misskey.Domain;

namespace vrc_screenshot_to_misskey.ApplicationService;

public sealed class MisskeyAutoUploadService
{
    private readonly IApplicationConfigRepository _applicationConfigRepository;
    private readonly MisskeyFileUploadServices _fileUploadServices;
    private readonly ILastUploadDataRepository _lastUploadDataRepository;

    private bool _stopRequest = false;
    private bool _isExitOk = true;

    public MisskeyAutoUploadService(IApplicationConfigRepository applicationConfigRepository, MisskeyFileUploadServices fileUploadServices, ILastUploadDataRepository lastUploadDataRepository)
    {
        _applicationConfigRepository = applicationConfigRepository;
        _fileUploadServices = fileUploadServices;
        _lastUploadDataRepository = lastUploadDataRepository;
    }

    public async Task RunAsync()
    {
        // ファイルを読み込む
        var applicationConfig = await _applicationConfigRepository.FindAsync();
        // マイグレーションのために一度書き込み
        await _applicationConfigRepository.StoreAsync(applicationConfig);

        try
        {
            var misskey = await GetMisskeyClient(applicationConfig);
            var vrcPath = applicationConfig.SrcDir;

            // フォルダ監視を開始
            _isExitOk = false;
            while (!_stopRequest)
            {
                var lastUploadDate = await _lastUploadDataRepository.FindAsync();

                DirectoryInfo di = new DirectoryInfo(vrcPath);
                var files = di.GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(x => x.Extension == ".png" || x.Extension == ".jpg" || x.Extension == ".jpeg")
                    .Where(x => x.CreationTime.Ticks > lastUploadDate.LastUploadTime.Ticks)
                    .OrderBy(fi => fi.CreationTime)
                    .ToList();

                foreach (var fileInfo in files)
                {
                    await _fileUploadServices.UploadScreenShot(misskey, fileInfo.FullName);
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                    // 終了を宣言されてたら処理を終わらせる
                    if (_stopRequest) break;
                }
                Debug.WriteLine("UploadTest");
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
            // TODO: ログ
            // 強制終了
            Application.Exit();
        }
        finally
        {
            _isExitOk = true;
        }

        await Task.CompletedTask;
    }

    public async Task Stop(Action exitAction)
    {
        if (_stopRequest) return;

        // 処理停止のリクエスト
        _stopRequest = true;

        // 終了できるまで待機
        await Task.Run(async () =>
        {
            while (!_isExitOk)
            {
                await Task.CompletedTask;
            }
        });

        Debug.WriteLine("END");
        exitAction();
    }

    private async Task<Misskey> GetMisskeyClient(ApplicationConfig applicationConfig)
    {
        var ok = false;
        if (!string.IsNullOrEmpty(applicationConfig.Token))
        {
            // tokenの有効性を確認
            var mi = new Misskey(applicationConfig.Domain, applicationConfig.Token);

            try
            {
                // 自分の情報が返ってこれればOK
                _ = await mi.IAsync(true);
                ok = true;
            }
            catch (MisskeyApiException e)
            {
                // APIサーバーから失敗ということはTokenが無効になってることにする
                ok = false;
            }
            catch (Exception)
            {
                // TODO: サーバー側で問題が発生してるからリトライとか
                ok = false;
            }

            if (ok)
            {
                return mi;
            }
        }

        if (ok == false)
        {
            var token = await MiOauthService.RunAsync(applicationConfig.Domain);
            // tokenを保存
            await _applicationConfigRepository.StoreAsync(new ApplicationConfig(applicationConfig.Domain, token, applicationConfig.UploadPath, applicationConfig.UploadPath));

            return new Misskey(applicationConfig.Domain, token);
        }

        throw new Exception("Misskeyクライアントの初期化に失敗");
    }
}
