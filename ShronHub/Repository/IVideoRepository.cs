using ShronHub.Model;

namespace ShronHub.Repository;

public interface IVideoRepository
{
    Task<string> UploadAsync(string filename, VideoMetaData videoMetaData, Stream stream);
    Task<Video?> GetByIdAsync(string targetId);
    Task<List<Video>> GetAllAsync();
    Task<bool> DeleteAsync(string id);
    Task<Stream?> OpenStreamASync(string id);
    
}