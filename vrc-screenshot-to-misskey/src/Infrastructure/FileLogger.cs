using vrc_screenshot_to_misskey.Domain;

namespace vrc_screenshot_to_misskey.Infrastructure;

public sealed class FileLogger : ILogger
{
    private string fileName => "logfile.txt";

    private string GetDate => DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

    public FileLogger()
    {
        // 起動したら最初に罫線をつける
        using var streamWriter = new StreamWriter(fileName, append: true);
        streamWriter.WriteLine($"------------------------------------------------");
    }

    public void Info(string message)
    {
        using var streamWriter = new StreamWriter(fileName, append: true);
        streamWriter.WriteLine($"[Info] {GetDate}●{message}");
    }

    public void Warning(string message)
    {
        using var streamWriter = new StreamWriter(fileName, append: true);
        streamWriter.WriteLine($"[Warning] {GetDate}●{message}");
    }

    public void Error(string message)
    {
        using var streamWriter = new StreamWriter(fileName, append: true);
        streamWriter.WriteLine($"[Error] {GetDate}●{message}");
    }

    public void Error(Exception ex)
    {
        using var streamWriter = new StreamWriter(fileName, append: true);
        streamWriter.WriteLine($"[Exception] {GetDate}●{ex.Message}\n{ex.StackTrace}");
    }
}