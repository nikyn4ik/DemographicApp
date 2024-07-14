using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database.Models
{
    public class DemographicData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int RegionId { get; set; }
        public Region Region { get; set; }

        public DateTime Date { get; set; }
        public int Population { get; set; }
        public int MalePopulation { get; set; }
        public int FemalePopulation { get; set; }
    }
}
