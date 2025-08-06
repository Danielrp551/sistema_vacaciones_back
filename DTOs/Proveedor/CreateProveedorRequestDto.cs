using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SISTEMA_VACACIONES.DTOs.Proveedor
{
    public class CreateProveedorRequestDto
    {
        [Required(ErrorMessage = "La razón social es obligatoria")]
        [MaxLength(150, ErrorMessage = "La razón social no puede tener más de 150 caracteres")]
        [MinLength(5, ErrorMessage = "La razón social debe tener al menos 5 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\s.-]+$", ErrorMessage = "La razón social solo puede contener letras, números, espacios, puntos y guiones")]
        public string RazonSocial { get; set; } = string.Empty; // Razón social (alfanumérico)

        [Required(ErrorMessage = "El nombre comercial es obligatorio")]
        [MaxLength(150, ErrorMessage = "El nombre comercial no puede tener más de 150 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\s.-]*$", ErrorMessage = "El nombre comercial solo puede contener letras, números, espacios, puntos y guiones")]
        public string NombreComercial { get; set; } = string.Empty;// Nombre comercial (alfanumérico)
        
        [Required(ErrorMessage = "La identificación tributaria es obligatoria")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "La identificación tributaria debe tener exactamente 11 dígitos numéricos")]
        public string IdentificacionTributaria { get; set; } = string.Empty;// Identificación tributaria (numérico - 11 dígitos)
        
        [Required(ErrorMessage = "El número telefónico es obligatorio")]
        [Phone(ErrorMessage = "El número telefónico no tiene un formato válido")]
        public string NumeroTelefonico { get; set; } = string.Empty;// Número telefónico (tipo teléfono)
        
        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico es inválido")]
        public string CorreoElectronico { get; set; } = string.Empty;// Correo electrónico (tipo correo)
        
        [Required(ErrorMessage = "El sitio web es obligatorio")]
        [Url(ErrorMessage = "El sitio web debe ser una URL válida")]
        public string SitioWeb { get; set; } = string.Empty;// Sitio web (enlace)

        [Required(ErrorMessage = "La dirección física es obligatoria")]
        [MaxLength(255, ErrorMessage = "La dirección física no puede tener más de 255 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9\s.,#-]+$", ErrorMessage = "La dirección física solo puede contener letras, números, espacios y símbolos como ., # y -")]
        public string DireccionFisica { get; set; } = string.Empty; // Dirección física (alfanumérico)
        
        [Required(ErrorMessage = "El país es obligatorio")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "El país debe ser un código alfanumérico válido (por ejemplo: PE, US, MX)")]
        public string Pais { get; set; } = string.Empty; // País (alfanumérico - desplegable de opciones)
        
        [Required(ErrorMessage = "La facturación anual es obligatoria")]
        [Range(0, double.MaxValue, ErrorMessage = "La facturación anual debe ser un número positivo")]
        public decimal FacturacionAnualUSD { get; set; } // Facturación anual en dólares (numérico)     
    }
}