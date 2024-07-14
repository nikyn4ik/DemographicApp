using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Database.Models
{
    public class ComparisonResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ParentRegionId { get; set; }
        public int ChildRegionId { get; set; }
        public DateTime ComparisonDate { get; set; }
        public string ComparisonResultData { get; set; }
    }
}
