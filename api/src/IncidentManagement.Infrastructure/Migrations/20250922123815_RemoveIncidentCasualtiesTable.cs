using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncidentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIncidentCasualtiesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deaths_IncidentCasualties_IncidentCasualtyId",
                table: "Deaths");

            migrationBuilder.DropForeignKey(
                name: "FK_Injuries_IncidentCasualties_IncidentCasualtyId",
                table: "Injuries");

            migrationBuilder.DropTable(
                name: "IncidentCasualties");

            migrationBuilder.RenameColumn(
                name: "IncidentCasualtyId",
                table: "Injuries",
                newName: "IncidentId");

            migrationBuilder.RenameIndex(
                name: "IX_Injuries_IncidentCasualtyId",
                table: "Injuries",
                newName: "IX_Injuries_IncidentId");

            migrationBuilder.RenameColumn(
                name: "IncidentCasualtyId",
                table: "Deaths",
                newName: "IncidentId");

            migrationBuilder.RenameIndex(
                name: "IX_Deaths_IncidentCasualtyId",
                table: "Deaths",
                newName: "IX_Deaths_IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Deaths_Incidents_IncidentId",
                table: "Deaths",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Injuries_Incidents_IncidentId",
                table: "Injuries",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Deaths_Incidents_IncidentId",
                table: "Deaths");

            migrationBuilder.DropForeignKey(
                name: "FK_Injuries_Incidents_IncidentId",
                table: "Injuries");

            migrationBuilder.RenameColumn(
                name: "IncidentId",
                table: "Injuries",
                newName: "IncidentCasualtyId");

            migrationBuilder.RenameIndex(
                name: "IX_Injuries_IncidentId",
                table: "Injuries",
                newName: "IX_Injuries_IncidentCasualtyId");

            migrationBuilder.RenameColumn(
                name: "IncidentId",
                table: "Deaths",
                newName: "IncidentCasualtyId");

            migrationBuilder.RenameIndex(
                name: "IX_Deaths_IncidentId",
                table: "Deaths",
                newName: "IX_Deaths_IncidentCasualtyId");

            migrationBuilder.CreateTable(
                name: "IncidentCasualties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeadCivilians = table.Column<int>(type: "int", nullable: true),
                    DeadFiremen = table.Column<int>(type: "int", nullable: true),
                    DeadPersonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeadPersonType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InjuredCivilians = table.Column<int>(type: "int", nullable: true),
                    InjuredFiremen = table.Column<int>(type: "int", nullable: true),
                    InjuredPersonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InjuredPersonType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCasualties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentCasualties_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCasualties_IncidentId",
                table: "IncidentCasualties",
                column: "IncidentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Deaths_IncidentCasualties_IncidentCasualtyId",
                table: "Deaths",
                column: "IncidentCasualtyId",
                principalTable: "IncidentCasualties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Injuries_IncidentCasualties_IncidentCasualtyId",
                table: "Injuries",
                column: "IncidentCasualtyId",
                principalTable: "IncidentCasualties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
