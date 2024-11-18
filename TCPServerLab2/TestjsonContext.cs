using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace TCPServerLab2;

public partial class TestjsonContext : DbContext
{
    public TestjsonContext()
    {
    }

    public TestjsonContext(DbContextOptions<TestjsonContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AdminPanel> AdminPanels { get; set; }

    public virtual DbSet<Balance> Balances { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=YLIXANDR;Initial Catalog=TESTJSON;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");

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

        modelBuilder.Entity<Balance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Balance__3214EC0733AAA219");

            entity.ToTable("Balance");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.EmployeeId).HasName("PK__Employee__7AD04FF166E5AEC0");

            entity.Property(e => e.EmployeeId)
                .ValueGeneratedNever()
                .HasColumnName("EmployeeID");
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

        modelBuilder.Entity<Role>(entity =>
        {
            entity.Property(e => e.RoleId).ValueGeneratedNever();
            entity.Property(e => e.RoleName).HasMaxLength(50);
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
