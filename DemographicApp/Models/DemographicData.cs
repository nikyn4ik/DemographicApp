using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemographicApp.Models
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
        public int AgeGroup1 { get; set; } // Пример: 0-18 лет
        public int AgeGroup2 { get; set; } // Пример: 19-35 лет
        public int AgeGroup3 { get; set; } // Пример: 36-60 лет
        public int AgeGroup4 { get; set; } // Пример: 60+ лет
    }
}