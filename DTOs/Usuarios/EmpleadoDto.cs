namespace sistema_vacaciones_back.DTOs.Usuarios
{
    public class EmpleadoDto
    {
        public string Id { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool EsDirecto { get; set; }
    }
}
