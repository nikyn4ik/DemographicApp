using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public DateTime GeneratedOn { get; set; }

        public string GeneratedBy { get; set; }

        public string ReportData { get; set; }

        // Additional properties as needed
        public int ParentRegionId { get; set; }

        public int ChildRegionId { get; set; }

        public DateTime ReportDate { get; set; }
    }
}
