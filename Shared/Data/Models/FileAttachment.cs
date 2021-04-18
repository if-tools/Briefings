using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IFToolsBriefings.Shared.Data.Models
{
    [Table("Attachments")]
    public class FileAttachment
    {
        [Key]
        public int Id { get; set; }
        
        public string FileName { get; set; }
        
        public long FileSize { get; set; }
        
        public string Guid { get; set; }
        
        public bool Deleted { get; set; }
        
        public DateTime CreatedOn { get; set; }
    }
}