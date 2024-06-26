using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;

namespace IFToolsBriefings.Shared.Data.Models
{
    [Table("Briefings")]
    public class Briefing
    {
        public ObjectId Id { get; set; }
        
        // this is stored separately because ObjectId.ToString() returns zeroes on WASM
        public string StringId { get; set; }
        
        public string Server { get; set; }

        public string Origin { get; set; }
        
        public string Destination { get; set; }
        
        public string OriginRunway { get; set; }

        public string DestinationRunway { get; set; }

        public int FlightLevel { get; set; }
        
        public double CruiseSpeed { get; set; }
        
        public DateTime DepartureTime { get; set; }

        // stored in Ticks
        public long TimeEnroute { get; set; }
        
        public string FlightPlan { get; set; }
        
        public string Remarks { get; set; }

        public string Author { get; set; }
        
        public string Attachments { get; set; }
        
        public string EditPasswordHash { get; set; }
        
        public string ViewPasswordHash { get; set; }
        
        public DateTime? CreatedOn { get; set; }
        
        public DateTime? LastEdited { get; set; }
        
        // These are needed due to the fact that cryptography is not supported on the Web.
        [NotMapped]
        public string EditPassword { get; set; }
        
        [NotMapped]
        public string ViewPassword { get; set; }
        
        [NotMapped]
        public string[] AttachmentsArray
        {
            get => Attachments == null ? Array.Empty<string>() : Attachments.Split(',');
            set => Attachments = string.Join(',', value);
        }
        
        [NotMapped]
        public bool IsPrivate => !string.IsNullOrWhiteSpace(ViewPasswordHash);
        
        public TimeSpan GetTimeEnroute()
        {
            return TimeSpan.FromTicks(TimeEnroute);
        }

        public static Briefing Default => new ();
    }
}
