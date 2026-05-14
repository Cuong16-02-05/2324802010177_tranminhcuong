using System.ComponentModel.DataAnnotations;

namespace ASC.Web.Areas.Configuration.Models
{
    public class MasterDataKeyViewModel
    {
        public string? UniqueId { get; set; }

        [Required(ErrorMessage = "Tên key không được để trống")]
        [Display(Name = "Key Name")]
        public string? Name { get; set; }

        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class MasterKeysViewModel
    {
        public List<MasterDataKeyViewModel> MasterDataKeys { get; set; } = new();
        public MasterDataKeyViewModel MasterDataKey { get; set; } = new();
    }

    public class MasterDataValueViewModel
    {
        public string? UniqueId { get; set; }

        [Required(ErrorMessage = "Tên value không được để trống")]
        [Display(Name = "Value Name")]
        public string? Name { get; set; }

        public string? MasterDataKeyId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Giá tham khảo (VND)")]
        public decimal? Price { get; set; }
    }

    public class MasterValuesViewModel
    {
        public List<MasterDataValueViewModel> MasterDataValues { get; set; } = new();
        public MasterDataValueViewModel MasterDataValue { get; set; } = new();
        public string? MasterDataKeyId { get; set; }
        public string? MasterDataKeyName { get; set; }
    }
}
