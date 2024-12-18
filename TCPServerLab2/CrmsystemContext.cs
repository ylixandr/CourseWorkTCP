﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TCPServer;

public partial class CrmsystemContext : DbContext
{
    public CrmsystemContext()
    {
    }

    public CrmsystemContext(DbContextOptions<CrmsystemContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AdminPanel> AdminPanels { get; set; }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<Balance> Balances { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductTransaction> ProductTransactions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<StockAdjustmentRequest> StockAdjustmentRequests { get; set; }

    public virtual DbSet<SupportStatus> SupportStatuses { get; set; }

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=YLIXANDR;Initial Catalog=CRMSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");

            entity.HasIndex(e => e.Id, "IX_Account");

            entity.Property(e => e.Login).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Roles");
        });

        modelBuilder.Entity<AdminPanel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Table__3214EC0700B5A22C");

            entity.ToTable("AdminPanel");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.AdminCode)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.ManagerCode)
                .HasMaxLength(10)
                .IsFixedLength();
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Applicat__3214EC0781B00B3C");

            entity.Property(e => e.ContactInfo).HasMaxLength(255);
            entity.Property(e => e.DateSubmitted).HasColumnType("datetime");
            entity.Property(e => e.ProductName).HasMaxLength(255);
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitOfMeasurement).HasMaxLength(50);

            entity.HasOne(d => d.Account).WithMany(p => p.Applications)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__Accou__3864608B");

            entity.HasOne(d => d.Status).WithMany(p => p.Applications)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Applicati__Statu__395884C4");
        });

        modelBuilder.Entity<Balance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Balance__3214EC0733AAA219");

            entity.ToTable("Balance");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__tmp_ms_x__7AD04FF1202FE75A");

            entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MiddleName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Position)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Salary).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CD599DAFB3");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.LastUpdated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ProductName).HasMaxLength(100);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitOfMeasurement).HasMaxLength(50);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<ProductTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__ProductT__55433A6B0308EB69");

            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductTransactions)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK__ProductTr__Produ__40058253");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Statuses__3214EC073AFCCA6F");

            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<StockAdjustmentRequest>(entity =>
        {
            entity.HasKey(e => e.RequestId).HasName("PK__StockAdj__33A8517ADCD2675B");

            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionType).HasMaxLength(50);

            entity.HasOne(d => d.Product).WithMany(p => p.StockAdjustmentRequests)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_StockAdjustmentRequests_Products");
        });

        modelBuilder.Entity<SupportStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK__SupportS__C8EE20638B0EE6E5");

            entity.Property(e => e.StatusName).HasMaxLength(50);
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__SupportT__712CC6072A30559B");

            entity.Property(e => e.SubmissionDate).HasColumnType("datetime");
            entity.Property(e => e.UserEmail).HasMaxLength(255);

            entity.HasOne(d => d.Status).WithMany(p => p.SupportTickets)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SupportTi__Statu__6DCC4D03");

            entity.HasOne(d => d.User).WithMany(p => p.SupportTickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SupportTi__UserI__6EC0713C");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC078998B034");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TransactionType).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
