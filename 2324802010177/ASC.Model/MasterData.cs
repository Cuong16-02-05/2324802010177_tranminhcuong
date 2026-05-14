using System.ComponentModel.DataAnnotations;

namespace ASC.Model
{
    public class MasterDataKey : BaseTypes.BaseEntity, BaseTypes.IAuditTracker
    {
        [Key]
        public string? UniqueId { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }
    }

    public class MasterDataValue : BaseTypes.BaseEntity, BaseTypes.IAuditTracker
    {
        [Key]
        public string? UniqueId { get; set; }
        public string? MasterDataKeyId { get; set; }
        public string? Name { get; set; }
        public bool IsActive { get; set; }

        /// <summary>Giá dịch vụ tham khảo (VND). Null = Thỏa thuận.</summary>
        public decimal? Price { get; set; }
    }
}
