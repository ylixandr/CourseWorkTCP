using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TCPServer.balanceModule;


namespace TCPServer
{
    public partial class CrmsystemContext : DbContext
    {
        public CrmsystemContext()
        {
        }

        public CrmsystemContext(DbContextOptions<CrmsystemContext> options)
            : base(options)
        {
        }

        // Существующие DbSet
        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
      
        public virtual DbSet<Description> Descriptions { get; set; }
        
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Liability> Liabilities { get; set; }
        public DbSet<Equity> Equity { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<BalanceSnapshot> BalanceSnapshots { get; set; }

        // Новые DbSet для модуля "Учёт продукции"
        public DbSet<ProductionModule.Product> Products { get; set; }
        public DbSet<ProductionModule.ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductionModule.ProductBatch> ProductBatches { get; set; }
        public DbSet<ProductionModule.Warehouse> Warehouses { get; set; }
        public DbSet<ProductionModule.Inventory> Inventories { get; set; }
        public DbSet<ProductionModule.InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<ProductionModule.ProductComponent> ProductComponents { get; set; }
        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Data Source=YLIXANDR;Initial Catalog=CourseWork;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Существующие конфигурации
            modelBuilder.Entity<Account>(entity =>
            {
                entity.ToTable("Account");
                entity.HasIndex(e => e.Id, "IX_Account");
                entity.Property(e => e.Login).HasMaxLength(100);
                entity.Property(e => e.Password).HasMaxLength(50);
                entity.HasOne(d => d.Role)
                      .WithMany(p => p.Accounts)
                      .HasForeignKey(d => d.RoleId)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("FK_Account_Roles");
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.EmployeeId);
                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
                entity.Property(e => e.FirstName).HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.LastName).HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.MiddleName).HasMaxLength(50).IsUnicode(false);
                entity.Property(e => e.Position).HasMaxLength(100).IsUnicode(false);
                entity.Property(e => e.Salary).HasColumnType("decimal(10, 2)");
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.Property(e => e.RoleId).ValueGeneratedNever();
                entity.Property(e => e.RoleName).HasMaxLength(50);
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

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.UserName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityType).HasMaxLength(50);
                entity.Property(e => e.Details).HasMaxLength(500);
                entity.Property(e => e.Timestamp).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<BalanceSnapshot>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalAssets).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalLiabilities).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Equity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Timestamp).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.AuditLog)
                      .WithMany() // Изменили на один-ко-многим
                      .HasForeignKey(e => e.AuditLogId);
            });

            modelBuilder.Entity<Description>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
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

            // Конфигурация новых сущностей для модуля "Учёт продукции"
            modelBuilder.Entity<ProductionModule.Product>(entity =>
            {
                entity.HasKey(e => e.ProductId);
                entity.Property(e => e.PurchasePrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SellingPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(e => e.CategoryId);
                entity.HasOne(e => e.Description)
                      .WithMany(d => d.Products)
                      .HasForeignKey(e => e.DescriptionId);
            });

            modelBuilder.Entity<ProductionModule.ProductCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.ParentCategory)
                      .WithMany(c => c.SubCategories)
                      .HasForeignKey(e => e.ParentCategoryId);
            });

            modelBuilder.Entity<ProductionModule.ProductBatch>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Cost).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Batches)
                      .HasForeignKey(e => e.ProductId);
            });

            modelBuilder.Entity<ProductionModule.Warehouse>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<ProductionModule.Inventory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ReservedQuantity).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Inventories)
                      .HasForeignKey(e => e.ProductId);
                entity.HasOne(e => e.Warehouse)
                      .WithMany(w => w.Inventories)
                      .HasForeignKey(e => e.WarehouseId);
            });

            modelBuilder.Entity<ProductionModule.InventoryTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.Property(e => e.TransactionDate).HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.TransactionType)
                      .HasConversion<string>(); // Конвертация перечисления в строку
                entity.HasOne(e => e.ProductBatch)
                      .WithMany(b => b.Transactions)
                      .HasForeignKey(e => e.ProductBatchId);
                entity.HasOne(e => e.FromWarehouse)
                      .WithMany(w => w.FromTransactions)
                      .HasForeignKey(e => e.FromWarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.ToWarehouse)
                      .WithMany(w => w.ToTransactions)
                      .HasForeignKey(e => e.ToWarehouseId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.AuditLog)
                      .WithMany(a => a.InventoryTransactions)
                      .HasForeignKey(e => e.AuditLogId);
            });

            modelBuilder.Entity<ProductionModule.ProductComponent>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.ParentProduct)
                      .WithMany(p => p.Components)
                      .HasForeignKey(e => e.ParentProductId);
                entity.HasOne(e => e.ComponentProduct)
                      .WithMany()
                      .HasForeignKey(e => e.ComponentProductId);
            });

            modelBuilder.Entity<ExchangeRate>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Rate).HasColumnType("decimal(18,6)");
                entity.Property(e => e.Date).HasColumnType("datetime");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}