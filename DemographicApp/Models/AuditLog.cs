using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DemographicApp.Models
{
    public class AuditLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } // Пример: "Edit", "Delete", "Verify"
        public string EntityName { get; set; } // Пример: "DemographicData"
        public int EntityId { get; set; }
        public string Changes { get; set; } // JSON или другой формат для хранения изменений
    }
}
