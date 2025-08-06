using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISTEMA_VACACIONES.DTOs.Account
{
    public class NewUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }  
        public string NombreCompleto { get; set; }
        public IList<string> Roles { get; set; }  
        public List<string> AllowedRoutes { get; set; }    
    }
}