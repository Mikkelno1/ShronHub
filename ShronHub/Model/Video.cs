namespace ShronHub.Model;

public class Video
{
    public string Id { get; set; } = "";
    public string FileName  { get; set; } = "";
    public long Length  { get; set; }
    public DateTime UploadedAt  { get; set; }
    public VideoMetaData VideoMetaData  { get; set; }
}