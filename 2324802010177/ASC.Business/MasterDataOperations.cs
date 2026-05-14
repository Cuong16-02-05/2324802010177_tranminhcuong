using ASC.DataAccess;
using ASC.Model;

namespace ASC.Business
{
    public interface IMasterDataOperations
    {
        Task<IEnumerable<MasterDataKey>> GetAllMasterKeysAsync();
        Task<MasterDataKey> CreateMasterKeyAsync(string name, string createdBy);
        Task<MasterDataKey> UpdateMasterKeyAsync(string id, string name, bool isActive, string updatedBy);
        Task<IEnumerable<MasterDataValue>> GetMasterValuesByKeyAsync(string keyId);
        Task<MasterDataValue> CreateMasterValueAsync(string keyId, string name, string createdBy, decimal? price = null);
        Task<MasterDataValue> UpdateMasterValueAsync(string id, string name, bool isActive, string updatedBy, decimal? price = null);
    }

    public class MasterDataOperations : IMasterDataOperations
    {
        private readonly IUnitOfWork _unitOfWork;
        public MasterDataOperations(IUnitOfWork unitOfWork) { _unitOfWork = unitOfWork; }

        public async Task<IEnumerable<MasterDataKey>> GetAllMasterKeysAsync()
            => await _unitOfWork.Repository<MasterDataKey>().FindAllAsync();

        public async Task<MasterDataKey> CreateMasterKeyAsync(string name, string createdBy)
        {
            var key = new MasterDataKey
            {
                UniqueId = Guid.NewGuid().ToString(), Name = name,
                IsActive = true, CreatedBy = createdBy, CreatedDate = DateTime.UtcNow
            };
            await _unitOfWork.Repository<MasterDataKey>().CreateAsync(key);
            await _unitOfWork.SaveChangesAsync();
            return key;
        }

        public async Task<MasterDataKey> UpdateMasterKeyAsync(string id, string name, bool isActive, string updatedBy)
        {
            var key = await _unitOfWork.Repository<MasterDataKey>().FindAsync(id)
                ?? throw new Exception($"MasterDataKey {id} not found");
            key.Name = name; key.IsActive = isActive;
            key.UpdatedBy = updatedBy; key.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<MasterDataKey>().UpdateAsync(key);
            await _unitOfWork.SaveChangesAsync();
            return key;
        }

        public async Task<IEnumerable<MasterDataValue>> GetMasterValuesByKeyAsync(string keyId)
            => await _unitOfWork.Repository<MasterDataValue>().FindAllByAsync(v => v.MasterDataKeyId == keyId);

        public async Task<MasterDataValue> CreateMasterValueAsync(string keyId, string name, string createdBy, decimal? price = null)
        {
            var value = new MasterDataValue
            {
                UniqueId = Guid.NewGuid().ToString(), MasterDataKeyId = keyId,
                Name = name, IsActive = true, Price = price,
                CreatedBy = createdBy, CreatedDate = DateTime.UtcNow
            };
            await _unitOfWork.Repository<MasterDataValue>().CreateAsync(value);
            await _unitOfWork.SaveChangesAsync();
            return value;
        }

        public async Task<MasterDataValue> UpdateMasterValueAsync(string id, string name, bool isActive, string updatedBy, decimal? price = null)
        {
            var value = await _unitOfWork.Repository<MasterDataValue>().FindAsync(id)
                ?? throw new Exception($"MasterDataValue {id} not found");
            value.Name = name; value.IsActive = isActive; value.Price = price;
            value.UpdatedBy = updatedBy; value.UpdatedDate = DateTime.UtcNow;
            await _unitOfWork.Repository<MasterDataValue>().UpdateAsync(value);
            await _unitOfWork.SaveChangesAsync();
            return value;
        }
    }
}
