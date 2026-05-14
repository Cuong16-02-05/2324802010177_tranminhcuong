using System.ComponentModel.DataAnnotations;

namespace ASC.Model
{
    /// <summary>
    /// Tin nhắn chat giữa Customer ↔ Admin/Engineer trong context của 1 ServiceRequest
    /// </summary>
    public class ChatMessage : BaseTypes.BaseEntity, BaseTypes.IAuditTracker
    {
        [Key]
        public string? UniqueId { get; set; }

        public string? ServiceRequestId { get; set; }
        public string? FromEmail { get; set; }
        public string? FromDisplayName { get; set; }
        public string? ToEmail { get; set; }

        [Required]
        public string? Message { get; set; }

        public DateTime SentDate { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        /// <summary>Role của người gửi: Admin, Engineer, User</summary>
        public string? SenderRole { get; set; }
    }
}
