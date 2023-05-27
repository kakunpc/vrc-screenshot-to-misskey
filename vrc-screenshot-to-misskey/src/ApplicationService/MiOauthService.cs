using System.Net;
using System.Text;
using MisskeyDotNet;

namespace vrc_screenshot_to_misskey.ApplicationService;

/// <summary>
/// MisskeyのMiAuthを行う
/// </summary>
public static class MiOauthService
{
    private static readonly Random Random = new Random();

    public static async Task<string> RunAsync(string domain, bool isNotSecureServer)
    {
        var port = Random.Next(8000, 40000);

        var code = "";
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://127.0.0.1:{port}/");
        listener.Start();

        var miAuth = new MiAuth(domain,
            isNotSecureServer,
            "vrc-screenshot-to-misskey",
            "https://misskey-media.kakunpc.com/media/aa9bf6d6-24c1-4998-8f32-a8c0513e9e44.jpg",
            $"http://127.0.0.1:{port}/",
            Permission.All); // 面倒なのですべてのパーミッションを要求してしまう
        if (!miAuth.TryOpenBrowser())
        {
            // エラー発生
            Application.Exit();
            return "";
        }

        while (string.IsNullOrEmpty(code))
        {
            var context = await listener.GetContextAsync();
            var request = context.Request;

            using var response = context.Response;

            if (request.RawUrl != null && request.RawUrl.StartsWith("/?session="))
            {
                try
                {
                    // セッションを確認する
                    var io = await miAuth.CheckAsync();
                    var i = await io.ApiAsync<Dictionary<string, object>>("i");
                    var userName = i["username"];
                    code = io.Token!;
                    byte[] text = Encoding.UTF8.GetBytes(
                        $"<html><head><meta charset='utf-8'/></head><body><h1>@{userName}さん連携が完了しました。<br>このページを閉じてください。</h1></body></html>");
                    response.OutputStream.Write(text, 0, text.Length);
                }
                catch (Exception)
                {
                    response.StatusCode = 500;
                }
            }
            else
            {
                response.StatusCode = 404;
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(1));

        return code;
    }
}