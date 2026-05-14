using ASC.Business;
using ASC.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

namespace ASC.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatOperations _chatOps;
        private readonly IServiceRequestOperations _srOps;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(
            IChatOperations chatOps,
            IServiceRequestOperations srOps,
            UserManager<ApplicationUser> userManager,
            ILogger<ChatHub> logger)
        {
            _chatOps    = chatOps;
            _srOps      = srOps;
            _userManager = userManager;
            _logger     = logger;
        }

        // ── Lấy user hiện tại (fallback an toàn trong SignalR context) ──
        private async Task<ApplicationUser?> ResolveUserAsync()
        {
            ApplicationUser? user = null;
            try { user = await _userManager.GetUserAsync(Context.User!); } catch { }
            if (user == null)
            {
                var name = Context.User?.Identity?.Name;
                if (!string.IsNullOrEmpty(name))
                    user = await _userManager.FindByNameAsync(name)
                        ?? await _userManager.FindByEmailAsync(name);
            }
            return user;
        }

        // ── Kiểm tra user có quyền truy cập đơn này không ──
        private async Task<bool> CanAccessRequestAsync(ApplicationUser user, string serviceRequestId)
        {
            var roles = await _userManager.GetRolesAsync(user);

            // Admin xem được tất cả
            if (roles.Contains("Admin")) return true;

            var request = await _srOps.GetServiceRequestByIdAsync(serviceRequestId);
            if (request == null) return false;

            // User chỉ xem đơn của chính mình
            if (roles.Contains("User"))
                return request.CustomerEmail == user.Email;

            // Engineer chỉ xem đơn được phân công cho mình
            if (roles.Contains("Engineer"))
                return request.ServiceEngineer == user.Email;

            return false;
        }

        public async Task JoinServiceRequest(string serviceRequestId)
        {
            var user = await ResolveUserAsync();
            if (user == null)
            {
                await Clients.Caller.SendAsync("ChatError", "Phiên đăng nhập hết hạn. Vui lòng tải lại trang.");
                return;
            }

            if (!await CanAccessRequestAsync(user, serviceRequestId))
            {
                _logger.LogWarning("ChatHub.JoinServiceRequest: {Email} không có quyền truy cập {Id}", user.Email, serviceRequestId);
                await Clients.Caller.SendAsync("ChatError", "Bạn không có quyền truy cập cuộc trò chuyện này.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, serviceRequestId);
            _logger.LogDebug("ChatHub: {ConnectionId} joined group {Group}", Context.ConnectionId, serviceRequestId);
        }

        public async Task LeaveServiceRequest(string serviceRequestId)
            => await Groups.RemoveFromGroupAsync(Context.ConnectionId, serviceRequestId);

        public async Task SendMessage(string serviceRequestId, string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            var user = await ResolveUserAsync();
            if (user == null)
            {
                _logger.LogWarning("ChatHub.SendMessage: không xác định được user. ConnectionId={Id}", Context.ConnectionId);
                await Clients.Caller.SendAsync("ChatError", "Phiên đăng nhập hết hạn. Vui lòng tải lại trang.");
                return;
            }

            if (!await CanAccessRequestAsync(user, serviceRequestId))
            {
                await Clients.Caller.SendAsync("ChatError", "Bạn không có quyền gửi tin nhắn trong đơn này.");
                return;
            }

            var roles       = await _userManager.GetRolesAsync(user);
            var senderRole  = roles.FirstOrDefault() ?? "User";
            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            if (string.IsNullOrEmpty(displayName)) displayName = user.Email ?? "Unknown";

            ChatMessage saved;
            try
            {
                saved = await _chatOps.SendMessageAsync(new ChatMessage
                {
                    ServiceRequestId = serviceRequestId,
                    FromEmail        = user.Email,
                    FromDisplayName  = displayName,
                    Message          = message.Trim(),
                    SenderRole       = senderRole
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ChatHub.SendMessage: lỗi lưu DB. User={Email}", user.Email);
                await Clients.Caller.SendAsync("ChatError", "Không thể gửi tin nhắn. Thử lại sau.");
                return;
            }

            var payload = new
            {
                id              = saved.UniqueId,
                fromEmail       = saved.FromEmail,
                fromDisplayName = saved.FromDisplayName,
                message         = saved.Message,
                senderRole      = saved.SenderRole,
                sentDate        = saved.SentDate.ToString("HH:mm dd/MM/yyyy")
            };

            // Gửi cho người gửi với isOwn = true
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                payload.id, payload.fromEmail, payload.fromDisplayName,
                payload.message, payload.senderRole, payload.sentDate,
                isOwn = true
            });

            // Gửi cho những người khác trong group với isOwn = false
            await Clients.GroupExcept(serviceRequestId, new[] { Context.ConnectionId })
                .SendAsync("ReceiveMessage", new
                {
                    payload.id, payload.fromEmail, payload.fromDisplayName,
                    payload.message, payload.senderRole, payload.sentDate,
                    isOwn = false
                });
        }
    }
}
