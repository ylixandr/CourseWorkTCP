using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCPServer.Migrations
{
    /// <inheritdoc />
    public partial class updatingBalanceSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BalanceSnapshot_AuditLogs_AuditLogId",
                table: "BalanceSnapshot");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BalanceSnapshot",
                table: "BalanceSnapshot");

            migrationBuilder.RenameTable(
                name: "BalanceSnapshot",
                newName: "BalanceSnapshots");

            migrationBuilder.RenameIndex(
                name: "IX_BalanceSnapshot_AuditLogId",
                table: "BalanceSnapshots",
                newName: "IX_BalanceSnapshots_AuditLogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BalanceSnapshots",
                table: "BalanceSnapshots",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BalanceSnapshots_AuditLogs_AuditLogId",
                table: "BalanceSnapshots",
                column: "AuditLogId",
                principalTable: "AuditLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BalanceSnapshots_AuditLogs_AuditLogId",
                table: "BalanceSnapshots");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BalanceSnapshots",
                table: "BalanceSnapshots");

            migrationBuilder.RenameTable(
                name: "BalanceSnapshots",
                newName: "BalanceSnapshot");

            migrationBuilder.RenameIndex(
                name: "IX_BalanceSnapshots_AuditLogId",
                table: "BalanceSnapshot",
                newName: "IX_BalanceSnapshot_AuditLogId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BalanceSnapshot",
                table: "BalanceSnapshot",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BalanceSnapshot_AuditLogs_AuditLogId",
                table: "BalanceSnapshot",
                column: "AuditLogId",
                principalTable: "AuditLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
