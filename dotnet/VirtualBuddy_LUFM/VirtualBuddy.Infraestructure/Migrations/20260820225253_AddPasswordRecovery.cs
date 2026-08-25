using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VirtualBuddy.Infraestructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionVersion",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PasswordRecoveryChallenges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InvalidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedAttempts = table.Column<int>(type: "integer", nullable: false),
                    ConcurrencyStamp = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordRecoveryChallenges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordRecoveryRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    OriginHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordRecoveryRequests", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRecoveryChallenges_UserId_IssuedAt",
                table: "PasswordRecoveryChallenges",
                columns: new[] { "UserId", "IssuedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRecoveryRequests_EmailHash_RequestedAt",
                table: "PasswordRecoveryRequests",
                columns: new[] { "EmailHash", "RequestedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PasswordRecoveryRequests_OriginHash_RequestedAt",
                table: "PasswordRecoveryRequests",
                columns: new[] { "OriginHash", "RequestedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PasswordRecoveryChallenges");

            migrationBuilder.DropTable(
                name: "PasswordRecoveryRequests");

            migrationBuilder.DropColumn(
                name: "SessionVersion",
                table: "AspNetUsers");
        }
    }
}
