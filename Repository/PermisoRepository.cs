using Microsoft.EntityFrameworkCore;
using sistema_vacaciones_back.Data;
using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Interfaces;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Repository
{
    /// <summary>
    /// Implementación del repositorio de permisos
    /// </summary>
    public class PermisoRepository : IPermisoRepository
    {
        private readonly ApplicationDBContext _context;

        /// <summary>
        /// Módulos disponibles para el sistema
        /// </summary>
        private static readonly List<string> MODULOS_DISPONIBLES = new List<string>
        {
            "ADMINISTRACION",
            "GENERAL",
            "REPORTE EQUIPO"
        };

        public PermisoRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<PermisosAdminResponse> GetAllPermisosAsync(PermisosQueryObject query)
        {
            // Validar parámetros de paginación
            query.ValidatePagination();

            // Query base
            var permisosQuery = _context.Permisos
                .Where(p => !p.IsDeleted)
                .AsQueryable();

            // Aplicar filtros
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.ToLower();
                permisosQuery = permisosQuery.Where(p =>
                    p.Nombre.ToLower().Contains(searchTerm) ||
                    p.Descripcion.ToLower().Contains(searchTerm) ||
                    p.Modulo.ToLower().Contains(searchTerm) ||
                    p.CodigoPermiso.ToLower().Contains(searchTerm)
                );
            }

            if (!string.IsNullOrWhiteSpace(query.Modulo))
            {
                permisosQuery = permisosQuery.Where(p =>
                    p.Modulo.ToLower() == query.Modulo.ToLower()
                );
            }

            if (query.IsActive.HasValue)
            {
                permisosQuery = permisosQuery.Where(p => p.IsActive == query.IsActive.Value);
            }

            // Aplicar ordenamiento
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                permisosQuery = ApplySorting(permisosQuery, query.SortBy, query.IsDescending);
            }
            else
            {
                // Ordenamiento por defecto
                permisosQuery = permisosQuery.OrderBy(p => p.Modulo).ThenBy(p => p.Nombre);
            }

            // Contar total de registros
            var totalRecords = await permisosQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalRecords / query.PageSize);

            // Aplicar paginación
            var permisos = await permisosQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            // Obtener cantidad de roles para cada permiso
            var permisosConRoles = new List<PermisoAdminDto>();
            foreach (var permiso in permisos)
            {
                var numeroRoles = await GetNumeroRolesConPermisoAsync(permiso.Id);
                permisosConRoles.Add(new PermisoAdminDto
                {
                    Id = permiso.Id,
                    Nombre = permiso.Nombre,
                    Descripcion = permiso.Descripcion,
                    Modulo = permiso.Modulo,
                    CodigoPermiso = permiso.CodigoPermiso,
                    NumeroRoles = numeroRoles,
                    CreatedOn = permiso.CreatedOn,
                    CreatedBy = permiso.CreatedBy ?? "",
                    UpdatedOn = permiso.UpdatedOn,
                    UpdatedBy = permiso.UpdatedBy,
                    IsActive = permiso.IsActive
                });
            }

            // Obtener datos adicionales
            var modulosDisponibles = await GetModulosDisponiblesAsync();
            var estadisticas = await GetEstadisticasPermisosAsync();

            return new PermisosAdminResponse
            {
                Permisos = permisosConRoles,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = query.PageNumber,
                PageSize = query.PageSize,
                ModulosDisponibles = modulosDisponibles,
                Estadisticas = estadisticas
            };
        }

        public async Task<Permiso?> GetPermisoByIdAsync(string id)
        {
            return await _context.Permisos
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<Permiso?> GetPermisoByCodigoAsync(string codigoPermiso)
        {
            return await _context.Permisos
                .FirstOrDefaultAsync(p => p.CodigoPermiso == codigoPermiso && !p.IsDeleted);
        }

        public async Task<bool> ExistsPermisoByCodigoAsync(string codigoPermiso, string? excludeId = null)
        {
            var query = _context.Permisos.Where(p => 
                p.CodigoPermiso.ToLower() == codigoPermiso.ToLower() && !p.IsDeleted);

            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(p => p.Id != excludeId);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsPermisoByNombreAsync(string nombre, string? excludeId = null)
        {
            var query = _context.Permisos.Where(p => 
                p.Nombre.ToLower() == nombre.ToLower() && !p.IsDeleted);

            if (!string.IsNullOrEmpty(excludeId))
            {
                query = query.Where(p => p.Id != excludeId);
            }

            return await query.AnyAsync();
        }

        public async Task<Permiso> CreatePermisoAsync(Permiso permiso)
        {
            _context.Permisos.Add(permiso);
            await _context.SaveChangesAsync();
            return permiso;
        }

        public async Task<Permiso> UpdatePermisoAsync(Permiso permiso)
        {
            _context.Permisos.Update(permiso);
            await _context.SaveChangesAsync();
            return permiso;
        }

        public async Task<bool> DeletePermisoAsync(string id)
        {
            var permiso = await GetPermisoByIdAsync(id);
            if (permiso == null) return false;

            // Soft delete
            permiso.IsDeleted = true;
            permiso.DeletedOn = DateTime.UtcNow;
            
            _context.Permisos.Update(permiso);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<string>> GetModulosDisponiblesAsync()
        {
            // Retornar la lista de módulos desde la constante
            return await Task.FromResult(new List<string>(MODULOS_DISPONIBLES));
        }

        public async Task<PermisosEstadisticas> GetEstadisticasPermisosAsync()
        {
            var totalActivos = await _context.Permisos
                .CountAsync(p => !p.IsDeleted && p.IsActive);

            var totalInactivos = await _context.Permisos
                .CountAsync(p => !p.IsDeleted && !p.IsActive);

            var modulos = await _context.Permisos
                .Where(p => !p.IsDeleted)
                .Select(p => p.Modulo)
                .Distinct()
                .CountAsync();

            var promedioPermisosPorModulo = modulos > 0 
                ? Math.Round((decimal)(totalActivos + totalInactivos) / modulos, 2)
                : 0;

            return new PermisosEstadisticas
            {
                TotalPermisosActivos = totalActivos,
                TotalPermisosInactivos = totalInactivos,
                TotalModulos = modulos,
                PromedioPermisosPorModulo = promedioPermisosPorModulo
            };
        }

        public async Task<int> GetNumeroRolesConPermisoAsync(string permisoId)
        {
            return await _context.RolPermisos
                .Where(rp => rp.PermisoId == permisoId)
                .Select(rp => rp.RolId)
                .Distinct()
                .CountAsync();
        }

        public async Task<bool> CanDeletePermisoAsync(string permisoId)
        {
            var numeroRoles = await GetNumeroRolesConPermisoAsync(permisoId);
            return numeroRoles == 0;
        }

        private IQueryable<Permiso> ApplySorting(IQueryable<Permiso> query, string sortBy, bool isDescending)
        {
            return sortBy.ToLower() switch
            {
                "nombre" => isDescending 
                    ? query.OrderByDescending(p => p.Nombre)
                    : query.OrderBy(p => p.Nombre),
                "descripcion" => isDescending 
                    ? query.OrderByDescending(p => p.Descripcion)
                    : query.OrderBy(p => p.Descripcion),
                "modulo" => isDescending 
                    ? query.OrderByDescending(p => p.Modulo)
                    : query.OrderBy(p => p.Modulo),
                "codigopermiso" => isDescending 
                    ? query.OrderByDescending(p => p.CodigoPermiso)
                    : query.OrderBy(p => p.CodigoPermiso),
                "createdon" => isDescending 
                    ? query.OrderByDescending(p => p.CreatedOn)
                    : query.OrderBy(p => p.CreatedOn),
                "createdby" => isDescending 
                    ? query.OrderByDescending(p => p.CreatedBy)
                    : query.OrderBy(p => p.CreatedBy),
                "updatedon" => isDescending 
                    ? query.OrderByDescending(p => p.UpdatedOn)
                    : query.OrderBy(p => p.UpdatedOn),
                "isactive" => isDescending 
                    ? query.OrderByDescending(p => p.IsActive)
                    : query.OrderBy(p => p.IsActive),
                _ => query.OrderBy(p => p.Modulo).ThenBy(p => p.Nombre)
            };
        }
    }
}
