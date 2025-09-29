using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncidentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInjuryAndDeathTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Deaths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentCasualtyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deaths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deaths_IncidentCasualties_IncidentCasualtyId",
                        column: x => x.IncidentCasualtyId,
                        principalTable: "IncidentCasualties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Injuries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentCasualtyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Injuries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Injuries_IncidentCasualties_IncidentCasualtyId",
                        column: x => x.IncidentCasualtyId,
                        principalTable: "IncidentCasualties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Deaths_IncidentCasualtyId",
                table: "Deaths",
                column: "IncidentCasualtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Injuries_IncidentCasualtyId",
                table: "Injuries",
                column: "IncidentCasualtyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Deaths");

            migrationBuilder.DropTable(
                name: "Injuries");
        }
    }
}
