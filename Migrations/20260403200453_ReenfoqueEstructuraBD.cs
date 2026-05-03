using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BioLabProject.Migrations
{
    /// <inheritdoc />
    public partial class ReenfoqueEstructuraBD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tasas");

            migrationBuilder.DropColumn(
                name: "Moneda",
                table: "Pagos");

            migrationBuilder.DropColumn(
                name: "Metodo_pago",
                table: "Detalles");

            migrationBuilder.DropColumn(
                name: "PrecioVentaBolivares",
                table: "Detalles");

            migrationBuilder.RenameColumn(
                name: "Total",
                table: "Ordenes",
                newName: "TotalDivisa");

            migrationBuilder.RenameColumn(
                name: "NumeroControl",
                table: "Ordenes",
                newName: "TasaBcv");

            migrationBuilder.RenameColumn(
                name: "Estado_Pago",
                table: "Ordenes",
                newName: "EstadoPago");

            migrationBuilder.RenameColumn(
                name: "Nombre_Examen",
                table: "Examenes",
                newName: "NombreExamen");

            migrationBuilder.RenameColumn(
                name: "Costo_en_Divisa",
                table: "Examenes",
                newName: "CostoenBolivares");

            migrationBuilder.RenameColumn(
                name: "Costo_en_Bolivares",
                table: "Examenes",
                newName: "CostoEnDivisa");

            migrationBuilder.RenameColumn(
                name: "PrecioVentaDivisa",
                table: "Detalles",
                newName: "PrecioMomentoDivisa");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalDivisa",
                table: "Ordenes",
                newName: "Total");

            migrationBuilder.RenameColumn(
                name: "TasaBcv",
                table: "Ordenes",
                newName: "NumeroControl");

            migrationBuilder.RenameColumn(
                name: "EstadoPago",
                table: "Ordenes",
                newName: "Estado_Pago");

            migrationBuilder.RenameColumn(
                name: "NombreExamen",
                table: "Examenes",
                newName: "Nombre_Examen");

            migrationBuilder.RenameColumn(
                name: "CostoenBolivares",
                table: "Examenes",
                newName: "Costo_en_Divisa");

            migrationBuilder.RenameColumn(
                name: "CostoEnDivisa",
                table: "Examenes",
                newName: "Costo_en_Bolivares");

            migrationBuilder.RenameColumn(
                name: "PrecioMomentoDivisa",
                table: "Detalles",
                newName: "PrecioVentaDivisa");

            migrationBuilder.AddColumn<int>(
                name: "Moneda",
                table: "Pagos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Metodo_pago",
                table: "Detalles",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVentaBolivares",
                table: "Detalles",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Tasas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FechaActualizacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Valor = table.Column<decimal>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasas", x => x.Id);
                });
        }
    }
}
