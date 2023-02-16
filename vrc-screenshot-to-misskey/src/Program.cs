using Microsoft.Extensions.DependencyInjection;
using vrc_screenshot_to_misskey.ApplicationService;
using vrc_screenshot_to_misskey.Domain;
using vrc_screenshot_to_misskey.Infrastructure;

namespace vrc_screenshot_to_misskey;

internal static class Program
{
    /// <summary>
    ///     The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        // ApplicationConfiguration.Initialize();
        // Application.Run(new Form1());
        _ = serviceProvider.GetRequiredService<Form1>();
        Application.Run();
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        services.AddSingleton<Form1>();
        services.AddScoped<IApplicationConfigRepository, JsonApplicationConfigRepository>();
        services.AddScoped<ILastUploadDataRepository, JsonLastUploadDataRepository>();
        services.AddScoped<ILogger, FileLogger>();

        services.AddSingleton<MisskeyFileUploadServices>();
        services.AddSingleton<MisskeyAutoUploadService>();
        services.AddSingleton<AvifImageConvertService>();
    }
}
