using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SISTEMA_VACACIONES.DTOs.Proveedor;
using SISTEMA_VACACIONES.Helpers;
using SISTEMA_VACACIONES.Models;

namespace SISTEMA_VACACIONES.Interfaces
{
    public interface IProveedorRepository
    {
        Task<List<Proveedor>> GetAllAsync(ProveedorQueryObject queryObject);
        Task<Proveedor?> GetByIdAsync(int id);
        Task<Proveedor?> CreateAsync(Proveedor proveedor);
        Task<Proveedor?> UpdateAsync(int id, UpdateProveedorRequestDto proveedor);
        Task<Proveedor?> DeleteAsync(int id);
    }
}