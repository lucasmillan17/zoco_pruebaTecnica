using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ComerciosCuitUnicoSoloActivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comercios_Cuit",
                table: "Comercios");

            migrationBuilder.CreateIndex(
                name: "IX_Comercios_Cuit",
                table: "Comercios",
                column: "Cuit",
                unique: true,
                filter: "\"Activo\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comercios_Cuit",
                table: "Comercios");

            migrationBuilder.CreateIndex(
                name: "IX_Comercios_Cuit",
                table: "Comercios",
                column: "Cuit",
                unique: true);
        }
    }
}
