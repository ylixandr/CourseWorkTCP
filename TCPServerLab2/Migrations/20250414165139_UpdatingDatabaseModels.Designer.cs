﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TCPServer;

#nullable disable

namespace TCPServer.Migrations
{
    [DbContext(typeof(CrmsystemContext))]
    [Migration("20250414165139_UpdatingDatabaseModels")]
    partial class UpdatingDatabaseModels
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("TCPServer.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.HasIndex(new[] { "Id" }, "IX_Account");

                    b.ToTable("Account", (string)null);
                });

            modelBuilder.Entity("TCPServer.Application", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AccountId")
                        .HasColumnType("int");

                    b.Property<string>("ContactInfo")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime?>("DateSubmitted")
                        .HasColumnType("datetime");

                    b.Property<int?>("DescriptionId")
                        .HasColumnType("int");

                    b.Property<int?>("ProductId")
                        .HasColumnType("int");

                    b.Property<int?>("Quantity")
                        .HasColumnType("int");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.Property<decimal?>("TotalPrice")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<string>("UnitOfMeasurement")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id")
                        .HasName("PK__Applicat__3214EC0781B00B3C");

                    b.HasIndex("AccountId");

                    b.HasIndex("DescriptionId");

                    b.HasIndex("ProductId");

                    b.HasIndex("StatusId");

                    b.ToTable("Applications");
                });

            modelBuilder.Entity("TCPServer.BalanceHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Balance")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<DateTime>("PeriodEnd")
                        .HasColumnType("datetime");

                    b.Property<DateTime>("PeriodStart")
                        .HasColumnType("datetime");

                    b.Property<decimal>("TotalExpenses")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<decimal>("TotalIncome")
                        .HasColumnType("decimal(18, 2)");

                    b.HasKey("Id");

                    b.ToTable("BalanceHistories");
                });

            modelBuilder.Entity("TCPServer.Description", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Descriptions");
                });

            modelBuilder.Entity("TCPServer.Employee", b =>
                {
                    b.Property<int>("EmployeeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasColumnName("EmployeeID");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EmployeeId"));

                    b.Property<DateOnly?>("BirthDate")
                        .HasColumnType("date");

                    b.Property<string>("FirstName")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<DateOnly?>("HireDate")
                        .HasColumnType("date");

                    b.Property<string>("LastName")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Position")
                        .HasMaxLength(100)
                        .IsUnicode(false)
                        .HasColumnType("varchar(100)");

                    b.Property<decimal?>("Salary")
                        .HasColumnType("decimal(10, 2)");

                    b.HasKey("EmployeeId")
                        .HasName("PK__tmp_ms_x__7AD04FF1202FE75A");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("TCPServer.Product", b =>
                {
                    b.Property<int>("ProductId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductId"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<DateTime?>("LastUpdated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<string>("UnitOfMeasurement")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal?>("UnitPrice")
                        .HasColumnType("decimal(18, 2)");

                    b.HasKey("ProductId")
                        .HasName("PK__Products__B40CC6CD599DAFB3");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("TCPServer.ProductTransaction", b =>
                {
                    b.Property<int>("TransactionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TransactionId"));

                    b.Property<int?>("DescriptionId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<DateTime>("TransactionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.HasKey("TransactionId")
                        .HasName("PK__ProductT__55433A6B3E1C5F5D");

                    b.HasIndex("DescriptionId");

                    b.HasIndex("ProductId");

                    b.ToTable("ProductTransactions");
                });

            modelBuilder.Entity("TCPServer.Role", b =>
                {
                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("RoleId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("TCPServer.Status", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("StatusName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id")
                        .HasName("PK__Statuses__3214EC073AFCCA6F");

                    b.ToTable("Statuses");
                });

            modelBuilder.Entity("TCPServer.StockAdjustmentRequest", b =>
                {
                    b.Property<int>("RequestId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RequestId"));

                    b.Property<int?>("DescriptionId")
                        .HasColumnType("int");

                    b.Property<int>("ProductId")
                        .HasColumnType("int");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<DateTime>("RequestDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.Property<string>("TransactionType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("RequestId")
                        .HasName("PK__StockAdj__33A8517ADCD2675B");

                    b.HasIndex("DescriptionId");

                    b.HasIndex("ProductId");

                    b.ToTable("StockAdjustmentRequests");
                });

            modelBuilder.Entity("TCPServer.SupportTicket", b =>
                {
                    b.Property<int>("TicketId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TicketId"));

                    b.Property<int?>("DescriptionId")
                        .HasColumnType("int");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.Property<DateTime>("SubmissionDate")
                        .HasColumnType("datetime");

                    b.Property<string>("UserEmail")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.HasKey("TicketId")
                        .HasName("PK__SupportT__712CC6072A30559B");

                    b.HasIndex("DescriptionId");

                    b.HasIndex("StatusId");

                    b.HasIndex("UserId");

                    b.ToTable("SupportTickets");
                });

            modelBuilder.Entity("TCPServer.Transaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Amount")
                        .HasColumnType("decimal(18, 2)");

                    b.Property<int>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int?>("DescriptionId")
                        .HasColumnType("int");

                    b.Property<int?>("RelatedEntityId")
                        .HasColumnType("int");

                    b.Property<string>("RelatedEntityType")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime>("TransactionDate")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetime")
                        .HasDefaultValueSql("(getdate())");

                    b.HasKey("Id")
                        .HasName("PK__Transact__3214EC078998B034");

                    b.HasIndex("CategoryId");

                    b.HasIndex("DescriptionId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("TCPServer.TransactionCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("TransactionCategories");
                });

            modelBuilder.Entity("TCPServer.Account", b =>
                {
                    b.HasOne("TCPServer.Role", "Role")
                        .WithMany("Accounts")
                        .HasForeignKey("RoleId")
                        .IsRequired()
                        .HasConstraintName("FK_Account_Roles");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("TCPServer.Application", b =>
                {
                    b.HasOne("TCPServer.Account", "Account")
                        .WithMany("Applications")
                        .HasForeignKey("AccountId")
                        .IsRequired()
                        .HasConstraintName("FK__Applicati__Accou__3864608B");

                    b.HasOne("TCPServer.Description", "Description")
                        .WithMany("Applications")
                        .HasForeignKey("DescriptionId")
                        .HasConstraintName("FK_Applications_Descriptions");

                    b.HasOne("TCPServer.Product", "Product")
                        .WithMany("Applications")
                        .HasForeignKey("ProductId")
                        .HasConstraintName("FK_Applications_Products");

                    b.HasOne("TCPServer.Status", "Status")
                        .WithMany("Applications")
                        .HasForeignKey("StatusId")
                        .IsRequired()
                        .HasConstraintName("FK__Applicati__Statu__395884C4");

                    b.Navigation("Account");

                    b.Navigation("Description");

                    b.Navigation("Product");

                    b.Navigation("Status");
                });

            modelBuilder.Entity("TCPServer.ProductTransaction", b =>
                {
                    b.HasOne("TCPServer.Description", "Description")
                        .WithMany("ProductTransactions")
                        .HasForeignKey("DescriptionId")
                        .HasConstraintName("FK_ProductTransactions_Descriptions");

                    b.HasOne("TCPServer.Product", "Product")
                        .WithMany("ProductTransactions")
                        .HasForeignKey("ProductId")
                        .IsRequired()
                        .HasConstraintName("FK__ProductTr__Produ__4CA06362");

                    b.Navigation("Description");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("TCPServer.StockAdjustmentRequest", b =>
                {
                    b.HasOne("TCPServer.Description", "Description")
                        .WithMany("StockAdjustmentRequests")
                        .HasForeignKey("DescriptionId")
                        .HasConstraintName("FK_StockAdjustmentRequests_Descriptions");

                    b.HasOne("TCPServer.Product", "Product")
                        .WithMany("StockAdjustmentRequests")
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_StockAdjustmentRequests_Products");

                    b.Navigation("Description");

                    b.Navigation("Product");
                });

            modelBuilder.Entity("TCPServer.SupportTicket", b =>
                {
                    b.HasOne("TCPServer.Description", "Description")
                        .WithMany("SupportTickets")
                        .HasForeignKey("DescriptionId")
                        .HasConstraintName("FK_SupportTickets_Descriptions");

                    b.HasOne("TCPServer.Status", "Status")
                        .WithMany("SupportTickets")
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("TCPServer.Account", "User")
                        .WithMany("SupportTickets")
                        .HasForeignKey("UserId")
                        .IsRequired()
                        .HasConstraintName("FK__SupportTi__UserI__6EC0713C");

                    b.Navigation("Description");

                    b.Navigation("Status");

                    b.Navigation("User");
                });

            modelBuilder.Entity("TCPServer.Transaction", b =>
                {
                    b.HasOne("TCPServer.TransactionCategory", "Category")
                        .WithMany("Transactions")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("FK_Transactions_TransactionCategories");

                    b.HasOne("TCPServer.Description", "Description")
                        .WithMany("Transactions")
                        .HasForeignKey("DescriptionId")
                        .HasConstraintName("FK_Transactions_Descriptions");

                    b.Navigation("Category");

                    b.Navigation("Description");
                });

            modelBuilder.Entity("TCPServer.Account", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("SupportTickets");
                });

            modelBuilder.Entity("TCPServer.Description", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("ProductTransactions");

                    b.Navigation("StockAdjustmentRequests");

                    b.Navigation("SupportTickets");

                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("TCPServer.Product", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("ProductTransactions");

                    b.Navigation("StockAdjustmentRequests");
                });

            modelBuilder.Entity("TCPServer.Role", b =>
                {
                    b.Navigation("Accounts");
                });

            modelBuilder.Entity("TCPServer.Status", b =>
                {
                    b.Navigation("Applications");

                    b.Navigation("SupportTickets");
                });

            modelBuilder.Entity("TCPServer.TransactionCategory", b =>
                {
                    b.Navigation("Transactions");
                });
#pragma warning restore 612, 618
        }
    }
}
