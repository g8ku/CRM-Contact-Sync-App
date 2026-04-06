using Microsoft.AspNetCore.Mvc;
using ContactSync.API.Models;
using ContactSync.API.Repositories;
using ContactSync.API.Services;

namespace ContactSync.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly IContactRepository _repository;
        private readonly SyncService _syncService;

        public ContactsController(IContactRepository repository, SyncService syncService)
        {
            _repository = repository;
            _syncService = syncService;
        }

        // GET api/contacts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contacts = await _repository.GetAllAsync();
            return Ok(contacts);
        }

        // GET api/contacts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var contact = await _repository.GetByIdAsync(id);
            if (contact == null) return NotFound($"Contact {id} not found");
            return Ok(contact);
        }

        // GET api/contacts/status/pending
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(SyncStatus status)
        {
            var contacts = await _repository.GetBySyncStatusAsync(status);
            return Ok(contacts);
        }

        // POST api/contacts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Contact contact)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var created = await _repository.CreateAsync(contact);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/contacts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Contact contact)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var updated = await _repository.UpdateAsync(id, contact);
            if (updated == null) return NotFound($"Contact {id} not found");
            return Ok(updated);
        }

        // DELETE api/contacts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _repository.DeleteAsync(id);
            if (!success) return NotFound($"Contact {id} not found");
            return NoContent();
        }

        // POST api/contacts/5/sync
        [HttpPost("{id}/sync")]
        public async Task<IActionResult> SyncContact(int id)
        {
            var result = await _syncService.SyncContactAsync(id);
            if (!result.Success) return Ok(result);
            return Ok(result);
        }

        // POST api/contacts/sync/pending
        [HttpPost("sync/pending")]
        public async Task<IActionResult> SyncAllPending()
        {
            var result = await _syncService.SyncAllPendingAsync();
            return Ok(result);
        }
    }
}