namespace vrc_screenshot_to_misskey.Domain;

public interface IApplicationConfigRepository
{
    Task<ApplicationConfig> FindAsync();
    Task StoreAsync(ApplicationConfig applicationConfig);
}
