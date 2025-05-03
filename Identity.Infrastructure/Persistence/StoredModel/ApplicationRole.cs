using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Persistence.StoredModel;

internal class ApplicationRole : IdentityRole<Guid> {

    public ApplicationRole(): base()
    {
        Id = Guid.NewGuid();
    }

}
