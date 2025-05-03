using Joseco.DDD.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Events;

public record UserCreated : DomainEvent
{
    public Guid UserId { get; init; }
    public string FullName { get; init; }
    public string Email { get; init; }

    public UserCreated(Guid userId, string fullName, string email) : base()
    {
        UserId = userId;
        FullName = fullName;
        Email = email;
    }
}
