using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using sistema_vacaciones_back.Models;
using sistema_vacaciones_back.Models.Enums;

namespace sistema_vacaciones_back.Data
{
    public class ApplicationDBContext : IdentityDbContext<Usuario>
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        //TABLAS 
        // VACACIONES
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public new DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<Departamento> Departamentos { get; set; }

        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Vacaciones> Vacaciones { get; set; }
        public DbSet<SolicitudVacaciones> SolicitudesVacaciones { get; set; }
        public DbSet<Restriccion> Restricciones { get; set; }

        // AUDITORÍA
        public DbSet<AuditoriaAcciones> AuditoriaAcciones { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RolPermiso>()
                .HasKey(rp => new { rp.RolId, rp.PermisoId });

            builder.Entity<Usuario>()
                .HasOne(u => u.Jefe)
                .WithMany(j => j.Subordinados)
                .HasForeignKey(u => u.JefeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de relación Usuario-Departamento
            builder.Entity<Usuario>()
                .HasOne(u => u.Departamento)
                .WithMany(d => d.Empleados)
                .HasForeignKey(u => u.DepartamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de jefe de departamento
            builder.Entity<Departamento>()
                .HasOne(d => d.JefeDepartamento)
                .WithMany()
                .HasForeignKey(d => d.JefeDepartamentoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices únicos
            builder.Entity<Permiso>()
                .HasIndex(p => new { p.Modulo, p.CodigoPermiso })
                .IsUnique();

            builder.Entity<Departamento>()
                .HasIndex(d => d.Codigo)
                .IsUnique()
                .HasFilter("[Codigo] IS NOT NULL");

            // ===== CONFIGURACIÓN DE AUDITORÍA =====
            
            // Índice compuesto para consultas por módulo y fecha (más común)
            builder.Entity<AuditoriaAcciones>()
                .HasIndex(a => new { a.Modulo, a.FechaHora })
                .HasDatabaseName("IX_AuditoriaAcciones_Modulo_FechaHora");

            // Índice para consultas por usuario ejecutor
            builder.Entity<AuditoriaAcciones>()
                .HasIndex(a => a.UsuarioEjecutorId)
                .HasDatabaseName("IX_AuditoriaAcciones_UsuarioEjecutor");

            // Índice para consultas por usuario afectado
            builder.Entity<AuditoriaAcciones>()
                .HasIndex(a => a.UsuarioAfectadoId)
                .HasDatabaseName("IX_AuditoriaAcciones_UsuarioAfectado")
                .HasFilter("[UsuarioAfectadoId] IS NOT NULL");

            // Índice para consultas por tipo de acción
            builder.Entity<AuditoriaAcciones>()
                .HasIndex(a => a.TipoAccion)
                .HasDatabaseName("IX_AuditoriaAcciones_TipoAccion");

            // Índice para consultas por tabla afectada y registro
            builder.Entity<AuditoriaAcciones>()
                .HasIndex(a => new { a.TablaAfectada, a.RegistroAfectadoId })
                .HasDatabaseName("IX_AuditoriaAcciones_Tabla_Registro");

            // Índice para consultas por fecha (para reportes y limpieza)
            builder.Entity<AuditoriaAcciones>()
                .HasIndex(a => a.FechaHora)
                .HasDatabaseName("IX_AuditoriaAcciones_FechaHora");

            // Índice para consultas por severidad (para alertas críticas)
            builder.Entity<AuditoriaAcciones>()
                .HasIndex(a => a.Severidad)
                .HasDatabaseName("IX_AuditoriaAcciones_Severidad");

            // Índice para filtrar registros visibles
            builder.Entity<AuditoriaAcciones>()
                .HasIndex(a => a.EsVisible)
                .HasDatabaseName("IX_AuditoriaAcciones_EsVisible");

            // Configuración de conversión de enums a string para mejor legibilidad en BD
            builder.Entity<AuditoriaAcciones>()
                .Property(a => a.TipoAccion)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Entity<AuditoriaAcciones>()
                .Property(a => a.Modulo)
                .HasConversion<string>()
                .HasMaxLength(50);

            builder.Entity<AuditoriaAcciones>()
                .Property(a => a.Severidad)
                .HasConversion<string>()
                .HasMaxLength(20);
        }
    }
}