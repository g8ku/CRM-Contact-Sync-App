using ContactSync.API.Models;

namespace ContactSync.API.Repositories
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetAllAsync();
        Task<Contact?> GetByIdAsync(int id);
        Task<Contact> CreateAsync(Contact contact);
        Task<Contact?> UpdateAsync(int id, Contact contact);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<Contact>> GetBySyncStatusAsync(SyncStatus status);
    }
}