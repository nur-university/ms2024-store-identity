using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Config;

internal class InitializerJsonRole
{
    public string role { get; set; }
    public List<InitializerJsonPermission> permissions { get; set; }
}
