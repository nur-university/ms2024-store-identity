using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Config;

internal class PermissionJsonConfig
{
    public string Mnemonic { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public PermissionJsonConfig()
    {
        Mnemonic = "";
        Name = "";
        Description = "";
    }
}
