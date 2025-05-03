using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Config;

public interface ISecurityInitializer
{
    Task Initialize(string permissionJsonFilePath, string securityInitializationJsonFilePath);
}
