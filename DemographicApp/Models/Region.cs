using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemographicApp.Models
{
    public class Region
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int ParentRegionId { get; set; }
        public Region ParentRegion { get; set; }
        public ICollection<Region> ChildRegions { get; set; }
        public ICollection<DemographicData> DemographicData { get; set; }
    }
}
