using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SISTEMA_VACACIONES.Migrations
{
    /// <inheritdoc />
    public partial class AuditoriaAcciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditoriaAcciones",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TipoAccion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TablaAfectada = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RegistroAfectadoId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioEjecutorId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UsuarioEjecutorNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UsuarioEjecutorEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UsuarioAfectadoId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UsuarioAfectadoNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UsuarioAfectadoEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MensajeCorto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MensajeDetallado = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MensajePlantilla = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ValoresAnteriores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValoresNuevos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Severidad = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EsVisible = table.Column<bool>(type: "bit", nullable: false),
                    Tags = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SessionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TiempoEjecucionMs = table.Column<int>(type: "int", nullable: true),
                    MetadatosExtras = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditoriaAcciones", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_EsVisible",
                table: "AuditoriaAcciones",
                column: "EsVisible");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_FechaHora",
                table: "AuditoriaAcciones",
                column: "FechaHora");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_Modulo_FechaHora",
                table: "AuditoriaAcciones",
                columns: new[] { "Modulo", "FechaHora" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_Severidad",
                table: "AuditoriaAcciones",
                column: "Severidad");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_Tabla_Registro",
                table: "AuditoriaAcciones",
                columns: new[] { "TablaAfectada", "RegistroAfectadoId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_TipoAccion",
                table: "AuditoriaAcciones",
                column: "TipoAccion");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_UsuarioAfectado",
                table: "AuditoriaAcciones",
                column: "UsuarioAfectadoId",
                filter: "[UsuarioAfectadoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditoriaAcciones_UsuarioEjecutor",
                table: "AuditoriaAcciones",
                column: "UsuarioEjecutorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditoriaAcciones");
        }
    }
}
