using Joseco.Outbox.Contracts.Model;
using Joseco.Outbox.EFCore.Persistence;
using Microsoft.EntityFrameworkCore;
using Joseco.DDD.Core.Abstractions;

namespace Identity.Infrastructure.Persistence;

internal class UnitOfWork(SecurityDbContext dbContext) : IUnitOfWork, IOutboxDatabase<DomainEvent>
{
    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public DbSet<OutboxMessage<DomainEvent>> GetOutboxMessages()
    {
        return dbContext.OutboxMessages;
    }
}
