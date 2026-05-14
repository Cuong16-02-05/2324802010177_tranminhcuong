using System.ComponentModel.DataAnnotations;

namespace ASC.Model
{
    public class ServiceRequest : BaseTypes.BaseEntity, BaseTypes.IAuditTracker
    {
        [Key]
        public string? UniqueId { get; set; }
        public string? RequestedServices { get; set; }
        public string? ServiceEngineer { get; set; }
        public string? Status { get; set; }
        public DateTime? RequestedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? VehicleName { get; set; }
        public string? VehicleRegistrationNumber { get; set; }
        public string? Comments { get; set; }
        public string? CustomerEmail { get; set; }

        // Báo giá & thanh toán (Lab 7+)
        public decimal? EstimatedPrice { get; set; }
        public string? EngineerNotes { get; set; }
        public string? QuoteStatus { get; set; }   // Pending | Approved | Rejected
        public decimal? FinalPrice { get; set; }
        public string? PaymentStatus { get; set; } // Unpaid | Paid

        /// <summary>Số lần khách deal lại giá. Giới hạn 2 lần.</summary>
        public int NegotiationCount { get; set; } = 0;
    }
}
