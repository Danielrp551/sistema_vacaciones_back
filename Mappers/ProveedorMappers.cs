using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SISTEMA_VACACIONES.DTOs.Proveedor;
using SISTEMA_VACACIONES.Models;

namespace SISTEMA_VACACIONES.Mappers
{
    public static class ProveedorMappers
    {
        public static ProveedorDto ToProveedorDto(this Proveedor proveedor)
        {
            return new ProveedorDto
            {
                Id = proveedor.Id,
                RazonSocial = proveedor.RazonSocial,
                NombreComercial = proveedor.NombreComercial,
                IdentificacionTributaria = proveedor.IdentificacionTributaria,
                NumeroTelefonico = proveedor.NumeroTelefonico,
                CorreoElectronico = proveedor.CorreoElectronico,
                SitioWeb = proveedor.SitioWeb,
                DireccionFisica = proveedor.DireccionFisica,
                Pais = proveedor.Pais,
                FacturacionAnualUSD = proveedor.FacturacionAnualUSD,
                FechaCreacion = proveedor.FechaCreacion,
                FechaUltimaEdicion = proveedor.FechaUltimaEdicion
            };
        }

        public static Proveedor ToProveedorFromCreateDto(this CreateProveedorRequestDto createProveedorRequestDto)
        {
            return new Proveedor
            {
                RazonSocial = createProveedorRequestDto.RazonSocial,
                NombreComercial = createProveedorRequestDto.NombreComercial,
                IdentificacionTributaria = createProveedorRequestDto.IdentificacionTributaria,
                NumeroTelefonico = createProveedorRequestDto.NumeroTelefonico,
                CorreoElectronico = createProveedorRequestDto.CorreoElectronico,
                SitioWeb = createProveedorRequestDto.SitioWeb,
                DireccionFisica = createProveedorRequestDto.DireccionFisica,
                Pais = createProveedorRequestDto.Pais,
                FacturacionAnualUSD = createProveedorRequestDto.FacturacionAnualUSD,
                FechaCreacion = DateTime.Now,
                FechaUltimaEdicion = DateTime.Now
            };
        }
        
    }
}