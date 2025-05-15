using Microsoft.CodeAnalysis.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Config;

internal class InitializerJsonUser
{
    public string firstName { get; set; }
    public string lastName { get; set; }
    public string user { get; set; }
    public string password { get; set; }
    public string email { get; set; }
    public string phone { get; set; }
    public string jobtitle { get; set; }
    public List<InitializerJsonUserRole> userroles { get; set; }

    public InitializerJsonUser()
    {
        userroles = new List<InitializerJsonUserRole>();
    }
}
