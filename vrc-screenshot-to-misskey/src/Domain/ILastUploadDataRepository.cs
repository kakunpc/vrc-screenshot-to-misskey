namespace vrc_screenshot_to_misskey.Domain;

public interface ILastUploadDataRepository
{
    Task<LastUploadData> FindAsync();
    Task StoreAsync(LastUploadData lastUploadData);
}
