using ASC.Business;
using ASC.DataAccess;
using ASC.Model;
using ASC.Web.Controllers;
using ASC.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    [Authorize]
    public class ServiceRequestController : BaseController
    {
        private readonly IServiceRequestOperations _serviceRequestOps;
        private readonly IUnitOfWork               _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender              _emailSender;

        public ServiceRequestController(
            IServiceRequestOperations serviceRequestOps,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            _serviceRequestOps = serviceRequestOps;
            _unitOfWork        = unitOfWork;
            _userManager       = userManager;
            _emailSender       = emailSender;
        }

        // ── TẠO YÊU CẦU ─────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> ServiceRequest()
        {
            var masterKeys = await _unitOfWork.Repository<MasterDataKey>().FindAllByAsync(k => k.IsActive);
            var allValues  = new List<MasterDataValue>();
            foreach (var key in masterKeys)
                allValues.AddRange(await _unitOfWork.Repository<MasterDataValue>()
                    .FindAllByAsync(v => v.MasterDataKeyId == key.UniqueId && v.IsActive));

            ViewBag.ServiceValues = allValues.OrderBy(v => v.Name).ToList();
            ViewBag.Services      = masterKeys.ToList();
            return View(new ASC.Model.ServiceRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ServiceRequest(ASC.Model.ServiceRequest model)
        {
            if (!ModelState.IsValid)
            {
                var keys = await _unitOfWork.Repository<MasterDataKey>().FindAllByAsync(k => k.IsActive);
                var vals = new List<MasterDataValue>();
                foreach (var key in keys)
                    vals.AddRange(await _unitOfWork.Repository<MasterDataValue>()
                        .FindAllByAsync(v => v.MasterDataKeyId == key.UniqueId && v.IsActive));
                ViewBag.ServiceValues = vals.OrderBy(v => v.Name).ToList();
                ViewBag.Services      = keys.ToList();
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            model.CustomerEmail = user!.Email;
            model.CreatedBy     = user.Email;

            var created = await _serviceRequestOps.CreateServiceRequestAsync(model);

            await _emailSender.SendEmailAsync(user.Email!,
                "Yêu cầu dịch vụ đã được tiếp nhận",
                $"<h3>Yêu cầu dịch vụ của bạn đã được tạo thành công.</h3>" +
                $"<p>Xe: <strong>{created.VehicleName}</strong> ({created.VehicleRegistrationNumber})</p>" +
                $"<p>Dịch vụ: {created.RequestedServices}</p>" +
                $"<p>Trạng thái: <strong>Pending</strong> — Admin sẽ xem xét và gửi báo giá sớm nhất có thể.</p>");

            TempData["Success"] = "Tạo yêu cầu dịch vụ thành công! Chúng tôi sẽ gửi báo giá cho bạn sớm.";
            return RedirectToAction("Dashboard", "Dashboard");
        }

        // ── CHI TIẾT ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var request = await _serviceRequestOps.GetServiceRequestByIdAsync(id);
            if (request == null) return NotFound();

            if (User.IsInRole(Constants.Roles.Admin))
            {
                var engineers = await _userManager.GetUsersInRoleAsync(Constants.Roles.Engineer);
                ViewBag.Engineers = engineers.ToList();

                // Load giá tham khảo từ MasterData để hiện sẵn vào ô báo giá
                if (!string.IsNullOrEmpty(request.RequestedServices))
                {
                    var allValues = await _unitOfWork.Repository<MasterDataValue>()
                        .FindAllByAsync(v => v.Name == request.RequestedServices && v.IsActive);
                    var matched = allValues.FirstOrDefault();
                    ViewBag.SuggestedPrice = matched?.Price;
                }
            }

            return View(request);
        }

        // ── CẬP NHẬT TRẠNG THÁI (Admin/Engineer) ─────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineer")]
        public async Task<IActionResult> UpdateStatus(string id, string status, string? engineerEmail)
        {
            try
            {
                var request = await _serviceRequestOps.UpdateStatusAndAssignEngineerAsync(
                    id, status, engineerEmail, User.Identity?.Name ?? "system");

                await _emailSender.SendEmailAsync(request.CustomerEmail!,
                    $"Cập nhật trạng thái dịch vụ: {status}",
                    $"<h3>Yêu cầu dịch vụ của bạn đã được cập nhật.</h3>" +
                    $"<p>Trạng thái mới: <strong>{status}</strong></p>");

                TempData["Success"] = $"Đã cập nhật trạng thái: {status}";
            }
            catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction("Dashboard", "Dashboard");
        }

        // ── BÁO GIÁ & THANH TOÁN (Admin/Engineer) ────────────────────────
        // quoteAction: "draft" | "send" | "resend" | "payment"

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Engineer")]
        public async Task<IActionResult> UpdateQuote(
            string id,
            decimal? estimatedPrice,
            string? engineerNotes,
            decimal? finalPrice,
            string? paymentStatus,
            string? quoteAction)
        {
            var request = await _serviceRequestOps.GetServiceRequestByIdAsync(id);
            if (request == null) return NotFound();

            request.UpdatedBy = User.Identity?.Name ?? "system";

            switch (quoteAction)
            {
                // ── Lưu nháp: ghi giá + ghi chú, KHÔNG gửi email, KHÔNG đổi Status ──
                case "draft":
                    request.EstimatedPrice = estimatedPrice;
                    request.EngineerNotes  = engineerNotes;
                    if (string.IsNullOrEmpty(request.QuoteStatus))
                        request.QuoteStatus = "Pending";
                    await _serviceRequestOps.UpdateServiceRequestAsync(request);
                    TempData["Success"] = "Đã lưu nháp báo giá.";
                    break;

                // ── Gửi báo giá lần đầu → Status = QuoteSent, QuoteStatus = Pending ──
                case "send":
                    if (!estimatedPrice.HasValue || estimatedPrice <= 0)
                    {
                        TempData["Error"] = "Vui lòng nhập giá ước tính (> 0) trước khi gửi.";
                        return RedirectToAction("Details", new { id });
                    }
                    request.EstimatedPrice = estimatedPrice;
                    request.EngineerNotes  = engineerNotes;
                    request.QuoteStatus    = "Pending";
                    request.Status         = "QuoteSent";
                    await _serviceRequestOps.UpdateServiceRequestAsync(request);

                    await _emailSender.SendEmailAsync(
                        request.CustomerEmail!,
                        "Báo giá dịch vụ xe — vui lòng xác nhận",
                        $"<h3>Garage đã gửi báo giá cho yêu cầu #{id.Substring(0, 8)}</h3>" +
                        $"<p>Dịch vụ: <strong>{request.RequestedServices}</strong></p>" +
                        $"<p>Giá ước tính: <strong>{estimatedPrice.Value:N0} ₫</strong></p>" +
                        $"<p>Ghi chú kỹ thuật: {engineerNotes ?? "-"}</p>" +
                        $"<p>Vui lòng đăng nhập hệ thống để <strong>đồng ý</strong> hoặc " +
                        $"<strong>deal lại giá</strong> (tối đa 2 lần).</p>");

                    TempData["Success"] = $"Đã gửi báo giá {estimatedPrice.Value:N0} ₫ cho khách. Chờ khách xác nhận.";
                    break;

                // ── Gửi lại báo giá sau khi khách Negotiating ──
                case "resend":
                    if (!estimatedPrice.HasValue || estimatedPrice <= 0)
                    {
                        TempData["Error"] = "Vui lòng nhập giá mới trước khi gửi lại.";
                        return RedirectToAction("Details", new { id });
                    }
                    if (request.QuoteStatus != "Negotiating")
                    {
                        TempData["Error"] = "Chỉ có thể gửi lại khi khách đang thương lượng.";
                        return RedirectToAction("Details", new { id });
                    }
                    request.EstimatedPrice = estimatedPrice;
                    request.EngineerNotes  = engineerNotes;
                    request.QuoteStatus    = "Pending";
                    request.Status         = "QuoteSent";
                    await _serviceRequestOps.UpdateServiceRequestAsync(request);

                    await _emailSender.SendEmailAsync(
                        request.CustomerEmail!,
                        "Báo giá điều chỉnh — vui lòng xác nhận",
                        $"<h3>Garage đã cập nhật báo giá #{id.Substring(0, 8)}</h3>" +
                        $"<p>Giá mới: <strong>{estimatedPrice.Value:N0} ₫</strong></p>" +
                        $"<p>Ghi chú: {engineerNotes ?? "-"}</p>" +
                        $"<p>Còn <strong>{2 - request.NegotiationCount}</strong> lần thương lượng.</p>");

                    TempData["Success"] = $"Đã gửi lại báo giá điều chỉnh {estimatedPrice.Value:N0} ₫.";
                    break;

                // ── Xác nhận thanh toán: ghi FinalPrice, đóng đơn ──
                case "payment":
                    if (paymentStatus == "Paid" && (!finalPrice.HasValue || finalPrice <= 0))
                    {
                        TempData["Error"] = "Vui lòng nhập giá cuối trước khi xác nhận đã thanh toán.";
                        return RedirectToAction("Details", new { id });
                    }
                    request.FinalPrice    = finalPrice ?? request.EstimatedPrice;
                    request.PaymentStatus = paymentStatus;
                    if (paymentStatus == "Paid")
                        request.Status = "Completed";
                    await _serviceRequestOps.UpdateServiceRequestAsync(request);

                    if (paymentStatus == "Paid")
                    {
                        await _emailSender.SendEmailAsync(
                            request.CustomerEmail!,
                            "Thanh toán thành công — Cảm ơn bạn!",
                            $"<h3>Yêu cầu #{id.Substring(0, 8)} đã hoàn tất</h3>" +
                            $"<p>Dịch vụ: {request.RequestedServices}</p>" +
                            $"<p>Số tiền: <strong>{request.FinalPrice:N0} ₫</strong></p>" +
                            $"<p>Cảm ơn bạn đã sử dụng dịch vụ của chúng tôi!</p>");
                        TempData["Success"] = "Đã xác nhận thanh toán. Đơn hoàn tất!";
                    }
                    else
                    {
                        TempData["Success"] = "Đã cập nhật trạng thái thanh toán.";
                    }
                    break;

                default:
                    TempData["Error"] = "Hành động không hợp lệ.";
                    break;
            }

            return RedirectToAction("Details", new { id });
        }

        // ── KHÁCH ĐỒNG Ý BÁO GIÁ ────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> ApproveQuote(string id)
        {
            var request   = await _serviceRequestOps.GetServiceRequestByIdAsync(id);
            if (request == null) return NotFound();

            var userEmail = (await _userManager.GetUserAsync(User))?.Email ?? "";
            if (request.CustomerEmail != userEmail)
            {
                TempData["Error"] = "Bạn không có quyền xác nhận yêu cầu này.";
                return RedirectToAction("Details", new { id });
            }

            request.QuoteStatus = "Approved";
            request.Status      = "InProgress";
            request.UpdatedBy   = userEmail;
            await _serviceRequestOps.UpdateServiceRequestAsync(request);

            // Thông báo tất cả Admin
            var admins = await _userManager.GetUsersInRoleAsync(Constants.Roles.Admin);
            foreach (var admin in admins)
            {
                await _emailSender.SendEmailAsync(admin.Email!,
                    $"Khách đồng ý báo giá — #{id.Substring(0, 8)}",
                    $"<h3>Khách hàng {userEmail} đã đồng ý giá <strong>{request.EstimatedPrice:N0} ₫</strong>.</h3>" +
                    $"<p>Vui lòng phân công kỹ thuật viên để tiến hành dịch vụ.</p>");
            }

            TempData["Success"] = "Bạn đã đồng ý báo giá. Kỹ thuật viên sẽ sớm được phân công!";
            return RedirectToAction("Details", new { id });
        }

        // ── KHÁCH YÊU CẦU DEAL LẠI GIÁ ──────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> NegotiateQuote(string id)
        {
            try
            {
                var userEmail = (await _userManager.GetUserAsync(User))?.Email ?? "";
                var request   = await _serviceRequestOps.NegotiateQuoteAsync(id, userEmail);

                var admins = await _userManager.GetUsersInRoleAsync(Constants.Roles.Admin);
                foreach (var admin in admins)
                {
                    await _emailSender.SendEmailAsync(admin.Email!,
                        $"Khách yêu cầu deal lại giá — #{id.Substring(0, 8)}",
                        $"<h3>Khách {userEmail} muốn thương lượng lại giá (lần {request.NegotiationCount}/2)</h3>" +
                        $"<p>Giá hiện tại: {request.EstimatedPrice:N0} ₫</p>" +
                        $"<p>Vui lòng vào chi tiết đơn để cập nhật giá mới rồi gửi lại cho khách.</p>");
                }

                TempData["Success"] = $"Đã gửi yêu cầu deal giá (lần {request.NegotiationCount}/2). Admin sẽ liên hệ lại sớm.";
            }
            catch (InvalidOperationException ex)  { TempData["Error"] = ex.Message; }
            catch (UnauthorizedAccessException ex) { TempData["Error"] = ex.Message; }

            return RedirectToAction("Details", new { id });
        }
    }
}
