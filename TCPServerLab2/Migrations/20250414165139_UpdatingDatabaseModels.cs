using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCPServer.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingDatabaseModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Удаляем внешние ключи, которые зависят от изменяемых таблиц
            migrationBuilder.DropForeignKey(
                name: "FK__ProductTr__Produ__40058253",
                table: "ProductTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK__SupportTi__Statu__6DCC4D03",
                table: "SupportTickets");

            // Создаем таблицу Descriptions
            migrationBuilder.CreateTable(
                name: "Descriptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descriptions", x => x.Id);
                });

            // Создаем таблицу TransactionCategories
            migrationBuilder.CreateTable(
                name: "TransactionCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionCategories", x => x.Id);
                });

            // Добавляем начальные категории
            migrationBuilder.Sql(
                "INSERT INTO TransactionCategories (Name) VALUES ('Продажи'), ('Зарплата'), ('Закупки')");

            // Переносим данные из Transactions.Description в Descriptions
            migrationBuilder.Sql(
                "INSERT INTO Descriptions (Content) SELECT DISTINCT Description FROM Transactions WHERE Description IS NOT NULL");

            migrationBuilder.AddColumn<int>(
                name: "DescriptionId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Transactions SET DescriptionId = (SELECT TOP 1 Id FROM Descriptions WHERE Content = Transactions.Description) WHERE Description IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transactions");

            // Добавляем новые столбцы в Transactions
            migrationBuilder.RenameColumn(
                name: "TransactionType",
                table: "Transactions",
                newName: "RelatedEntityType");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RelatedEntityId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Transactions SET CategoryId = (SELECT Id FROM TransactionCategories WHERE Name = CASE " +
                "WHEN RelatedEntityType = 'Пополнение' THEN 'Продажи' " +
                "WHEN RelatedEntityType = 'Списание' THEN 'Зарплата' " +
                "ELSE 'Закупки' END)");

            migrationBuilder.Sql("ALTER TABLE Transactions ALTER COLUMN CategoryId int NOT NULL");

            // Переносим данные из SupportTickets.Description в Descriptions
            migrationBuilder.Sql(
                "INSERT INTO Descriptions (Content) SELECT DISTINCT Description FROM SupportTickets WHERE Description IS NOT NULL AND Description NOT IN (SELECT Content FROM Descriptions)");

            migrationBuilder.AddColumn<int>(
                name: "DescriptionId",
                table: "SupportTickets",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE SupportTickets SET DescriptionId = (SELECT TOP 1 Id FROM Descriptions WHERE Content = SupportTickets.Description) WHERE Description IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SupportTickets");

            // Переносим данные из StockAdjustmentRequests.Description в Descriptions
            migrationBuilder.Sql(
                "INSERT INTO Descriptions (Content) SELECT DISTINCT Description FROM StockAdjustmentRequests WHERE Description IS NOT NULL AND Description NOT IN (SELECT Content FROM Descriptions)");

            migrationBuilder.AddColumn<int>(
                name: "DescriptionId",
                table: "StockAdjustmentRequests",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE StockAdjustmentRequests SET DescriptionId = (SELECT TOP 1 Id FROM Descriptions WHERE Content = StockAdjustmentRequests.Description) WHERE Description IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "StockAdjustmentRequests");

            // Переносим данные из ProductTransactions.Description в Descriptions
            migrationBuilder.DropPrimaryKey(
                name: "PK__ProductT__55433A6B0308EB69",
                table: "ProductTransactions");

            migrationBuilder.Sql(
                "INSERT INTO Descriptions (Content) SELECT DISTINCT Description FROM ProductTransactions WHERE Description IS NOT NULL AND Description NOT IN (SELECT Content FROM Descriptions)");

            migrationBuilder.AddColumn<int>(
                name: "DescriptionId",
                table: "ProductTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE ProductTransactions SET DescriptionId = (SELECT TOP 1 Id FROM Descriptions WHERE Content = ProductTransactions.Description) WHERE Description IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ProductTransactions");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "ProductTransactions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDate",
                table: "ProductTransactions",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldNullable: true,
                oldDefaultValueSql: "(getdate())");

            // Переносим данные из Applications
            migrationBuilder.AddColumn<int>(
                name: "ProductId",
                table: "Applications",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Applications SET ProductId = (SELECT TOP 1 ProductId FROM Products WHERE Products.ProductName = Applications.ProductName) WHERE ProductName IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Applications");

            migrationBuilder.Sql(
                "INSERT INTO Descriptions (Content) SELECT DISTINCT Description FROM Applications WHERE Description IS NOT NULL AND Description NOT IN (SELECT Content FROM Descriptions)");

            migrationBuilder.AddColumn<int>(
                name: "DescriptionId",
                table: "Applications",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE Applications SET DescriptionId = (SELECT TOP 1 Id FROM Descriptions WHERE Content = Applications.Description) WHERE Description IS NOT NULL");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasurement",
                table: "Applications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactInfo",
                table: "Applications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasurement",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Statuses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ApplicationStatus");

            migrationBuilder.Sql(
                "INSERT INTO Statuses (StatusName, Type) SELECT StatusName, 'SupportStatus' FROM SupportStatuses");

            migrationBuilder.Sql(
                "UPDATE SupportTickets SET StatusId = (SELECT TOP 1 Id FROM Statuses WHERE StatusName = (SELECT StatusName FROM SupportStatuses WHERE StatusId = SupportTickets.StatusId) AND Type = 'SupportStatus')");

            migrationBuilder.DropTable(
                name: "SupportStatuses");

            migrationBuilder.CreateTable(
                name: "BalanceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodStart = table.Column<DateTime>(type: "datetime", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime", nullable: false),
                    TotalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalExpenses = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalanceHistories", x => x.Id);
                });

            migrationBuilder.Sql(
                "INSERT INTO BalanceHistories (PeriodStart, PeriodEnd, TotalIncome, TotalExpenses, Balance) " +
                "SELECT GETDATE(), GETDATE(), 0, 0, Amount FROM Balance");

            migrationBuilder.DropTable(
                name: "Balance");

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProductT__55433A6B3E1C5F5D",
                table: "ProductTransactions",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_DescriptionId",
                table: "Transactions",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportTickets_DescriptionId",
                table: "SupportTickets",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentRequests_DescriptionId",
                table: "StockAdjustmentRequests",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductTransactions_DescriptionId",
                table: "ProductTransactions",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_DescriptionId",
                table: "Applications",
                column: "DescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ProductId",
                table: "Applications",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Descriptions",
                table: "Applications",
                column: "DescriptionId",
                principalTable: "Descriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Products",
                table: "Applications",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductTransactions_Descriptions",
                table: "ProductTransactions",
                column: "DescriptionId",
                principalTable: "Descriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK__ProductTr__Produ__4CA06362",
                table: "ProductTransactions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustmentRequests_Descriptions",
                table: "StockAdjustmentRequests",
                column: "DescriptionId",
                principalTable: "Descriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Descriptions",
                table: "SupportTickets",
                column: "DescriptionId",
                principalTable: "Descriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupportTickets_Statuses_StatusId",
                table: "SupportTickets",
                column: "StatusId",
                principalTable: "Statuses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Descriptions",
                table: "Transactions",
                column: "DescriptionId",
                principalTable: "Descriptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_TransactionCategories",
                table: "Transactions",
                column: "CategoryId",
                principalTable: "TransactionCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Descriptions",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Products",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductTransactions_Descriptions",
                table: "ProductTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK__ProductTr__Produ__4CA06362",
                table: "ProductTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustmentRequests_Descriptions",
                table: "StockAdjustmentRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Descriptions",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_SupportTickets_Statuses_StatusId",
                table: "SupportTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Descriptions",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_TransactionCategories",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "BalanceHistories");

            migrationBuilder.DropTable(
                name: "Descriptions");

            migrationBuilder.DropTable(
                name: "TransactionCategories");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CategoryId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_DescriptionId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_SupportTickets_DescriptionId",
                table: "SupportTickets");

            migrationBuilder.DropIndex(
                name: "IX_StockAdjustmentRequests_DescriptionId",
                table: "StockAdjustmentRequests");

            migrationBuilder.DropPrimaryKey(
                name: "PK__ProductT__55433A6B3E1C5F5D",
                table: "ProductTransactions");

            migrationBuilder.DropIndex(
                name: "IX_ProductTransactions_DescriptionId",
                table: "ProductTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Applications_DescriptionId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_ProductId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DescriptionId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DescriptionId",
                table: "SupportTickets");

            migrationBuilder.DropColumn(
                name: "DescriptionId",
                table: "StockAdjustmentRequests");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Statuses");

            migrationBuilder.DropColumn(
                name: "DescriptionId",
                table: "ProductTransactions");

            migrationBuilder.DropColumn(
                name: "DescriptionId",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "ProductId",
                table: "Applications");

            migrationBuilder.RenameColumn(
                name: "RelatedEntityType",
                table: "Transactions",
                newName: "TransactionType");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SupportTickets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "StockAdjustmentRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TransactionDate",
                table: "ProductTransactions",
                type: "datetime",
                nullable: true,
                defaultValueSql: "(getdate())",
                oldClrType: typeof(DateTime),
                oldType: "datetime",
                oldDefaultValueSql: "(getdate())");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ProductTransactions",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionType",
                table: "ProductTransactions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasurement",
                table: "Products",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Products",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasurement",
                table: "Applications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ContactInfo",
                table: "Applications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Applications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Applications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK__ProductT__55433A6B0308EB69",
                table: "ProductTransactions",
                column: "TransactionId");

            migrationBuilder.CreateTable(
                name: "Balance",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Balance__3214EC0733AAA219", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupportStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SupportS__C8EE20638B0EE6E5", x => x.StatusId);
                });

            migrationBuilder.AddForeignKey(
                name: "FK__ProductTr__Produ__40058253",
                table: "ProductTransactions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK__SupportTi__Statu__6DCC4D03",
                table: "SupportTickets",
                column: "StatusId",
                principalTable: "SupportStatuses",
                principalColumn: "StatusId");
        }
    }
}