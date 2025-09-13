using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SISTEMA_VACACIONES.Migrations
{
    /// <inheritdoc />
    public partial class AddPermisoSoftDeleteProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                table: "Permisos",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "Permisos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Permisos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Permisos",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "Permisos");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "Permisos");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Permisos");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Permisos");
        }
    }
}
