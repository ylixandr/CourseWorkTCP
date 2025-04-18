using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCPServer.Migrations
{
    /// <inheritdoc />
    public partial class TungTIngTingSahur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_AuditLogs_AuditLogId",
                table: "InventoryTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "AuditLogId",
                table: "InventoryTransactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_AuditLogs_AuditLogId",
                table: "InventoryTransactions",
                column: "AuditLogId",
                principalTable: "AuditLogs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_AuditLogs_AuditLogId",
                table: "InventoryTransactions");

            migrationBuilder.AlterColumn<int>(
                name: "AuditLogId",
                table: "InventoryTransactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_AuditLogs_AuditLogId",
                table: "InventoryTransactions",
                column: "AuditLogId",
                principalTable: "AuditLogs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
