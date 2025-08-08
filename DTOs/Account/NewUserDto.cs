using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using sistema_vacaciones_back.DTOs.Persona;

namespace SISTEMA_VACACIONES.DTOs.Account
{
    public class NewUserDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }  
        public PersonaDto Persona { get; set; }
        public IList<string> Roles { get; set; }  
        public List<string> Permisos { get; set; }    
    }
}