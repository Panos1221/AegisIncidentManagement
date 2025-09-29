using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IncidentManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentDetailEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IncidentCasualties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    InjuredFiremen = table.Column<int>(type: "int", nullable: true),
                    InjuredCivilians = table.Column<int>(type: "int", nullable: true),
                    InjuredPersonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InjuredPersonType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeadFiremen = table.Column<int>(type: "int", nullable: true),
                    DeadCivilians = table.Column<int>(type: "int", nullable: true),
                    DeadPersonName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DeadPersonType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "IncidentCommanders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    PersonnelId = table.Column<int>(type: "int", nullable: false),
                    Observations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCommanders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentCommanders_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IncidentCommanders_Personnel_PersonnelId",
                        column: x => x.PersonnelId,
                        principalTable: "Personnel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IncidentCommanders_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IncidentDamages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    OwnerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DamageAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SavedProperty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IncidentCause = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentDamages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentDamages_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentFires",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    BurnedArea = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BurnedItems = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentFires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentFires_Incidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncidentInvolvements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    FireTrucksNumber = table.Column<int>(type: "int", nullable: true),
                    FirePersonnel = table.Column<int>(type: "int", nullable: true),
                    OtherAgencies = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ServiceActions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RescuedPeople = table.Column<int>(type: "int", nullable: true),
                    RescueInformation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentInvolvements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentInvolvements_Incidents_IncidentId",
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

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCommanders_AssignedByUserId",
                table: "IncidentCommanders",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCommanders_IncidentId",
                table: "IncidentCommanders",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCommanders_PersonnelId",
                table: "IncidentCommanders",
                column: "PersonnelId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentDamages_IncidentId",
                table: "IncidentDamages",
                column: "IncidentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentFires_IncidentId",
                table: "IncidentFires",
                column: "IncidentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentInvolvements_IncidentId",
                table: "IncidentInvolvements",
                column: "IncidentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentCasualties");

            migrationBuilder.DropTable(
                name: "IncidentCommanders");

            migrationBuilder.DropTable(
                name: "IncidentDamages");

            migrationBuilder.DropTable(
                name: "IncidentFires");

            migrationBuilder.DropTable(
                name: "IncidentInvolvements");
        }
    }
}
