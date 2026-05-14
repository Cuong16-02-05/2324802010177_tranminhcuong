using ASC.Business;
using ASC.Model;
using ASC.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    [Authorize]
    public class ServicePricingController : BaseController
    {
        private readonly IMasterDataOperations _masterData;

        public ServicePricingController(IMasterDataOperations masterData)
        {
            _masterData = masterData;
        }

        /// <summary>
        /// Bảng giá dịch vụ công khai - tất cả roles đều xem được.
        /// Đọc từ MasterDataKey tên "ServiceType".
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allKeys = await _masterData.GetAllMasterKeysAsync();
            // Lấy tất cả keys active, không chỉ ServiceType - hiển thị toàn bộ dịch vụ
            var serviceKeys = allKeys.Where(k => k.IsActive).ToList();

            var services = new List<MasterDataValue>();
            foreach (var key in serviceKeys)
            {
                var values = await _masterData.GetMasterValuesByKeyAsync(key.UniqueId!);
                services.AddRange(values.Where(v => v.IsActive));
            }

            return View(services.OrderBy(s => s.Name).ToList());
        }
    }
}
