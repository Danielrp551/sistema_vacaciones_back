using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SISTEMA_VACACIONES.DTOs.Proveedor
{
    public class ProveedorDto
    {
        public int Id { get; set; } // Identificador único (numérico)
        public string RazonSocial { get; set; } = string.Empty; // Razón social o Nombre comercial (alfanumérico)
        public string NombreComercial { get; set; } = string.Empty;// Razón social o Nombre comercial (alfanumérico)
        public string IdentificacionTributaria { get; set; } = string.Empty;// Identificación tributaria (numérico - 11 dígitos)
        public string NumeroTelefonico { get; set; } = string.Empty;// Número telefónico (tipo teléfono)
        public string CorreoElectronico { get; set; } = string.Empty;// Correo electrónico (tipo correo)
        public string SitioWeb { get; set; } = string.Empty;// Sitio web (enlace)
        public string DireccionFisica { get; set; } = string.Empty; // Dirección física (alfanumérico)
        public string Pais { get; set; } = string.Empty; // País (alfanumérico - desplegable de opciones)
        public decimal FacturacionAnualUSD { get; set; } // Facturación anual en dólares (numérico)
        public DateTime FechaCreacion { get; set; } = DateTime.Now; // Fecha de creación (fecha y hora)
        public DateTime FechaUltimaEdicion { get; set; }  = DateTime.Now; // Fecha de última edición (fecha y hora)         
    }
}