using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GYMappWeb.Models
{
    public class LogEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string Level { get; set; }

        public string Message { get; set; }

        public string Exception { get; set; }

        [StringLength(255)]
        public string Logger { get; set; }

        [StringLength(100)]
        public string Controller { get; set; }

        [StringLength(100)]
        public string Action { get; set; }

        [StringLength(100)]
        public string User { get; set; }

        [StringLength(500)]
        public string Url { get; set; }

        [StringLength(50)]
        public string IpAddress { get; set; }

        // Additional useful fields
        [StringLength(10)]
        public string HttpMethod { get; set; }

        public int? StatusCode { get; set; }

        [StringLength(1000)]
        public string RequestPath { get; set; }

        public long? Duration { get; set; } // Duration in milliseconds
    }
}
