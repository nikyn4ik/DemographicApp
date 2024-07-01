using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemographicApp.Models
{
    public class VerificationRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int DemographicDataId { get; set; }
        public DemographicData DemographicData { get; set; }
        public int RequestedById { get; set; }
        public User RequestedBy { get; set; }
        public DateTime RequestedOn { get; set; }
        public bool IsVerified { get; set; }
        public int? VerifiedById { get; set; }
        public User VerifiedBy { get; set; }
        public DateTime? VerifiedOn { get; set; }
        public string VerificationNotes { get; set; }
    }
}
