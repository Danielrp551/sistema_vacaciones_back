# Documentación API - Gestión Completa de Solicitudes de Vacaciones

## 📋 **Endpoints Implementados**

### **1. 📝 Crear Solicitud de Vacaciones**
```http
POST /api/solicitud-vacaciones/crear-solicitud-vacaciones
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "tipoVacaciones": "libres", // "libres" o "bloque"
  "diasSolicitados": 5,
  "fechaInicio": "2025-08-15",
  "fechaFin": "2025-08-19",
  "periodo": 2025
}
```

**Respuesta Exitosa:**
```json
{
  "message": "Solicitud creada exitosamente",
  "solicitudId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
}
```

---

### **2. 📋 Listar Mis Solicitudes de Vacaciones**
```http
GET /api/solicitud-vacaciones/mis-solicitudes?pageNumber=1&pageSize=10&sortBy=fechaSolicitud&isDescending=true
Authorization: Bearer {token}
```

**Parámetros de Query:**
- `pageNumber`: Número de página (default: 1)
- `pageSize`: Tamaño de página (default: 10)
- `sortBy`: Campo para ordenar (opcional)
- `isDescending`: Orden descendente (default: true)

**Respuesta Exitosa:**
```json
{
  "total": 15,
  "solicitudes": [
    {
      "id": 123456789,
      "usuarioId": "user-guid",
      "tipoVacaciones": "libres",
      "diasSolicitados": 5,
      "fechaInicio": "2025-08-15T00:00:00Z",
      "fechaFin": "2025-08-19T00:00:00Z",
      "estado": "pendiente",
      "fechaSolicitud": "2025-08-06T10:30:00Z",
      "periodo": 2025,
      "diasFinde": 2
    }
  ],
  "usuario": "user-guid",
  "pagina": 1,
  "tamanoPagina": 10
}
```

---

### **3. 🔍 Obtener Detalle de Solicitud**
```http
GET /api/solicitud-vacaciones/{solicitudId}
Authorization: Bearer {token}
```

**Respuesta Exitosa:**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "usuarioId": "user-guid",
  "nombreSolicitante": "Juan Pérez",
  "emailSolicitante": "juan.perez@empresa.com",
  "aprobadorId": null,
  "nombreAprobador": null,
  "tipoVacaciones": "libres",
  "diasSolicitados": 5,
  "fechaInicio": "2025-08-15T00:00:00Z",
  "fechaFin": "2025-08-19T00:00:00Z",
  "estado": "pendiente",
  "fechaSolicitud": "2025-08-06T10:30:00Z",
  "periodo": 2025,
  "diasFinde": 2,
  "fechaAprobacion": null,
  "comentarios": "",
  "puedeCancelar": true,
  "puedeAprobar": false
}
```

**Lógica de Permisos:**
- `puedeCancelar`: Solo el solicitante, solo si está pendiente y no ha comenzado
- `puedeAprobar`: Solo administradores/jefes, no propias solicitudes

---

### **4. ❌ Cancelar Solicitud**
```http
PUT /api/solicitud-vacaciones/{solicitudId}/cancelar
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "motivoCancelacion": "Cambio de planes personales"
}
```

**Respuesta Exitosa:**
```json
{
  "message": "Solicitud cancelada exitosamente"
}
```

**Validaciones:**
- ✅ Solo el propietario puede cancelar
- ✅ Solo solicitudes pendientes
- ✅ No se puede cancelar si ya comenzó

---

### **5. ✅ Aprobar/Rechazar Solicitud (Admin/Jefe)**
```http
PUT /api/solicitud-vacaciones/{solicitudId}/aprobar
Authorization: Bearer {token}
Content-Type: application/json
```

**Body:**
```json
{
  "accion": "aprobar", // "aprobar" o "rechazar"
  "comentarios": "Aprobado según disponibilidad del equipo"
}
```

**Respuesta Exitosa:**
```json
{
  "message": "Solicitud aprobada exitosamente"
}
```

**Validaciones:**
- ✅ Solo administradores o jefes directos
- ✅ No puede aprobar sus propias solicitudes
- ✅ Solo solicitudes pendientes

---

### **6. 📊 Listar Solicitudes con Paginación (Existente)**
```http
GET /api/solicitud-vacaciones/get-solicitudes-pagination?pageNumber=1&pageSize=10
Authorization: Bearer {token}
```

---

## 🔒 **Seguridad Implementada**

### **Atributos de Autorización**
- `[OwnerOnly]`: Solo recursos propios del usuario autenticado
- `[AdminOnly]`: Solo administradores y jefes pueden acceder

### **Validaciones de Negocio**
- **Cancelación**: Solo el propietario, solo pendientes, no iniciadas
- **Aprobación**: Solo admin/jefe, no propias, solo pendientes
- **Detalle**: Propietario o admin/jefe con permisos

### **Logging de Auditoría**
- Todas las operaciones registradas con userId y timestamps
- Errores detallados para debugging
- Intentos de acceso no autorizado monitoreados

---

## 📝 **DTOs Utilizados**

### **CreateSolicitudRequestDto**
```csharp
public class CreateSolicitudRequestDto
{
    [Required] public string TipoVacaciones { get; set; } // "libres" o "bloque"
    [Required] public int DiasSolicitados { get; set; }
    [Required] public DateTime FechaInicio { get; set; }
    [Required] public DateTime FechaFin { get; set; }
    [Required] public int Periodo { get; set; }
}
```

### **SolicitudVacacionesDetailDto**
```csharp
public class SolicitudVacacionesDetailDto
{
    public string Id { get; set; }
    public string NombreSolicitante { get; set; }
    public string EmailSolicitante { get; set; }
    public string? NombreAprobador { get; set; }
    public string TipoVacaciones { get; set; }
    public int DiasSolicitados { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; }
    public DateTime FechaSolicitud { get; set; }
    public int Periodo { get; set; }
    public int DiasFinde { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string Comentarios { get; set; }
    public bool PuedeCancelar { get; set; }
    public bool PuedeAprobar { get; set; }
}
```

### **AprobarSolicitudRequestDto**
```csharp
public class AprobarSolicitudRequestDto
{
    [Required]
    [RegularExpression("^(aprobar|rechazar)$")]
    public string Accion { get; set; }
    
    [MaxLength(1000)]
    public string? Comentarios { get; set; }
}
```

### **CancelarSolicitudRequestDto**
```csharp
public class CancelarSolicitudRequestDto
{
    [MaxLength(500)]
    public string? MotivoCancelacion { get; set; }
}
```

---

## 🚀 **Ejemplos de Uso**

### **Frontend - Listar mis solicitudes**
```javascript
async function getMisSolicitudes(pagina = 1) {
  const response = await fetch('/api/solicitud-vacaciones/mis-solicitudes?pageNumber=' + pagina, {
    headers: {
      'Authorization': 'Bearer ' + token,
      'Content-Type': 'application/json'
    }
  });
  
  const data = await response.json();
  return data.solicitudes;
}
```

### **Frontend - Cancelar solicitud**
```javascript
async function cancelarSolicitud(solicitudId, motivo) {
  const response = await fetch(`/api/solicitud-vacaciones/${solicitudId}/cancelar`, {
    method: 'PUT',
    headers: {
      'Authorization': 'Bearer ' + token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      motivoCancelacion: motivo
    })
  });
  
  if (response.ok) {
    alert('Solicitud cancelada exitosamente');
  }
}
```

### **Frontend - Aprobar solicitud (Admin)**
```javascript
async function aprobarSolicitud(solicitudId, accion, comentarios) {
  const response = await fetch(`/api/solicitud-vacaciones/${solicitudId}/aprobar`, {
    method: 'PUT',
    headers: {
      'Authorization': 'Bearer ' + token,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      accion: accion, // "aprobar" o "rechazar"
      comentarios: comentarios
    })
  });
  
  if (response.ok) {
    const data = await response.json();
    alert(data.message);
  }
}
```

---

## ✅ **Estados de Solicitud**

| Estado | Descripción | Acciones Disponibles |
|--------|-------------|---------------------|
| `pendiente` | Recién creada, esperando aprobación | Cancelar (usuario), Aprobar/Rechazar (admin) |
| `aprobada` | Aprobada por administrador/jefe | Ver detalle únicamente |
| `rechazada` | Rechazada por administrador/jefe | Ver detalle únicamente |
| `cancelado` | Cancelada por el usuario | Ver detalle únicamente |

---

## 🔧 **Mejores Prácticas Implementadas**

### **1. Seguridad**
- ✅ Autenticación JWT obligatoria
- ✅ Autorización basada en roles
- ✅ Validación de propiedad de recursos
- ✅ Prevención de manipulación de IDs

### **2. Validación de Datos**
- ✅ Data Annotations en DTOs
- ✅ Validaciones de negocio en Repository
- ✅ Manejo de errores descriptivos

### **3. Logging y Auditoría**
- ✅ Registro de todas las operaciones
- ✅ Información de usuario en logs
- ✅ Errores detallados para debugging

### **4. Arquitectura**
- ✅ Separación de responsabilidades
- ✅ DTOs específicos para cada operación
- ✅ Mappers robustos con validaciones
- ✅ Repository pattern con transacciones

### **5. Performance**
- ✅ Paginación en listados
- ✅ Queries optimizadas con Include
- ✅ Validaciones tempranas

La API está completamente funcional y lista para uso en producción! 🎉
