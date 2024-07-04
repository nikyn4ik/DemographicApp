using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemographicDatabase.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime GeneratedOn { get; set; }
        public string GeneratedBy { get; set; }
        public string ReportData { get; set; } // JSON или другой формат данных отчета
    }
}
