using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCPServer.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogRelationToBalanceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Accounts",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuditLogs");

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "BalanceSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalAssets = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLiabilities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Equity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BalanceSnapshot_AuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalanceSnapshot_AuditLogId",
                table: "BalanceSnapshot",
                column: "AuditLogId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BalanceSnapshot");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "AuditLogs");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Accounts",
                table: "AuditLogs",
                column: "UserId",
                principalTable: "Account",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
