using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Comercios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RazonSocial = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cuit = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: false),
                    NombreDelContacto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Direccion = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Rubro = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FechaDeCreacionEmpresa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Notas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comercios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposInteraccion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposInteraccion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Interacciones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComercioId = table.Column<Guid>(type: "uuid", nullable: false),
                    FechaInteraccion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipoInteraccionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interacciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interacciones_Comercios_ComercioId",
                        column: x => x.ComercioId,
                        principalTable: "Comercios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Interacciones_TiposInteraccion_TipoInteraccionId",
                        column: x => x.TipoInteraccionId,
                        principalTable: "TiposInteraccion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TiposInteraccion",
                columns: new[] { "Id", "Activo", "Codigo", "CreatedAt", "Descripcion", "Nombre", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111101"), true, "llamada", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Llamada telefónica con el contacto.", "Llamada", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-111111111102"), true, "whatsapp", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Conversación por WhatsApp.", "WhatsApp", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-111111111103"), true, "reunion", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reunión presencial o virtual con el comercio.", "Reunión", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-111111111104"), true, "email", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Correo electrónico enviado o recibido.", "Email", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-111111111105"), true, "nota_interna", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nota interna del equipo de ventas.", "Nota interna", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-111111111106"), true, "demo", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Demostración del producto (POS, QR, etc.).", "Demo", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-111111111107"), true, "visita", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Visita presencial al local del comercio.", "Visita", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-111111111108"), true, "videollamada", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Videollamada (Meet, Zoom, etc.).", "Videollamada", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-111111111109"), true, "envio_propuesta", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Envío de propuesta comercial o cotización.", "Envío de propuesta", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-11111111110a"), true, "seguimiento", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Follow-up o recordatorio de contacto.", "Seguimiento", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-11111111110b"), true, "queja", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reclamo o problema reportado por el comercio.", "Queja / problema", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-11111111110c"), true, "firma_contrato", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Firma de contrato o acuerdo.", "Firma de contrato", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("11111111-1111-1111-1111-11111111110d"), true, "nota_sistema", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Evento generado automáticamente por el sistema.", "Nota de sistema", new DateTime(2026, 8, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comercios_Cuit",
                table: "Comercios",
                column: "Cuit",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interacciones_ComercioId",
                table: "Interacciones",
                column: "ComercioId");

            migrationBuilder.CreateIndex(
                name: "IX_Interacciones_TipoInteraccionId",
                table: "Interacciones",
                column: "TipoInteraccionId");

            migrationBuilder.CreateIndex(
                name: "IX_TiposInteraccion_Codigo",
                table: "TiposInteraccion",
                column: "Codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interacciones");

            migrationBuilder.DropTable(
                name: "Comercios");

            migrationBuilder.DropTable(
                name: "TiposInteraccion");
        }
    }
}
