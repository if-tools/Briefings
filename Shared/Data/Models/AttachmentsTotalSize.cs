using MongoDB.Bson;

namespace IFToolsBriefings.Shared.Data.Models;

public class AttachmentsTotalSize
{
    public ObjectId Id { get; set; }
    public long Size { get; set; }
}