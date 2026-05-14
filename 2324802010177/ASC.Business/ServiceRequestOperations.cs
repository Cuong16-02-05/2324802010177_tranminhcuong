using ASC.DataAccess;
using ASC.Model;

namespace ASC.Business
{
    public interface IServiceRequestOperations
    {
        Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest request);
        Task<ServiceRequest?> GetServiceRequestByIdAsync(string id);
        Task<IEnumerable<ServiceRequest>> GetAllServiceRequestsAsync();
        Task<IEnumerable<ServiceRequest>> GetServiceRequestsByCustomerAsync(string customerEmail);
        Task<IEnumerable<ServiceRequest>> GetServiceRequestsByEngineerAsync(string engineerEmail);
        Task<ServiceRequest> UpdateServiceRequestStatusAsync(string id, string status, string updatedBy);
        Task<ServiceRequest> AssignEngineerAsync(string requestId, string engineerEmail, string updatedBy);
        Task<ServiceRequest> UpdateStatusAndAssignEngineerAsync(string id, string status, string? engineerEmail, string updatedBy);
        Task<ServiceRequest> UpdateServiceRequestAsync(ServiceRequest request);
        /// <summary>
        /// Khách yêu cầu deal lại giá — tối đa 2 lần.
        /// QuoteStatus → "Negotiating" (không phải Rejected), Status giữ "QuoteSent" để Admin thấy.
        /// </summary>
        Task<ServiceRequest> NegotiateQuoteAsync(string id, string customerEmail);
    }

    public class ServiceRequestOperations : IServiceRequestOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public ServiceRequestOperations(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public async Task<ServiceRequest> CreateServiceRequestAsync(ServiceRequest request)
        {
            request.UniqueId      = Guid.NewGuid().ToString();
            request.Status        = "Pending";
            request.PaymentStatus = "Unpaid";
            request.NegotiationCount = 0;
            request.RequestedDate = DateTime.UtcNow;
            request.CreatedDate   = DateTime.UtcNow;
            await _unitOfWork.Repository<ServiceRequest>().CreateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            return request;
        }

        public async Task<ServiceRequest?> GetServiceRequestByIdAsync(string id)
            => await _unitOfWork.Repository<ServiceRequest>().FindAsync(id);

        public async Task<IEnumerable<ServiceRequest>> GetAllServiceRequestsAsync()
            => await _unitOfWork.Repository<ServiceRequest>().FindAllAsync();

        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByCustomerAsync(string customerEmail)
            => await _unitOfWork.Repository<ServiceRequest>().FindAllByAsync(r => r.CustomerEmail == customerEmail);

        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByEngineerAsync(string engineerEmail)
            => await _unitOfWork.Repository<ServiceRequest>().FindAllByAsync(r => r.ServiceEngineer == engineerEmail);

        public async Task<ServiceRequest> UpdateServiceRequestStatusAsync(string id, string status, string updatedBy)
        {
            var r = await FindOrThrowAsync(id);
            r.Status = status; r.UpdatedBy = updatedBy; r.UpdatedDate = DateTime.UtcNow;
            if (status == "Completed") r.CompletedDate = DateTime.UtcNow;
            await SaveAsync(r);
            return r;
        }

        public async Task<ServiceRequest> AssignEngineerAsync(string requestId, string engineerEmail, string updatedBy)
        {
            var r = await FindOrThrowAsync(requestId);
            r.ServiceEngineer = engineerEmail;
            r.UpdatedBy = updatedBy; r.UpdatedDate = DateTime.UtcNow;
            await SaveAsync(r);
            return r;
        }

        public async Task<ServiceRequest> UpdateStatusAndAssignEngineerAsync(
            string id, string status, string? engineerEmail, string updatedBy)
        {
            var r = await FindOrThrowAsync(id);

            // Guard: chỉ cho assign engineer khi QuoteStatus = Approved
            if (!string.IsNullOrEmpty(engineerEmail)
                && r.EstimatedPrice.HasValue
                && r.QuoteStatus != "Approved")
            {
                throw new InvalidOperationException(
                    "Không thể phân công kỹ thuật viên khi khách hàng chưa duyệt báo giá.");
            }

            r.Status = status; r.UpdatedBy = updatedBy; r.UpdatedDate = DateTime.UtcNow;
            if (status == "Completed") r.CompletedDate = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(engineerEmail)) r.ServiceEngineer = engineerEmail;
            await SaveAsync(r);
            return r;
        }

        public async Task<ServiceRequest> UpdateServiceRequestAsync(ServiceRequest request)
        {
            request.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<ServiceRequest>().UpdateAsync(request);
            await _unitOfWork.SaveChangesAsync();
            return request;
        }

        public async Task<ServiceRequest> NegotiateQuoteAsync(string id, string customerEmail)
        {
            var r = await FindOrThrowAsync(id);

            if (r.CustomerEmail != customerEmail)
                throw new UnauthorizedAccessException("Bạn không có quyền deal giá cho yêu cầu này.");

            if (r.NegotiationCount >= 2)
                throw new InvalidOperationException(
                    "Đã đạt giới hạn deal giá (2 lần). Vui lòng liên hệ trực tiếp.");

            r.NegotiationCount++;
            // FIX: dùng "Negotiating" thay vì "Rejected" — đúng ngữ nghĩa nghiệp vụ
            // Status vẫn giữ "QuoteSent" để Admin thấy đơn còn đang xử lý, không mất vào Pending
            r.QuoteStatus = "Negotiating";
            r.UpdatedBy   = customerEmail;
            r.UpdatedDate = DateTime.UtcNow;
            await SaveAsync(r);
            return r;
        }

        private async Task<ServiceRequest> FindOrThrowAsync(string id)
            => await _unitOfWork.Repository<ServiceRequest>().FindAsync(id)
               ?? throw new Exception($"ServiceRequest '{id}' không tìm thấy.");

        private async Task SaveAsync(ServiceRequest r)
        {
            await _unitOfWork.Repository<ServiceRequest>().UpdateAsync(r);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
