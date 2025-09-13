using System.ComponentModel;

namespace sistema_vacaciones_back.Models.Enums
{
    /// <summary>
    /// Módulos o secciones del sistema para categorizar las acciones de auditoría
    /// </summary>
    public enum ModuloSistema
    {
        [Description("Gestión de Usuarios")]
        GESTION_USUARIOS,

        [Description("Solicitudes de Vacaciones")]
        SOLICITUDES_VACACIONES,

        [Description("Historial de Vacaciones")]
        HISTORIAL_VACACIONES,

        [Description("Saldos de Vacaciones")]
        SALDOS_VACACIONES,

        [Description("Gestión de Roles")]
        GESTION_ROLES,

        [Description("Gestión de Permisos")]
        GESTION_PERMISOS,

        [Description("Configuración del Sistema")]
        CONFIGURACION_SISTEMA,

        [Description("Reportes y Analytics")]
        REPORTES,

        [Description("Seguridad y Accesos")]
        SEGURIDAD,

        [Description("Dashboard Principal")]
        DASHBOARD,

        [Description("Administración General")]
        ADMINISTRACION
    }
}
