using System;
using System.Collections.Generic;

namespace sistema_vacaciones_back.DTOs.Usuarios
{
    /// <summary>
    /// DTO para mostrar información completa de un usuario específico
    /// </summary>
    public class UsuarioDetalleDto
    {
        /// <summary>
        /// ID único del usuario
        /// </summary>
        public string Id { get; set; } = string.Empty;

        // ========== INFORMACIÓN PERSONAL ==========
        
        /// <summary>
        /// Nombres del usuario
        /// </summary>
        public string Nombres { get; set; } = string.Empty;

        /// <summary>
        /// Apellido paterno
        /// </summary>
        public string ApellidoPaterno { get; set; } = string.Empty;

        /// <summary>
        /// Apellido materno
        /// </summary>
        public string ApellidoMaterno { get; set; } = string.Empty;

        /// <summary>
        /// Nombre completo concatenado
        /// </summary>
        public string NombreCompleto { get; set; } = string.Empty;

        /// <summary>
        /// DNI o documento de identidad
        /// </summary>
        public string Dni { get; set; } = string.Empty;

        /// <summary>
        /// Indica si es extranjero
        /// </summary>
        public bool Extranjero { get; set; }

        /// <summary>
        /// Número de celular
        /// </summary>
        public string? NumeroCelular { get; set; }

        // ========== INFORMACIÓN DE CUENTA ==========

        /// <summary>
        /// Email del usuario
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Nombre de usuario (username)
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Indica si debe cambiar contraseña en próximo login
        /// </summary>
        public bool DebeChangePassword { get; set; }

        /// <summary>
        /// Email confirmado
        /// </summary>
        public bool EmailConfirmed { get; set; }

        /// <summary>
        /// Fecha del último login
        /// </summary>
        public DateTime? UltimoLogin { get; set; }

        // ========== INFORMACIÓN LABORAL ==========

        /// <summary>
        /// Fecha de ingreso a la empresa
        /// </summary>
        public DateTime FechaIngreso { get; set; }

        /// <summary>
        /// Empresa donde trabaja
        /// </summary>
        public string? Empresa { get; set; }

        /// <summary>
        /// ID del departamento
        /// </summary>
        public string? DepartamentoId { get; set; }

        /// <summary>
        /// Nombre del departamento
        /// </summary>
        public string? Departamento { get; set; }

        /// <summary>
        /// Código del departamento
        /// </summary>
        public string? CodigoDepartamento { get; set; }

        /// <summary>
        /// ID del jefe directo
        /// </summary>
        public string? JefeId { get; set; }

        /// <summary>
        /// Nombre completo del jefe directo
        /// </summary>
        public string? Manager { get; set; }

        /// <summary>
        /// Email del jefe directo
        /// </summary>
        public string? EmailManager { get; set; }

        /// <summary>
        /// Número de subordinados directos
        /// </summary>
        public int NumeroSubordinados { get; set; }

        // ========== CONFIGURACIÓN Y ROLES ==========

        /// <summary>
        /// Lista de roles asignados
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();

        /// <summary>
        /// Estado del usuario
        /// </summary>
        public string Estado { get; set; } = "Activo";

        /// <summary>
        /// Indica si está activo (no eliminado)
        /// </summary>
        public bool EstaActivo { get; set; } = true;

        // ========== AUDITORÍA ==========

        /// <summary>
        /// Usuario que creó este registro
        /// </summary>
        public string CreadoPor { get; set; } = string.Empty;

        /// <summary>
        /// Fecha de creación
        /// </summary>
        public DateTime FechaCreacion { get; set; }

        /// <summary>
        /// Usuario que actualizó este registro por última vez
        /// </summary>
        public string? ActualizadoPor { get; set; }

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime? FechaActualizacion { get; set; }

        // ========== ESTADÍSTICAS ==========

        /// <summary>
        /// Tiempo en la empresa (calculado)
        /// </summary>
        public string TiempoEnEmpresa { get; set; } = string.Empty;

        /// <summary>
        /// Lista de subordinados directos (resumen)
        /// </summary>
        public List<UsuarioSimpleDto> SubordinadosDirectos { get; set; } = new List<UsuarioSimpleDto>();
    }
}
