using System;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;

namespace IFToolsBriefings.Shared.Data.Models
{
    [Table("Attachments")]
    public class FileAttachment
    {
        public ObjectId Id { get; set; }
        
        public string FileName { get; set; }
        
        [NotMapped]
        public string FileUrl { get; set; }
        
        public long FileSize { get; set; }
        
        public string Guid { get; set; }
        
        public bool Deleted { get; set; }
        
        public DateTime CreatedOn { get; set; }
    }
}