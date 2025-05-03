using Identity.Infrastructure.Persistence.StoredModel;
using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.EFCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Joseco.DDD.Core.Abstractions;

namespace Identity.Infrastructure.Persistence;

internal class SecurityDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IDatabase
{
    public DbSet<OutboxMessage<DomainEvent>> OutboxMessages { get; set; }

    public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddOutboxModel<DomainEvent>();
        base.OnModelCreating(modelBuilder);
    }

    public void Migrate()
    {
        Database.Migrate();
    }

}