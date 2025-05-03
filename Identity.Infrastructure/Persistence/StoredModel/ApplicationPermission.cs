using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Persistence.StoredModel;

internal class ApplicationPermission
{
    public string Mnemonic { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    private ApplicationPermission()
    {
    }

    internal ApplicationPermission(string mnemonic, string name, string description)
    {
        Mnemonic = mnemonic;
        Name = name;
        Description = description;
    }

}
