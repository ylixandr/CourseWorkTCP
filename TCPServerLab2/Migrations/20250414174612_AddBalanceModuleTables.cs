using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCPServer.Migrations
{
    /// <inheritdoc />
    public partial class AddBalanceModuleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "RUB"),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    DepreciationRate = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DescriptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assets_Descriptions",
                        column: x => x.DescriptionId,
                        principalTable: "Descriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Equity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false),
                    DescriptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equity_Descriptions",
                        column: x => x.DescriptionId,
                        principalTable: "Descriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Liabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    DescriptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Liabilities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Liabilities_Descriptions",
                        column: x => x.DescriptionId,
                        principalTable: "Descriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DescriptionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operations_Accounts",
                        column: x => x.UserId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Operations_Descriptions",
                        column: x => x.DescriptionId,
                        principalTable: "Descriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    OperationId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Accounts",
                        column: x => x.UserId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Operations",
                        column: x => x.OperationId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_DescriptionId",
                table: "Assets",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OperationId",
                table: "AuditLogs",
                column: "OperationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Equity_DescriptionId",
                table: "Equity",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Liabilities_DescriptionId",
                table: "Liabilities",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_DescriptionId",
                table: "Operations",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_UserId",
                table: "Operations",
                column: "UserId");
            migrationBuilder.Sql(
                "INSERT INTO Assets (Category, Name, Amount, Currency, AcquisitionDate) VALUES " +
                "('Денежные средства', 'Основной счет', 1000000, 'RUB', GETDATE()), " +
                "('Основные средства', 'Офисное здание', 5000000, 'RUB', GETDATE())");

            migrationBuilder.Sql(
                "INSERT INTO Liabilities (Category, Name, Amount, DueDate) VALUES " +
                "('Кредиторская задолженность', 'Поставщик А', 200000, DATEADD(year, 1, GETDATE()))");

            migrationBuilder.Sql(
                "INSERT INTO Equity (Name, Amount, Date) VALUES " +
                "('Уставный капитал', 5800000, GETDATE())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assets");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Equity");

            migrationBuilder.DropTable(
                name: "Liabilities");

            migrationBuilder.DropTable(
                name: "Operations");
        }
    }
}
