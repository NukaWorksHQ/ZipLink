using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Server.Migrations
{
    /// <inheritdoc />
    public partial class FixLinkStatsRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentUses",
                table: "Links",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Links",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Links",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaxUses",
                table: "Links",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TrackingEnabled",
                table: "Links",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LinkStats",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LinkId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Referer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AccessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LinkStats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LinkStats_Links_LinkId",
                        column: x => x.LinkId,
                        principalTable: "Links",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LinkStats_IpAddress",
                table: "LinkStats",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_LinkStats_LinkId",
                table: "LinkStats",
                column: "LinkId");

            migrationBuilder.CreateIndex(
                name: "IX_LinkStats_LinkId_AccessedAt",
                table: "LinkStats",
                columns: new[] { "LinkId", "AccessedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LinkStats");

            migrationBuilder.DropColumn(
                name: "CurrentUses",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "MaxUses",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "TrackingEnabled",
                table: "Links");
        }
    }
}
