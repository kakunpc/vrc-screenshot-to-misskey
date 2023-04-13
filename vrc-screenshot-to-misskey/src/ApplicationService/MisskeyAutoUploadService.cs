using System.Diagnostics;
using MisskeyDotNet;
using vrc_screenshot_to_misskey.Domain;

namespace vrc_screenshot_to_misskey.ApplicationService;

public sealed class MisskeyAutoUploadService
{
    private readonly IApplicationConfigRepository _applicationConfigRepository;
    private readonly MisskeyFileUploadServices _fileUploadServices;
    private readonly ILastUploadDataRepository _lastUploadDataRepository;
    private readonly AvifImageConvertService _avifImageConvertService;
    private readonly ILogger _logger;
    private readonly IVrNotification _vrNotification;

    private bool _stopRequest = false;
    private bool _isExitOk = true;

    public MisskeyAutoUploadService(IApplicationConfigRepository applicationConfigRepository,
        MisskeyFileUploadServices fileUploadServices, ILastUploadDataRepository lastUploadDataRepository,
        AvifImageConvertService avifImageConvertService, ILogger logger, IVrNotification vrNotification)
    {
        _applicationConfigRepository = applicationConfigRepository;
        _fileUploadServices = fileUploadServices;
        _lastUploadDataRepository = lastUploadDataRepository;
        _avifImageConvertService = avifImageConvertService;
        _logger = logger;
        _vrNotification = vrNotification;
    }

    public async Task RunAsync()
    {
        _logger.Info("Start");
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
            int backoff = 1;
            while (!_stopRequest)
            {
                var lastUploadDate = await _lastUploadDataRepository.FindAsync();

                DirectoryInfo di = new DirectoryInfo(vrcPath);
                var files = di.GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(x => x.Extension.ToLower() is ".png" or ".jpg" or ".jpeg" or ".heic" or ".avif")
                    .Where(x => x.CreationTime.Ticks > lastUploadDate.LastUploadTime.Ticks)
                    .OrderBy(fi => fi.CreationTime)
                    .ToList();

                if (files.Count > 0) _logger.Info($"FindFiles:{files.Count}");

                var max = files.Count;
                int i = 0;
                foreach (var fileInfo in files)
                {
                    _logger.Info($"[{i}/{max}] AvifImageConvertService:{fileInfo.FullName}");
                    var outPath = await _avifImageConvertService.Run(fileInfo.FullName);

                    _logger.Info($"[{i}/{max}] UploadScreenShot:{outPath}");
                    var uploadResult =
                        await _fileUploadServices.UploadScreenShot(misskey, outPath,
                            Path.GetFileNameWithoutExtension(fileInfo.Name),
                            fileInfo.CreationTime);

                    if (uploadResult == MisskeyFileUploadServices.MisskeyFileUploadResult.Success)
                    {
                        if (applicationConfig.UseXSOverlay)
                        {
                            _vrNotification.SendNotification($"{fileInfo.FullName} Upload Success.");
                        }
                    }
                    else if (uploadResult == MisskeyFileUploadServices.MisskeyFileUploadResult.Skip)
                    {
                        if (applicationConfig.UseXSOverlay)
                        {
                            _vrNotification.SendNotification($"{fileInfo.FullName} Upload Skipped.");
                        }
                    }
                    else if (uploadResult == MisskeyFileUploadServices.MisskeyFileUploadResult.Failure)
                    {
                        if (applicationConfig.UseXSOverlay)
                        {
                            _vrNotification.SendNotification($"{fileInfo.FullName} Upload Failure.\nRetry after {backoff} seconds");
                        }

                        // 失敗したら10秒待機してからファイル探索を再開する
                        _logger.Info($"Retry after {backoff} seconds");
                        await Task.Delay(TimeSpan.FromSeconds(backoff));
                        // 失敗するごとに 1  2  4  8  16 秒とだんだん増えていく
                        backoff = backoff * 2;
                        break;
                    }
                    else
                    {
                        if (applicationConfig.UseXSOverlay)
                        {
                            _vrNotification.SendNotification($"不明なアップロード状態が発生しました為終了します。\n{uploadResult.ToString()}");
                        }

                        _logger.Error($"不明なアップロード状態:{uploadResult.ToString()}");
                        _ = Stop(Application.Exit);
                    }
                    
                    // 処理が進んだのでBackoffを戻す
                    backoff = 1;

                    // アップロードしてから1秒はまつ
                    _logger.Info($"[{i}/{max}] Wait");
                    if (applicationConfig.UploadDelay > 0)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(applicationConfig.UploadDelay));
                    }

                    // 終了を宣言されてたら処理を終わらせる
                    if (_stopRequest) break;
                    ++i;
                }

                if (files.Count > 0) _logger.Info($"Resume file monitoring.");

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }
        catch (Exception e)
        {
            if (applicationConfig.UseXSOverlay)
            {
                _vrNotification.SendNotification($"エラーが発生したため終了します");
            }

            _logger.Error(e);
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
        _logger.Info("Call Stop");

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

        // 各処理のDispose
        _avifImageConvertService.Dispose();

        Debug.WriteLine("END");
        _logger.Info("END");
        exitAction();
    }

    private async Task<Misskey> GetMisskeyClient(ApplicationConfig applicationConfig)
    {
        var ok = false;
        if (!string.IsNullOrEmpty(applicationConfig.Token))
        {
            // tokenの有効性を確認
            var mi = new Misskey(applicationConfig.Domain, applicationConfig.IsNotSecureServer,
                applicationConfig.Token);

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
            var token = await MiOauthService.RunAsync(applicationConfig.Domain, applicationConfig.IsNotSecureServer);
            // tokenを保存
            await _applicationConfigRepository.StoreAsync(new ApplicationConfig(applicationConfig, token: token));

            return new Misskey(applicationConfig.Domain, token);
        }

        throw new Exception("Misskeyクライアントの初期化に失敗");
    }
}