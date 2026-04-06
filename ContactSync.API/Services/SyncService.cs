using ContactSync.API.Models;
using ContactSync.API.Repositories;

namespace ContactSync.API.Services
{
    public class SyncService
    {
        private readonly IContactRepository _repository;

        public SyncService(IContactRepository repository)
        {
            _repository = repository;
        }

        public async Task<SyncResult> SyncContactAsync(int contactId)
        {
            var contact = await _repository.GetByIdAsync(contactId);
            if (contact == null)
                return new SyncResult { Success = false, Message = "Contact not found" };

            // Simulate CRM sync — in a real system this would be an API call
            // to Salesforce, Dynamics, etc.
            await Task.Delay(200);

            var success = SimulateCrmCall(contact);

            contact.SyncStatus = success ? SyncStatus.Synced : SyncStatus.Failed;
            await _repository.UpdateAsync(contactId, contact);

            return new SyncResult
            {
                Success = success,
                Message = success
                    ? $"{contact.FirstName} {contact.LastName} synced to CRM successfully"
                    : $"CRM sync failed for {contact.FirstName} {contact.LastName} — will retry"
            };
        }

        public async Task<BatchSyncResult> SyncAllPendingAsync()
        {
            var pending = await _repository.GetBySyncStatusAsync(SyncStatus.Pending);
            var results = new List<SyncResult>();

            foreach (var contact in pending)
            {
                var result = await SyncContactAsync(contact.Id);
                results.Add(result);
            }

            return new BatchSyncResult
            {
                TotalProcessed = results.Count,
                Succeeded = results.Count(r => r.Success),
                Failed = results.Count(r => !r.Success),
                Results = results
            };
        }

        // Simulates an 85% success rate — realistic for a CRM sync scenario
        private bool SimulateCrmCall(Contact contact)
        {
            var random = new Random();
            return random.NextDouble() > 0.15;
        }
    }

    public class SyncResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class BatchSyncResult
    {
        public int TotalProcessed { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public List<SyncResult> Results { get; set; } = new();
    }
}