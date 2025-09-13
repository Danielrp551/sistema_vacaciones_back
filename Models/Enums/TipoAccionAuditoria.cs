using System.ComponentModel;

namespace sistema_vacaciones_back.Models.Enums
{
    /// <summary>
    /// Tipos de acciones que se pueden auditar en el sistema
    /// </summary>
    public enum TipoAccionAuditoria
    {
        // === GESTIÓN DE USUARIOS ===
        [Description("Usuario creado")]
        CREAR_USUARIO,

        [Description("Usuario editado")]
        EDITAR_USUARIO,

        [Description("Usuario eliminado")]
        ELIMINAR_USUARIO,

        [Description("Usuario activado")]
        ACTIVAR_USUARIO,

        [Description("Usuario desactivado")]
        DESACTIVAR_USUARIO,

        [Description("Contraseña reiniciada")]
        REINICIAR_PASSWORD,

        [Description("Rol cambiado")]
        CAMBIAR_ROL,

        // === GESTIÓN DE ROLES ===
        [Description("Rol creado")]
        CREAR_ROL,

        [Description("Rol editado")]
        EDITAR_ROL,

        [Description("Rol eliminado")]
        ELIMINAR_ROL,

        [Description("Rol activado")]
        ACTIVAR_ROL,

        [Description("Rol desactivado")]
        DESACTIVAR_ROL,

        [Description("Permisos de rol modificados")]
        MODIFICAR_PERMISOS_ROL,

        // === GESTIÓN DE PERMISOS ===
        [Description("Permiso creado")]
        CreacionPermiso,

        [Description("Permiso actualizado")]
        ActualizacionPermiso,

        [Description("Permiso eliminado")]
        EliminacionPermiso,

        // === SOLICITUDES DE VACACIONES ===
        [Description("Solicitud creada")]
        CREAR_SOLICITUD,

        [Description("Solicitud editada")]
        EDITAR_SOLICITUD,

        [Description("Solicitud aprobada")]
        APROBAR_SOLICITUD,

        [Description("Solicitud rechazada")]
        RECHAZAR_SOLICITUD,

        [Description("Solicitud cancelada")]
        CANCELAR_SOLICITUD,

        // === SALDOS DE VACACIONES ===
        [Description("Saldo ajustado")]
        AJUSTAR_SALDO,

        [Description("Saldo recalculado")]
        RECALCULAR_SALDO,

        // === ACCESOS Y SEGURIDAD ===
        [Description("Inicio de sesión")]
        LOGIN,

        [Description("Cierre de sesión")]
        LOGOUT,

        [Description("Acceso denegado")]
        ACCESO_DENEGADO,

        [Description("Intento de acceso no autorizado")]
        INTENTO_ACCESO_NO_AUTORIZADO,

        // === CONFIGURACIÓN ===
        [Description("Configuración modificada")]
        MODIFICAR_CONFIGURACION,

        // === REPORTES ===
        [Description("Reporte generado")]
        GENERAR_REPORTE,

        [Description("Reporte exportado")]
        EXPORTAR_REPORTE,

        // === BULK IMPORT ===
        [Description("Importación masiva exitosa")]
        BULK_IMPORT_SUCCESS,

        [Description("Error en importación masiva")]
        BULK_IMPORT_ERROR
    }
}
