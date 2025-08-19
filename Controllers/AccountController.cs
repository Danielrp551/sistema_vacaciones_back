using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SISTEMA_VACACIONES.DTOs.Account;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Mappers;
using sistema_vacaciones_back.Models;

namespace SISTEMA_VACACIONES.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPersonaRepository _personaRepository;
        private readonly ITokenService _tokenService;
        private readonly SignInManager<Usuario> _signInManager;

        public AccountController(
            IUsuarioRepository usuarioRepository,
            IPersonaRepository personaRepository,
            ITokenService tokenService,
            SignInManager<Usuario> signInManager)
        {
            _usuarioRepository = usuarioRepository;
            _personaRepository = personaRepository;
            _tokenService = tokenService;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _usuarioRepository.GetByUsernameAsync(loginDto.UserName);

            if (user == null)
                return Unauthorized("Usuario no encontrado");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
                return Unauthorized("Usuario o contraseña inválidos");

            var roles = await _usuarioRepository.GetUserRolesAsync(user);

            var permisos = await _usuarioRepository.GetUserPermissionsAsync(user);

            return Ok(new NewUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Token = _tokenService.CreateToken(user, roles),
                Persona = user.Persona.ToPersonaDto(),
                Roles = roles.ToList(),
                Permisos = permisos
            });

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var persona = new Persona
                {
                    Id = Guid.NewGuid().ToString(),
                    Nombres = registerDto.Nombres,
                    ApellidoPaterno = registerDto.ApellidoPaterno,
                    ApellidoMaterno = registerDto.ApellidoMaterno,
                    Dni = registerDto.Dni,
                    FechaIngreso = registerDto.FechaIngreso,
                    Extranjero = registerDto.Extranjero
                };

                await _personaRepository.AddAsync(persona);
                await _personaRepository.SaveChangesAsync();

                var appUser = new Usuario
                {
                    UserName = registerDto.Username,
                    Email = registerDto.Email,
                    PersonaId = persona.Id,
                    CreatedBy = "System",
                    CreatedOn = DateTime.UtcNow,
                    UpdatedBy = "System",
                    UpdatedOn = DateTime.UtcNow
                };

                var result = await _signInManager.UserManager.CreateAsync(appUser, registerDto.Password);
                if (!result.Succeeded)
                {
                    // Si la contraseña no es válida, eliminar la persona creada antes de retornar error
                    await _personaRepository.DeleteAsync(persona);
                    await _personaRepository.SaveChangesAsync();

                    var errorDescriptions = result.Errors.Select(e => e.Description);
                    return BadRequest(new { errors = errorDescriptions });
                }

                bool roleAdded = await _usuarioRepository.AddUserToRoleAsync(appUser, "User");
                if (!roleAdded)
                    return StatusCode(500, "No se pudo asignar el rol");

                var permisos = await _usuarioRepository.GetUserPermissionsAsync(appUser);

                return Ok(new NewUserDto
                {
                    Id = appUser.Id,
                    UserName = appUser.UserName,
                    Email = appUser.Email,
                    Token = _tokenService.CreateToken(appUser, new List<string> { "User" }),
                    Persona = persona.ToPersonaDto(),
                    Roles = new List<string> { "User" },
                    Permisos = permisos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}