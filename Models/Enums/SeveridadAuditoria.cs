using System.ComponentModel;

namespace sistema_vacaciones_back.Models.Enums
{
    /// <summary>
    /// Nivel de severidad de las acciones auditadas
    /// </summary>
    public enum SeveridadAuditoria
    {
        [Description("Información general")]
        INFO,

        [Description("Advertencia")]
        WARNING,

        [Description("Acción crítica")]
        CRITICAL,

        [Description("Error del sistema")]
        ERROR,

        [Description("Evento de seguridad")]
        SECURITY
    }
}
