using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCPServer.Migrations
{
    /// <inheritdoc />
    public partial class RemoveOperationIdFromAuditLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Operations",
                table: "AuditLogs");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_OperationId",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "OperationId",
                table: "AuditLogs",
                newName: "EntityId");

            migrationBuilder.AddColumn<string>(
                name: "Details",
                table: "AuditLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "AuditLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Details",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "EntityId",
                table: "AuditLogs",
                newName: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OperationId",
                table: "AuditLogs",
                column: "OperationId");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Operations",
                table: "AuditLogs",
                column: "OperationId",
                principalTable: "Operations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
