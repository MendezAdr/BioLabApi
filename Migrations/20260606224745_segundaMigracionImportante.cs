using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BioLabApi.Migrations
{
    /// <inheritdoc />
    public partial class segundaMigracionImportante : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostoenBolivares",
                table: "Examenes");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Permisos", "RolName" },
                values: new object[,]
                {
                    { 1, 100, "Admin" },
                    { 2, 2, "Usuario" }
                });

            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "Id", "Apellido", "Cedula", "Contrasena", "CreadoPorId", "FechaCreacion", "FechaModificacion", "IsActive", "ModificadoPorId", "Nombre", "RolId", "Username" },
                values: new object[] { 1, "User", "00", "$2a$11$fjPIBjjjV8VbtMHH5kRQNuE2WtVVt7cnhujWwTOO6cG.KWUYZjC8O", 0, new DateTime(2026, 6, 6, 18, 47, 44, 360, DateTimeKind.Local).AddTicks(5341), null, true, null, "Admin", 1, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AddColumn<decimal>(
                name: "CostoenBolivares",
                table: "Examenes",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
