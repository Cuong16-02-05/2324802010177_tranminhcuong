using ASC.Business;
using ASC.Model;
using ASC.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ASC.Web.Areas.ServiceRequests.Controllers
{
    [Area("ServiceRequests")]
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatOperations _chatOps;
        private readonly IServiceRequestOperations _srOps;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatOperations chatOps,
            IServiceRequestOperations srOps,
            UserManager<ApplicationUser> userManager,
            ILogger<ChatController> logger)
        {
            _chatOps    = chatOps;
            _srOps      = srOps;
            _userManager = userManager;
            _logger     = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string serviceRequestId)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceRequestId))
                    return Json(new List<object>());

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                // ── Kiểm tra quyền truy cập ──
                var request = await _srOps.GetServiceRequestByIdAsync(serviceRequestId);
                if (request == null)
                    return NotFound();

                var roles = await _userManager.GetRolesAsync(currentUser);
                bool canAccess = roles.Contains("Admin")
                    || (roles.Contains("User")     && request.CustomerEmail    == currentUser.Email)
                    || (roles.Contains("Engineer") && request.ServiceEngineer  == currentUser.Email);

                if (!canAccess)
                {
                    _logger.LogWarning("ChatController.GetMessages: {Email} không có quyền truy cập {Id}",
                        currentUser.Email, serviceRequestId);
                    return Forbid();
                }

                var msgs = await _chatOps.GetMessagesByServiceRequestAsync(serviceRequestId);

                var result = msgs.Select(m => new
                {
                    id              = m.UniqueId,
                    fromEmail       = m.FromEmail,
                    fromDisplayName = m.FromDisplayName,
                    message         = m.Message,
                    senderRole      = m.SenderRole,
                    sentDate        = m.SentDate.ToString("HH:mm dd/MM/yyyy"),
                    isOwn           = m.FromEmail == currentUser.Email
                }).ToList();

                await _chatOps.MarkMessagesAsReadAsync(serviceRequestId, currentUser.Email ?? "");

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMessages failed for serviceRequestId={Id}", serviceRequestId);
                return StatusCode(500, new
                {
                    error  = ex.Message,
                    detail = ex.InnerException?.Message,
                    type   = ex.GetType().Name
                });
            }
        }
    }
}
