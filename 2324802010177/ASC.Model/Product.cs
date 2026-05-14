using System.ComponentModel.DataAnnotations;

namespace ASC.Model
{
    public class Product : BaseTypes.BaseEntity, BaseTypes.IAuditTracker
    {
        [Key]
        public string? UniqueId { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Category { get; set; }
    }
}
