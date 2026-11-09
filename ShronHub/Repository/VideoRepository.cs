using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using ShronHub.Model;

namespace ShronHub.Repository;

public class VideoRepository : IVideoRepository
{

    private readonly IGridFSBucket _videoBucket;

    public VideoRepository(IGridFSBucket videoBucket)
    {
        _videoBucket = videoBucket;
    }

    public async Task<string> UploadAsync(string filename, VideoMetaData videoMetaData, Stream stream)
    {
        var metaDataDocument = videoMetaData.ToBsonDocument();

        var videoOption = new GridFSUploadOptions { Metadata = metaDataDocument };

        var gridFsId = await _videoBucket.UploadFromStreamAsync(filename, stream, videoOption);

        return gridFsId.ToString();
    }

    public async Task<Video?> GetByIdAsync(string targetId)
    {
        if (!ObjectId.TryParse(targetId, out var objectId))
        {
            return null;
        }

        var filter = Builders<GridFSFileInfo>.Filter.Eq(file => file.Id, objectId);

        var cursor = await _videoBucket.FindAsync(filter);

        var files = await cursor.ToListAsync();

        var fileInfo = files.FirstOrDefault();

        if (fileInfo == null)
        {
            return null;
        }

        var video = ToVideo(fileInfo);

        return video;

    }

    public async Task<List<Video>> GetAllAsync()
    {
        var filter = Builders<GridFSFileInfo>.Filter.Empty;

        var cursor = await _videoBucket.FindAsync(filter);

        var fileInfo = await cursor.ToListAsync();

        var videos = fileInfo.Select(ToVideo).ToList();

        return videos;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return false;
        }

        try
        {
            await _videoBucket.DeleteAsync(objectId);
            return true;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task<Stream?> OpenStreamASync(string id)
    {
        if (!ObjectId.TryParse(id, out var objectId))
        {
            return null;
        }

        try
        {
            var stream = await _videoBucket.OpenDownloadStreamAsync(objectId);
            return stream;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }
    }

    private static Video ToVideo(GridFSFileInfo fileInfo)
    {
        var metaData = BsonSerializer.Deserialize<VideoMetaData>(fileInfo.Metadata);

        var video = new Video
        {
            Id = fileInfo.Id.ToString(),
            FileName = fileInfo.Filename,
            Length = fileInfo.Length,
            VideoMetaData = metaData,
            UploadedAt = fileInfo.UploadDateTime
        };
        
        return video;
    }
}