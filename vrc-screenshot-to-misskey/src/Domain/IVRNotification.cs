namespace vrc_screenshot_to_misskey.Domain;

/// <summary>
/// VR空間へ通知
/// </summary>
public interface IVrNotification
{
    void SendNotification(string message);
}