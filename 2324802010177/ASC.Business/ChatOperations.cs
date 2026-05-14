using ASC.DataAccess;
using ASC.Model;

namespace ASC.Business
{
    public interface IChatOperations
    {
        Task<ChatMessage> SendMessageAsync(ChatMessage message);
        Task<List<ChatMessage>> GetMessagesByServiceRequestAsync(string serviceRequestId);
        Task MarkMessagesAsReadAsync(string serviceRequestId, string readerEmail);
        Task<int> GetUnreadCountAsync(string userEmail);
    }

    public class ChatOperations : IChatOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public ChatOperations(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public async Task<ChatMessage> SendMessageAsync(ChatMessage message)
        {
            message.UniqueId     = Guid.NewGuid().ToString();
            message.SentDate     = DateTime.UtcNow;
            message.CreatedDate  = DateTime.UtcNow;
            message.CreatedBy    = message.FromEmail;
            message.UpdatedDate  = DateTime.UtcNow;
            await _unitOfWork.Repository<ChatMessage>().CreateAsync(message);
            await _unitOfWork.SaveChangesAsync();
            return message;
        }

        public async Task<List<ChatMessage>> GetMessagesByServiceRequestAsync(string serviceRequestId)
        {
            var msgs = await _unitOfWork.Repository<ChatMessage>()
                .FindAllByAsync(m => m.ServiceRequestId == serviceRequestId);
            return msgs.OrderBy(m => m.SentDate).ToList();
        }

        public async Task MarkMessagesAsReadAsync(string serviceRequestId, string readerEmail)
        {
            var unread = (await _unitOfWork.Repository<ChatMessage>()
                .FindAllByAsync(m => m.ServiceRequestId == serviceRequestId
                                  && m.FromEmail != readerEmail
                                  && !m.IsRead)).ToList();

            if (!unread.Any()) return;

            // EF đã track các entity này từ FindAllByAsync.
            // Chỉ cần set property — KHÔNG gọi UpdateAsync (sẽ gây double-track lỗi).
            foreach (var msg in unread)
                msg.IsRead = true;

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userEmail)
        {
            // ToEmail không được set khi gửi tin → query theo FromEmail != userEmail
            // Đếm tất cả tin nhắn không phải do user này gửi và chưa đọc
            var unread = await _unitOfWork.Repository<ChatMessage>()
                .FindAllByAsync(m => m.FromEmail != userEmail && !m.IsRead);
            return unread.Count();
        }
    }
}
