using InfinityPOS.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Data;

public partial class InfinityPosDbContext
{
    public virtual DbSet<CashSession> CashSessions { get; set; }

    public virtual DbSet<CashAdjustment> CashAdjustments { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CashSession>(entity =>
        {
            entity.ToTable("CashSessions");

            entity.HasKey(e => e.CashSessionId)
                .HasName("PK_CashSessions");

            entity.HasIndex(
                e => e.BusinessDate,
                "UX_CashSessions_BusinessDate")
                .IsUnique();

            entity.Property(e => e.BusinessDate)
                .HasColumnType("date");

            entity.Property(e => e.OpeningCash)
                .HasColumnType("decimal(18, 2)")
                .HasDefaultValue(0m);

            entity.Property(e => e.SystemCash)
                .HasColumnType("decimal(18, 2)")
                .HasDefaultValue(0m);

            entity.Property(e => e.ClosingCash)
                .HasColumnType("decimal(18, 2)");

            entity.Property(e => e.Difference)
                .HasColumnType("decimal(18, 2)");

            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("OPEN");

            entity.Property(e => e.OpenedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.ClosedAt)
                .HasPrecision(0);

            entity.Property(e => e.Notes)
                .HasMaxLength(1000);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())");

            entity.HasOne(e => e.OpenedByUser)
                .WithMany()
                .HasForeignKey(e => e.OpenedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashSessions_OpenedBy");

            entity.HasOne(e => e.ClosedByUser)
                .WithMany()
                .HasForeignKey(e => e.ClosedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashSessions_ClosedBy");
        });

        modelBuilder.Entity<CashAdjustment>(entity =>
        {
            entity.ToTable("CashAdjustments");

            entity.HasKey(e => e.CashAdjustmentId)
                .HasName("PK_CashAdjustments");

            entity.HasIndex(
                e => e.CashSessionId,
                "IX_CashAdjustments_CashSessionId");

            entity.HasIndex(
                e => e.TransactionDate,
                "IX_CashAdjustments_TransactionDate");

            entity.Property(e => e.TransactionDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.Amount)
                .HasColumnType("decimal(18, 2)");

            entity.Property(e => e.ReferenceNumber)
                .HasMaxLength(100);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(getdate())");

            entity.Property(e => e.IsVoided)
                .HasDefaultValue(false);

            entity.HasOne(e => e.CashSession)
                .WithMany(e => e.CashAdjustments)
                .HasForeignKey(e => e.CashSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashAdjustments_CashSession");

            entity.HasOne(e => e.CreatedByUser)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CashAdjustments_CreatedBy");
        });
    }
}
