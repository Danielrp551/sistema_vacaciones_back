using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using sistema_vacaciones_back.Models;

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
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }

        public DbSet<RolPermiso> RolPermisos { get; set; }
        public DbSet<Vacaciones> Vacaciones { get; set; }
        public DbSet<SolicitudVacaciones> SolicitudesVacaciones { get; set; }
        public DbSet<Restriccion> Restricciones { get; set; }

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
        }
    }
}