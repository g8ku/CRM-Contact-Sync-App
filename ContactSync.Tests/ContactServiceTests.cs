using Microsoft.EntityFrameworkCore;
using ContactSync.API.Data;
using ContactSync.API.Models;
using ContactSync.API.Repositories;
using ContactSync.API.Services;

namespace ContactSync.Tests;

public class ContactRepositoryTests
{
    private AppDbContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllContacts()
    {
        var context = GetInMemoryContext();
        context.Contacts.AddRange(
            new Contact { FirstName = "Sarah", LastName = "Chen", Email = "sarah@test.com", SyncStatus = SyncStatus.Synced },
            new Contact { FirstName = "James", LastName = "Okafor", Email = "james@test.com", SyncStatus = SyncStatus.Pending }
        );
        await context.SaveChangesAsync();

        var repo = new ContactRepository(context);
        var result = await repo.GetAllAsync();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsCorrectContact()
    {
        var context = GetInMemoryContext();
        var contact = new Contact { FirstName = "Sarah", LastName = "Chen", Email = "sarah@test.com" };
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        var repo = new ContactRepository(context);
        var result = await repo.GetByIdAsync(contact.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Email, Is.EqualTo("sarah@test.com"));
    }

    [Test]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var context = GetInMemoryContext();
        var repo = new ContactRepository(context);

        var result = await repo.GetByIdAsync(999);

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task CreateAsync_AddsContactToDatabase()
    {
        var context = GetInMemoryContext();
        var repo = new ContactRepository(context);
        var contact = new Contact { FirstName = "Maria", LastName = "Reyes", Email = "maria@test.com", Company = "Fin Group" };

        var result = await repo.CreateAsync(contact);

        Assert.That(result.Id, Is.GreaterThan(0));
        Assert.That(context.Contacts.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task UpdateAsync_UpdatesExistingContact()
    {
        var context = GetInMemoryContext();
        var contact = new Contact { FirstName = "Sarah", LastName = "Chen", Email = "sarah@test.com" };
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        var repo = new ContactRepository(context);
        var updated = await repo.UpdateAsync(contact.Id, new Contact
        {
            FirstName = "Sarah",
            LastName = "Chen",
            Email = "sarah.updated@test.com",
            Phone = "780-555-9999",
            Company = "New Corp"
        });

        Assert.That(updated, Is.Not.Null);
        Assert.That(updated!.Email, Is.EqualTo("sarah.updated@test.com"));
        Assert.That(updated.Company, Is.EqualTo("New Corp"));
    }

    [Test]
    public async Task UpdateAsync_ReturnsNull_WhenContactNotFound()
    {
        var context = GetInMemoryContext();
        var repo = new ContactRepository(context);

        var result = await repo.UpdateAsync(999, new Contact { FirstName = "Ghost" });

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteAsync_RemovesContact()
    {
        var context = GetInMemoryContext();
        var contact = new Contact { FirstName = "Sarah", LastName = "Chen", Email = "sarah@test.com" };
        context.Contacts.Add(contact);
        await context.SaveChangesAsync();

        var repo = new ContactRepository(context);
        var success = await repo.DeleteAsync(contact.Id);

        Assert.That(success, Is.True);
        Assert.That(context.Contacts.Count(), Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteAsync_ReturnsFalse_WhenContactNotFound()
    {
        var context = GetInMemoryContext();
        var repo = new ContactRepository(context);

        var result = await repo.DeleteAsync(999);

        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetBySyncStatusAsync_ReturnsOnlyMatchingContacts()
    {
        var context = GetInMemoryContext();
        context.Contacts.AddRange(
            new Contact { FirstName = "Sarah", LastName = "Chen", Email = "sarah@test.com", SyncStatus = SyncStatus.Synced },
            new Contact { FirstName = "James", LastName = "Okafor", Email = "james@test.com", SyncStatus = SyncStatus.Pending },
            new Contact { FirstName = "Maria", LastName = "Reyes", Email = "maria@test.com", SyncStatus = SyncStatus.Pending }
        );
        await context.SaveChangesAsync();

        var repo = new ContactRepository(context);
        var result = await repo.GetBySyncStatusAsync(SyncStatus.Pending);

        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(c => c.SyncStatus == SyncStatus.Pending), Is.True);
    }

    [Test]
    public async Task SyncContactAsync_ReturnsFalse_WhenContactNotFound()
    {
        var context = GetInMemoryContext();
        var repo = new ContactRepository(context);
        var syncService = new SyncService(repo);

        var result = await syncService.SyncContactAsync(999);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task SyncAllPendingAsync_OnlyProcessesPendingContacts()
    {
        var context = GetInMemoryContext();
        context.Contacts.AddRange(
            new Contact { FirstName = "Sarah", LastName = "Chen", Email = "sarah@test.com", SyncStatus = SyncStatus.Synced },
            new Contact { FirstName = "James", LastName = "Okafor", Email = "james@test.com", SyncStatus = SyncStatus.Pending },
            new Contact { FirstName = "Maria", LastName = "Reyes", Email = "maria@test.com", SyncStatus = SyncStatus.Pending }
        );
        await context.SaveChangesAsync();

        var repo = new ContactRepository(context);
        var syncService = new SyncService(repo);

        var result = await syncService.SyncAllPendingAsync();

        Assert.That(result.TotalProcessed, Is.EqualTo(2));
    }
}