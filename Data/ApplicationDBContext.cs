using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SISTEMA_VACACIONES.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using sistema_vacaciones_back.Models;

namespace SISTEMA_VACACIONES.Data
{
    public class ApplicationDBContext : IdentityDbContext<Usuario>
    {
        public ApplicationDBContext(DbContextOptions dbContextOptions) : base(dbContextOptions)
        {

        }

        //TABLAS 
        public DbSet<Proveedor> Proveedores { get; set; }
        // VACACIONES
        public DbSet<Persona> Personas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UsuarioRol> UsuarioRoles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Vacaciones> Vacaciones { get; set; }
        public DbSet<SolicitudVacaciones> SolicitudesVacaciones { get; set; }
        public DbSet<AprobacionVacaciones> AprobacionesVacaciones { get; set; }
        public DbSet<Restriccion> Restricciones { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UsuarioRol>()
                .HasIndex(ur => new { ur.UsuarioId, ur.RolId })
                .IsUnique();

            builder.Entity<RolPermiso>()
                .HasIndex(rp => new { rp.RolId, rp.PermisoId })
                .IsUnique();


            string roleId = "032feba5-28c3-4dbe-a7d0-260767036d04";  // 🔹 Generar GUID en vez de int

            builder.Entity<Rol>().HasData(new Rol
            {
                Id = roleId,  // 🔹 Ahora es string
                Name = "User",
                NormalizedName = "USER",
                Descripcion = "Rol por defecto",
                NumeroPersonas = 0,
                Estado = "activo",
                CreatedBy = "Sistema",
                CreatedOn = new DateTime(2025, 2, 22, 0, 0, 0, DateTimeKind.Utc),
                UpdatedBy = "Sistema",
                UpdatedOn = new DateTime(2025, 2, 22, 18, 30, 0, DateTimeKind.Utc),
                isDeleted = false
            });

            // 📌 Insertar Permisos por defecto
            builder.Entity<Permiso>().HasData(
                new Permiso
                {
                    Id = 1,
                    NombreRuta = "/vacaciones",
                    Descripcion = ""
                },
                new Permiso
                {
                    Id = 2,
                    NombreRuta = "/historial-vacaciones",
                    Descripcion = ""
                },
                new Permiso
                {
                    Id = 3,
                    NombreRuta = "/mis-solicitudes-vacaciones",
                    Descripcion = ""
                },
                new Permiso
                {
                    Id = 4,
                    NombreRuta = "/aprobaciones",
                    Descripcion = ""
                },
                new Permiso
                {
                    Id = 5,
                    NombreRuta = "/administracion",
                    Descripcion = ""
                },
                new Permiso
                {
                    Id = 6,
                    NombreRuta = "/roles-permisos",
                    Descripcion = ""
                },
                new Permiso
                {
                    Id = 7,
                    NombreRuta = "/usuarios-jerarquias",
                    Descripcion = ""
                }
            );

            // 📌 Relación entre el Rol "User" y sus permisos
            builder.Entity<RolPermiso>().HasData(
                new RolPermiso
                {
                    Id = 1,
                    RolId = roleId,  // 🔹 Ahora es string
                    PermisoId = 1
                },
                new RolPermiso
                {
                    Id = 2,
                    RolId = roleId,  // 🔹 Ahora es string
                    PermisoId = 2
                },
                new RolPermiso
                {
                    Id = 3,
                    RolId = roleId,  // 🔹 Ahora es string
                    PermisoId = 3
                },
                new RolPermiso
                {
                    Id = 4,
                    RolId = roleId,  // 🔹 Ahora es string
                    PermisoId = 4
                },
                new RolPermiso
                {
                    Id = 5,
                    RolId = roleId,  // 🔹 Ahora es string
                    PermisoId = 5
                },
                new RolPermiso
                {
                    Id = 6,
                    RolId = roleId,  // 🔹 Ahora es string
                    PermisoId = 6
                },
                new RolPermiso
                {
                    Id = 7,
                    RolId = roleId,  // 🔹 Ahora es string
                    PermisoId = 7
                }
            );

            builder.Entity<AprobacionVacaciones>()
                .HasOne(av => av.Solicitud)
                .WithMany()
                .HasForeignKey(av => av.SolicitudId)
                .OnDelete(DeleteBehavior.NoAction);  // ⬅ Desactiva la eliminación en cascada

            builder.Entity<AprobacionVacaciones>()
                .HasOne(av => av.Aprobador)
                .WithMany()
                .HasForeignKey(av => av.AprobadorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Usuario>()
                .HasOne(u => u.Jefe)
                .WithMany(j => j.Subordinados)
                .HasForeignKey(u => u.JefeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}