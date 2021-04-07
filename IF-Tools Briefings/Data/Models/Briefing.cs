using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IFToolsBriefings.Data.Models
{
    [Table("Briefings")]
    public class Briefing
    {
        [Key]
        public int Id { get; set; }
        
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
        
        [NotMapped]
        public string[] AttachmentsArray
        {
            get => Attachments.Split(',');
            set => Attachments = string.Join(',', value);
        }

        public TimeSpan GetTimeEnroute()
        {
            return TimeSpan.FromTicks(TimeEnroute);
        }
    }
}
