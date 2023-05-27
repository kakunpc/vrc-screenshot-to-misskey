namespace vrc_screenshot_to_misskey.Domain;

public interface ILogger
{
    void Info(string message);
    void Warning(string message);
    void Error(string message);
    void Error(Exception ex);
}