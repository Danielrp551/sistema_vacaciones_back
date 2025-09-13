using sistema_vacaciones_back.DTOs.Permiso;
using sistema_vacaciones_back.Helpers;
using sistema_vacaciones_back.Models;

namespace sistema_vacaciones_back.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de permisos
    /// </summary>
    public interface IPermisoRepository
    {
        /// <summary>
        /// Obtiene todos los permisos con filtrado, búsqueda y paginación
        /// </summary>
        /// <param name="query">Parámetros de consulta y filtrado</param>
        /// <returns>Response paginado con permisos y estadísticas</returns>
        Task<PermisosAdminResponse> GetAllPermisosAsync(PermisosQueryObject query);

        /// <summary>
        /// Obtiene un permiso por su ID
        /// </summary>
        /// <param name="id">ID del permiso</param>
        /// <returns>Permiso encontrado o null</returns>
        Task<Permiso?> GetPermisoByIdAsync(string id);

        /// <summary>
        /// Obtiene un permiso por su código único
        /// </summary>
        /// <param name="codigoPermiso">Código único del permiso</param>
        /// <returns>Permiso encontrado o null</returns>
        Task<Permiso?> GetPermisoByCodigoAsync(string codigoPermiso);

        /// <summary>
        /// Verifica si existe un permiso con el código especificado
        /// </summary>
        /// <param name="codigoPermiso">Código a verificar</param>
        /// <param name="excludeId">ID a excluir de la verificación (para updates)</param>
        /// <returns>True si existe, false en caso contrario</returns>
        Task<bool> ExistsPermisoByCodigoAsync(string codigoPermiso, string? excludeId = null);

        /// <summary>
        /// Verifica si existe un permiso con el nombre especificado
        /// </summary>
        /// <param name="nombre">Nombre a verificar</param>
        /// <param name="excludeId">ID a excluir de la verificación (para updates)</param>
        /// <returns>True si existe, false en caso contrario</returns>
        Task<bool> ExistsPermisoByNombreAsync(string nombre, string? excludeId = null);

        /// <summary>
        /// Crea un nuevo permiso
        /// </summary>
        /// <param name="permiso">Permiso a crear</param>
        /// <returns>Permiso creado</returns>
        Task<Permiso> CreatePermisoAsync(Permiso permiso);

        /// <summary>
        /// Actualiza un permiso existente
        /// </summary>
        /// <param name="permiso">Permiso a actualizar</param>
        /// <returns>Permiso actualizado</returns>
        Task<Permiso> UpdatePermisoAsync(Permiso permiso);

        /// <summary>
        /// Elimina un permiso (soft delete)
        /// </summary>
        /// <param name="id">ID del permiso a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        Task<bool> DeletePermisoAsync(string id);

        /// <summary>
        /// Obtiene la lista de módulos únicos disponibles
        /// </summary>
        /// <returns>Lista de módulos</returns>
        Task<List<string>> GetModulosDisponiblesAsync();

        /// <summary>
        /// Obtiene estadísticas de permisos
        /// </summary>
        /// <returns>Estadísticas completas</returns>
        Task<PermisosEstadisticas> GetEstadisticasPermisosAsync();

        /// <summary>
        /// Obtiene la cantidad de roles que tienen asignado un permiso específico
        /// </summary>
        /// <param name="permisoId">ID del permiso</param>
        /// <returns>Cantidad de roles</returns>
        Task<int> GetNumeroRolesConPermisoAsync(string permisoId);

        /// <summary>
        /// Verifica si un permiso puede ser eliminado (no está asignado a ningún rol)
        /// </summary>
        /// <param name="permisoId">ID del permiso</param>
        /// <returns>True si puede eliminarse, false en caso contrario</returns>
        Task<bool> CanDeletePermisoAsync(string permisoId);
    }
}
