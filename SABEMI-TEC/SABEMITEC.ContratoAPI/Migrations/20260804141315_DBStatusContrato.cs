using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SABEMITEC.ContratoAPI.Migrations
{
    public partial class DBStatusContrato : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StatusContrato",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdTransacao = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IdContrato = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Falha = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataProcessamento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusContrato", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatusContrato");
        }
    }
}
