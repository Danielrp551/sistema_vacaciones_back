using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace sistema_vacaciones_back.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(IdentityUser user);
    }
}