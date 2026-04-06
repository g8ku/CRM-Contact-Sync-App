using Microsoft.EntityFrameworkCore;
using ContactSync.API.Data;
using ContactSync.API.Models;

namespace ContactSync.API.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly AppDbContext _context;

        public ContactRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contact>> GetAllAsync()
        {
            return await _context.Contacts
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Contact?> GetByIdAsync(int id)
        {
            return await _context.Contacts.FindAsync(id);
        }

        public async Task<Contact> CreateAsync(Contact contact)
        {
            contact.CreatedAt = DateTime.UtcNow;
            contact.UpdatedAt = DateTime.UtcNow;
            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task<Contact?> UpdateAsync(int id, Contact contact)
        {
            var existing = await _context.Contacts.FindAsync(id);
            if (existing == null) return null;

            existing.FirstName = contact.FirstName;
            existing.LastName = contact.LastName;
            existing.Email = contact.Email;
            existing.Phone = contact.Phone;
            existing.Company = contact.Company;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return false;

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Contact>> GetBySyncStatusAsync(SyncStatus status)
        {
            return await _context.Contacts
                .Where(c => c.SyncStatus == status)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
        }
    }
}