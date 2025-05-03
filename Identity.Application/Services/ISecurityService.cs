using Joseco.DDD.Core.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Application.Services;

public interface ISecurityService
{
    Task<Result<string>> Login(string username, string password);

    Task<Result<Guid>> RegisterUser(string email, string password, string firstname, string lastname);
}
