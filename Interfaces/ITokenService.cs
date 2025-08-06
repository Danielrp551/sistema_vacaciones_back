using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace SISTEMA_VACACIONES.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(IdentityUser user);
    }
}