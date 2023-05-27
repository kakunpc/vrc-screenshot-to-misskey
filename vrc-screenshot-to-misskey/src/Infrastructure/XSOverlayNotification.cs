using vrc_screenshot_to_misskey.Domain;
using XSNotifications;

namespace vrc_screenshot_to_misskey.Infrastructure;

public sealed class XSOverlayNotification : IVrNotification
{
    private readonly XSNotifier _xsNotifier = new XSNotifier();

    public void SendNotification(string message)
    {
        try
        {
            _xsNotifier.SendNotification(new XSNotification()
            {
                Title = "Screenshot To Misskey",
                Content = message,
                Timeout = 1f,
            });
        }
        catch (Exception)
        {
            // XSOverlayを持ってないなどでエラーが出ても無視
        }
    }
}