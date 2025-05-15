using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Config;

internal class InitializerJsonConfig
{
    public List<InitializerJsonRole> roles { get; set; }
    public InitializerJsonUser defaultUser { get; set; }

    public InitializerJsonConfig()
    {
        roles = new List<InitializerJsonRole>();
    }
}
