using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SISTEMA_VACACIONES.Migrations
{
    /// <inheritdoc />
    public partial class AuditfieldPermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Permisos",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOn",
                table: "Permisos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "UpdatedBy", "UpdatedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "UpdatedBy", "UpdatedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "UpdatedBy", "UpdatedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "UpdatedBy", "UpdatedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "UpdatedBy", "UpdatedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "UpdatedBy", "UpdatedOn" },
                values: new object[] { null, null });

            migrationBuilder.UpdateData(
                table: "Permisos",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "UpdatedBy", "UpdatedOn" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Permisos");

            migrationBuilder.DropColumn(
                name: "UpdatedOn",
                table: "Permisos");
        }
    }
}
