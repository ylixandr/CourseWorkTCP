using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TCPServer.balanceModule;

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



    public virtual DbSet<Application> Applications { get; set; }


    public virtual DbSet<BalanceHistory> BalanceHistories { get; set; }
    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductTransaction> ProductTransactions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<StockAdjustmentRequest> StockAdjustmentRequests { get; set; }
    public virtual DbSet<Description> Descriptions { get; set; }

    public virtual DbSet<TransactionCategory> TransactionCategories { get; set; }

    public virtual DbSet<SupportTicket> SupportTickets { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public DbSet<Asset> Assets { get; set; }
    public DbSet<Liability> Liabilities { get; set; }
    public DbSet<Equity> Equity { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=YLIXANDR;Initial Catalog=CRMSystem;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TransactionCategory>(entity =>
        {
            entity.Property(e => e.Name)
                  .HasMaxLength(50)
                  .IsRequired();
        });
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
        modelBuilder.Entity<TransactionCategory>(entity =>
        {
            entity.Property(e => e.Name).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Description>(entity =>
        {
            entity.Property(e => e.Content).IsRequired();
        });
        modelBuilder.Entity<BalanceHistory>(entity =>
        {
            entity.Property(e => e.PeriodStart).HasColumnType("datetime").IsRequired();
            entity.Property(e => e.PeriodEnd).HasColumnType("datetime").IsRequired();
            entity.Property(e => e.TotalIncome).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalExpenses).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Balance).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Applicat__3214EC0781B00B3C");
            entity.Property(e => e.ContactInfo).HasMaxLength(255);
            entity.Property(e => e.DateSubmitted).HasColumnType("datetime");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.UnitOfMeasurement).HasMaxLength(50);

            entity.HasOne(d => d.Account)
                  .WithMany(p => p.Applications)
                  .HasForeignKey(d => d.AccountId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__Applicati__Accou__3864608B");

            entity.HasOne(d => d.Product)
                  .WithMany(p => p.Applications)
                  .HasForeignKey(d => d.ProductId)
                  .HasConstraintName("FK_Applications_Products");

            entity.HasOne(d => d.Status)
                  .WithMany(p => p.Applications)
                  .HasForeignKey(d => d.StatusId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__Applicati__Statu__395884C4");

            entity.HasOne(d => d.Description) // Новая связь
                  .WithMany(p => p.Applications)
                  .HasForeignKey(d => d.DescriptionId)
                  .HasConstraintName("FK_Applications_Descriptions");
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
            entity.HasKey(e => e.TransactionId).HasName("PK__ProductT__55433A6B3E1C5F5D");

            entity.Property(e => e.TransactionDate)
                  .HasDefaultValueSql("(getdate())")
                  .HasColumnType("datetime");
            entity.Property(e => e.Quantity)
                  .HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Product)
                  .WithMany(p => p.ProductTransactions)
                  .HasForeignKey(d => d.ProductId)
                  .OnDelete(DeleteBehavior.ClientSetNull)
                  .HasConstraintName("FK__ProductTr__Produ__4CA06362");

            entity.HasOne(d => d.Description)
                  .WithMany(p => p.ProductTransactions)
                  .HasForeignKey(d => d.DescriptionId)
                  .HasConstraintName("FK_ProductTransactions_Descriptions");
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
            entity.Property(e => e.Type).HasMaxLength(50); // Добавляем поле Type
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
            entity.HasOne(d => d.Description) // Новая связь
          .WithMany(p => p.StockAdjustmentRequests)
          .HasForeignKey(d => d.DescriptionId)
          .HasConstraintName("FK_StockAdjustmentRequests_Descriptions");
        });



        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(e => e.TicketId).HasName("PK__SupportT__712CC6072A30559B");

            entity.Property(e => e.SubmissionDate).HasColumnType("datetime");
            entity.Property(e => e.UserEmail).HasMaxLength(255);

          
            entity.HasOne(d => d.User).WithMany(p => p.SupportTickets)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SupportTi__UserI__6EC0713C");
            entity.HasOne(d => d.Description) // Новая связь
          .WithMany(p => p.SupportTickets)
          .HasForeignKey(d => d.DescriptionId)
          .HasConstraintName("FK_SupportTickets_Descriptions");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC078998B034");
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TransactionDate)
                  .HasDefaultValueSql("(getdate())")
                  .HasColumnType("datetime");
            entity.Property(e => e.RelatedEntityType).HasMaxLength(50);

            entity.HasOne(d => d.Category)
                  .WithMany(p => p.Transactions)
                  .HasForeignKey(d => d.CategoryId)
                  .HasConstraintName("FK_Transactions_TransactionCategories");

            entity.HasOne(d => d.Description)
                  .WithMany(p => p.Transactions)
                  .HasForeignKey(d => d.DescriptionId)
                  .HasConstraintName("FK_Transactions_Descriptions");
        });
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("RUB");
            entity.Property(e => e.AcquisitionDate).HasColumnType("datetime");
            entity.Property(e => e.DepreciationRate).HasColumnType("decimal(5, 2)");
            entity.HasOne(e => e.Description)
                  .WithMany(d => d.Assets)
                  .HasForeignKey(e => e.DescriptionId)
                  .HasConstraintName("FK_Assets_Descriptions");
        });

        // Liability
        modelBuilder.Entity<Liability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DueDate).HasColumnType("datetime");
            entity.HasOne(e => e.Description)
                  .WithMany(d => d.Liabilities)
                  .HasForeignKey(e => e.DescriptionId)
                  .HasConstraintName("FK_Liabilities_Descriptions");
        });

        // Equity
        modelBuilder.Entity<Equity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.HasOne(e => e.Description)
                  .WithMany(d => d.Equity)
                  .HasForeignKey(e => e.DescriptionId)
                  .HasConstraintName("FK_Equity_Descriptions");
        });

        // Operation
        modelBuilder.Entity<Operation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Date).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.Description)
                  .WithMany(d => d.Operations)
                  .HasForeignKey(e => e.DescriptionId)
                  .HasConstraintName("FK_Operations_Descriptions");
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .HasConstraintName("FK_Operations_Accounts");
        });

        // AuditLog
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Timestamp).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .HasConstraintName("FK_AuditLogs_Accounts");
            entity.HasOne(e => e.Operation)
                  .WithMany()
                  .HasForeignKey(e => e.OperationId)
                  .HasConstraintName("FK_AuditLogs_Operations");
        });

        // Обновление существующей сущности Description
        modelBuilder.Entity<Description>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            // Добавляем навигационные свойства для новых сущностей
            entity.HasMany(d => d.Assets)
                  .WithOne(a => a.Description)
                  .HasForeignKey(a => a.DescriptionId);
            entity.HasMany(d => d.Liabilities)
                  .WithOne(l => l.Description)
                  .HasForeignKey(l => l.DescriptionId);
            entity.HasMany(d => d.Equity)
                  .WithOne(e => e.Description)
                  .HasForeignKey(e => e.DescriptionId);
            entity.HasMany(d => d.Operations)
                  .WithOne(o => o.Description)
                  .HasForeignKey(o => o.DescriptionId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
