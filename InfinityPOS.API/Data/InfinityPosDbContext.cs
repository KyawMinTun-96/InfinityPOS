using System;
using System.Collections.Generic;
using InfinityPOS.API.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace InfinityPOS.API.Data;

public partial class InfinityPosDbContext : DbContext
{
    public InfinityPosDbContext(DbContextOptions<InfinityPosDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountType> AccountTypes { get; set; }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerType> CustomerTypes { get; set; }

    public virtual DbSet<DocumentStatus> DocumentStatuses { get; set; }

    public virtual DbSet<Expense> Expenses { get; set; }

    public virtual DbSet<ExpenseItem> ExpenseItems { get; set; }

    public virtual DbSet<FiscalPeriod> FiscalPeriods { get; set; }

    public virtual DbSet<JournalEntry> JournalEntries { get; set; }

    public virtual DbSet<JournalEntryLine> JournalEntryLines { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PriceType> PriceTypes { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductComponent> ProductComponents { get; set; }

    public virtual DbSet<ProductPrice> ProductPrices { get; set; }

    public virtual DbSet<ProductType> ProductTypes { get; set; }

    public virtual DbSet<PurchaseInvoice> PurchaseInvoices { get; set; }

    public virtual DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems { get; set; }

    public virtual DbSet<PurchasePayment> PurchasePayments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalesInvoice> SalesInvoices { get; set; }

    public virtual DbSet<SalesInvoiceItem> SalesInvoiceItems { get; set; }

    public virtual DbSet<SalesPayment> SalesPayments { get; set; }

    public virtual DbSet<StockAdjustment> StockAdjustments { get; set; }

    public virtual DbSet<StockAdjustmentItem> StockAdjustmentItems { get; set; }

    public virtual DbSet<StockAdjustmentReason> StockAdjustmentReasons { get; set; }

    public virtual DbSet<StockBalance> StockBalances { get; set; }

    public virtual DbSet<StockMovement> StockMovements { get; set; }

    public virtual DbSet<StockMovementType> StockMovementTypes { get; set; }

    public virtual DbSet<StockTransfer> StockTransfers { get; set; }

    public virtual DbSet<StockTransferItem> StockTransferItems { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Latin1_General_100_CI_AS_SC_UTF8");

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.AccountId).HasName("PK__Accounts__349DA5A6E49658C9");

            entity.HasIndex(e => e.AccountCode, "UQ_Accounts_Code").IsUnique();

            entity.Property(e => e.AccountCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.AccountName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.AccountType).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.AccountTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Accounts_Type");

            entity.HasOne(d => d.ParentAccount).WithMany(p => p.InverseParentAccount)
                .HasForeignKey(d => d.ParentAccountId)
                .HasConstraintName("FK_Accounts_Parent");
        });

        modelBuilder.Entity<AccountType>(entity =>
        {
            entity.HasKey(e => e.AccountTypeId).HasName("PK__AccountT__8F9585AFA1174886");

            entity.HasIndex(e => e.TypeCode, "UQ_AccountTypes_Code").IsUnique();

            entity.Property(e => e.TypeCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.AuditLogId).HasName("PK__AuditLog__EB5F6CBDC25314BA");

            entity.Property(e => e.ActionType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IpAddress)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TableName)
                .HasMaxLength(128)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_AuditLogs_User");
        });

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.BrandId).HasName("PK__Brands__DAD4F05E8BEDD924");

            entity.HasIndex(e => e.BrandCode, "UQ_Brands_Code").IsUnique();

            entity.Property(e => e.BrandCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BrandName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BF9A9B0BC");

            entity.HasIndex(e => e.CategoryCode, "UQ_Categories_Code").IsUnique();

            entity.Property(e => e.CategoryCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CategoryName).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.CurrencyId).HasName("PK__Currenci__14470AF05A0CED88");

            entity.HasIndex(e => e.CurrencyCode, "UQ_Currencies_Code").IsUnique();

            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.CurrencyName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Symbol).HasMaxLength(10);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64D8B452A8FC");

            entity.HasIndex(e => e.CustomerCode, "UQ_Customers_Code").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CreditLimit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.CustomerCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CustomerName).HasMaxLength(300);
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CustomerType).WithMany(p => p.Customers)
                .HasForeignKey(d => d.CustomerTypeId)
                .HasConstraintName("FK_Customers_Type");
        });

        modelBuilder.Entity<CustomerType>(entity =>
        {
            entity.HasKey(e => e.CustomerTypeId).HasName("PK__Customer__958B61ACC84D7A84");

            entity.HasIndex(e => e.TypeCode, "UQ_CustomerTypes_Code").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TypeCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<DocumentStatus>(entity =>
        {
            entity.HasKey(e => e.DocumentStatusId).HasName("PK__Document__AFDCAF5DFEC3AD3F");

            entity.HasIndex(e => new { e.DocumentType, e.StatusCode }, "UQ_DocumentStatuses").IsUnique();

            entity.Property(e => e.DocumentType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StatusCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.StatusName).HasMaxLength(100);
        });

        modelBuilder.Entity<Expense>(entity =>
        {
            entity.HasKey(e => e.ExpenseId).HasName("PK__Expenses__1445CFD35CA82333");

            entity.HasIndex(e => e.ExpenseNumber, "UQ_Expenses_Number").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ExchangeRate)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(19, 8)");
            entity.Property(e => e.ExpenseDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ExpenseNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.PayeeName).HasMaxLength(300);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_Expenses_CreatedBy");

            entity.HasOne(d => d.Currency).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Expenses_Currency");

            entity.HasOne(d => d.DocumentStatus).WithMany(p => p.Expenses)
                .HasForeignKey(d => d.DocumentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Expenses_Status");
        });

        modelBuilder.Entity<ExpenseItem>(entity =>
        {
            entity.HasKey(e => e.ExpenseItemId).HasName("PK__ExpenseI__E41A54F4154BB2F1");

            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(d => d.Account).WithMany(p => p.ExpenseItems)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExpenseItems_Account");

            entity.HasOne(d => d.Expense).WithMany(p => p.ExpenseItems)
                .HasForeignKey(d => d.ExpenseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExpenseItems_Expense");
        });

        modelBuilder.Entity<FiscalPeriod>(entity =>
        {
            entity.HasKey(e => e.FiscalPeriodId).HasName("PK__FiscalPe__9E68FFEB001F9186");

            entity.Property(e => e.ClosedAt).HasPrecision(0);
            entity.Property(e => e.PeriodName).HasMaxLength(100);

            entity.HasOne(d => d.ClosedByUser).WithMany(p => p.FiscalPeriods)
                .HasForeignKey(d => d.ClosedByUserId)
                .HasConstraintName("FK_FiscalPeriods_ClosedBy");
        });

        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.JournalEntryId).HasName("PK__JournalE__575A70DB92552C61");

            entity.HasIndex(e => e.JournalNumber, "UQ_JournalEntries_Number").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.JournalDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.JournalNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.JournalEntries)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_JournalEntries_CreatedBy");

            entity.HasOne(d => d.DocumentStatus).WithMany(p => p.JournalEntries)
                .HasForeignKey(d => d.DocumentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalEntries_Status");

            entity.HasOne(d => d.FiscalPeriod).WithMany(p => p.JournalEntries)
                .HasForeignKey(d => d.FiscalPeriodId)
                .HasConstraintName("FK_JournalEntries_Period");
        });

        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.HasKey(e => e.JournalEntryLineId).HasName("PK__JournalE__38207DB87F3FED24");

            entity.Property(e => e.Credit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Debit).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.ExchangeRate).HasColumnType("decimal(19, 8)");

            entity.HasOne(d => d.Account).WithMany(p => p.JournalEntryLines)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalEntryLines_Account");

            entity.HasOne(d => d.Currency).WithMany(p => p.JournalEntryLines)
                .HasForeignKey(d => d.CurrencyId)
                .HasConstraintName("FK_JournalEntryLines_Currency");

            entity.HasOne(d => d.JournalEntry).WithMany(p => p.JournalEntryLines)
                .HasForeignKey(d => d.JournalEntryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_JournalEntryLines_Journal");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.PaymentMethodId).HasName("PK__PaymentM__DC31C1D38F7AE2C7");

            entity.HasIndex(e => e.MethodCode, "UQ_PaymentMethods_Code").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MethodCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.MethodName).HasMaxLength(100);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.PermissionId).HasName("PK__Permissi__EFA6FB2F2DB9D94E");

            entity.HasIndex(e => e.PermissionCode, "UQ_Permissions_Code").IsUnique();

            entity.Property(e => e.PermissionCode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PermissionName).HasMaxLength(200);
        });

        modelBuilder.Entity<PriceType>(entity =>
        {
            entity.HasKey(e => e.PriceTypeId).HasName("PK__PriceTyp__F43A0320E0270C4A");

            entity.HasIndex(e => e.PriceTypeCode, "UQ_PriceTypes_Code").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsSalesPrice).HasDefaultValue(true);
            entity.Property(e => e.PriceTypeCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.PriceTypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6CDC50629CF");

            entity.HasIndex(e => e.Sku, "UQ_Products_SKU").IsUnique();

            entity.HasIndex(e => e.Barcode, "UX_Products_Barcode")
                .IsUnique()
                .HasFilter("([Barcode] IS NOT NULL)");

            entity.Property(e => e.Barcode)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ProductName).HasMaxLength(300);
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("SKU");
            entity.Property(e => e.TrackInventory).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.Brand).WithMany(p => p.Products)
                .HasForeignKey(d => d.BrandId)
                .HasConstraintName("FK_Products_Brand");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Products_Category");

            entity.HasOne(d => d.ProductType).WithMany(p => p.Products)
                .HasForeignKey(d => d.ProductTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_ProductType");

            entity.HasOne(d => d.Unit).WithMany(p => p.Products)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Unit");
        });

        modelBuilder.Entity<ProductComponent>(entity =>
        {
            entity.HasKey(e => e.ProductComponentId).HasName("PK__ProductC__E29891DFD11212E2");

            entity.HasIndex(e => new { e.ParentProductId, e.ComponentProductId }, "UQ_ProductComponents").IsUnique();

            entity.Property(e => e.CostAllocationPercent).HasColumnType("decimal(9, 4)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.ComponentProduct).WithMany(p => p.ProductComponentComponentProducts)
                .HasForeignKey(d => d.ComponentProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductComponents_Component");

            entity.HasOne(d => d.ParentProduct).WithMany(p => p.ProductComponentParentProducts)
                .HasForeignKey(d => d.ParentProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductComponents_Parent");
        });

        modelBuilder.Entity<ProductPrice>(entity =>
        {
            entity.HasKey(e => e.ProductPriceId).HasName("PK__ProductP__92B9436FAB7E6071");

            entity.HasIndex(e => new { e.ProductId, e.PriceTypeId, e.EffectiveFrom }, "IX_ProductPrices_Product_Type_Date").IsDescending(false, false, true);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.EffectiveFrom).HasPrecision(0);
            entity.Property(e => e.EffectiveTo).HasPrecision(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Price).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_ProductPrices_CreatedBy");

            entity.HasOne(d => d.Currency).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductPrices_Currency");

            entity.HasOne(d => d.PriceType).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.PriceTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductPrices_PriceType");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductPrices)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductPrices_Product");
        });

        modelBuilder.Entity<ProductType>(entity =>
        {
            entity.HasKey(e => e.ProductTypeId).HasName("PK__ProductT__A1312F6E4F2CF78D");

            entity.HasIndex(e => e.TypeCode, "UQ_ProductTypes_Code").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TypeCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<PurchaseInvoice>(entity =>
        {
            entity.HasKey(e => e.PurchaseInvoiceId).HasName("PK__Purchase__4E3CABD3AB184D72");

            entity.HasIndex(e => e.InvoiceNumber, "UQ_PurchaseInvoices_Number").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ExchangeRate)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(19, 8)");
            entity.Property(e => e.InvoiceDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.PurchaseInvoices)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_PurchaseInvoices_CreatedBy");

            entity.HasOne(d => d.Currency).WithMany(p => p.PurchaseInvoices)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseInvoices_Currency");

            entity.HasOne(d => d.DocumentStatus).WithMany(p => p.PurchaseInvoices)
                .HasForeignKey(d => d.DocumentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseInvoices_Status");

            entity.HasOne(d => d.Supplier).WithMany(p => p.PurchaseInvoices)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_PurchaseInvoices_Supplier");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.PurchaseInvoices)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseInvoices_Warehouse");
        });

        modelBuilder.Entity<PurchaseInvoiceItem>(entity =>
        {
            entity.HasKey(e => e.PurchaseInvoiceItemId).HasName("PK__Purchase__1169B490BBDCE68E");

            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TotalAmount)
                .HasComputedColumnSql("(([Quantity]*[UnitCost]-[DiscountAmount])+[TaxAmount])", true)
                .HasColumnType("decimal(38, 7)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Product).WithMany(p => p.PurchaseInvoiceItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseInvoiceItems_Product");

            entity.HasOne(d => d.PurchaseInvoice).WithMany(p => p.PurchaseInvoiceItems)
                .HasForeignKey(d => d.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchaseInvoiceItems_Invoice");
        });

        modelBuilder.Entity<PurchasePayment>(entity =>
        {
            entity.HasKey(e => e.PurchasePaymentId).HasName("PK__Purchase__16D46374690E4D9B");

            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PaymentDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(200);

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.PurchasePayments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchasePayments_Method");

            entity.HasOne(d => d.PurchaseInvoice).WithMany(p => p.PurchasePayments)
                .HasForeignKey(d => d.PurchaseInvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PurchasePayments_Invoice");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE1A17BACAE2");

            entity.HasIndex(e => e.RoleCode, "UQ_Roles_Code").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.RoleCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.RoleName).HasMaxLength(100);

            entity.HasMany(d => d.Permissions).WithMany(p => p.Roles)
                .UsingEntity<Dictionary<string, object>>(
                    "RolePermission",
                    r => r.HasOne<Permission>().WithMany()
                        .HasForeignKey("PermissionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolePermissions_Permission"),
                    l => l.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_RolePermissions_Role"),
                    j =>
                    {
                        j.HasKey("RoleId", "PermissionId");
                        j.ToTable("RolePermissions");
                    });
        });

        modelBuilder.Entity<SalesInvoice>(entity =>
        {
            entity.HasKey(e => e.SalesInvoiceId).HasName("PK__SalesInv__BA05CD1AC620EBC3");

            entity.HasIndex(e => e.InvoiceNumber, "UQ_SalesInvoices_Number").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ExchangeRate)
                .HasDefaultValue(1m)
                .HasColumnType("decimal(19, 8)");
            entity.Property(e => e.InvoiceDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.SubTotal).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.SalesInvoices)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_SalesInvoices_CreatedBy");

            entity.HasOne(d => d.Currency).WithMany(p => p.SalesInvoices)
                .HasForeignKey(d => d.CurrencyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesInvoices_Currency");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesInvoices)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_SalesInvoices_Customer");

            entity.HasOne(d => d.DocumentStatus).WithMany(p => p.SalesInvoices)
                .HasForeignKey(d => d.DocumentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesInvoices_Status");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.SalesInvoices)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesInvoices_Warehouse");
        });

        modelBuilder.Entity<SalesInvoiceItem>(entity =>
        {
            entity.HasKey(e => e.SalesInvoiceItemId).HasName("PK__SalesInv__BA84EC64AD494585");

            entity.Property(e => e.Cogsamount)
                .HasComputedColumnSql("([Quantity]*[UnitCost])", true)
                .HasColumnType("decimal(38, 7)")
                .HasColumnName("COGSAmount");
            entity.Property(e => e.DiscountAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.GrossProfit)
                .HasComputedColumnSql("(([Quantity]*[UnitPrice]-[DiscountAmount])-[Quantity]*[UnitCost])", true)
                .HasColumnType("decimal(38, 7)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TaxAmount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.TotalAmount)
                .HasComputedColumnSql("(([Quantity]*[UnitPrice]-[DiscountAmount])+[TaxAmount])", true)
                .HasColumnType("decimal(38, 7)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesInvoiceItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesInvoiceItems_Product");

            entity.HasOne(d => d.ProductPrice).WithMany(p => p.SalesInvoiceItems)
                .HasForeignKey(d => d.ProductPriceId)
                .HasConstraintName("FK_SalesInvoiceItems_ProductPrice");

            entity.HasOne(d => d.SalesInvoice).WithMany(p => p.SalesInvoiceItems)
                .HasForeignKey(d => d.SalesInvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesInvoiceItems_Invoice");
        });

        modelBuilder.Entity<SalesPayment>(entity =>
        {
            entity.HasKey(e => e.SalesPaymentId).HasName("PK__SalesPay__2864F01C8C0B4254");

            entity.Property(e => e.Amount).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PaymentDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ReferenceNumber).HasMaxLength(200);

            entity.HasOne(d => d.PaymentMethod).WithMany(p => p.SalesPayments)
                .HasForeignKey(d => d.PaymentMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesPayments_Method");

            entity.HasOne(d => d.SalesInvoice).WithMany(p => p.SalesPayments)
                .HasForeignKey(d => d.SalesInvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesPayments_Invoice");
        });

        modelBuilder.Entity<StockAdjustment>(entity =>
        {
            entity.HasKey(e => e.StockAdjustmentId).HasName("PK__StockAdj__0A9711B30058C90B");

            entity.HasIndex(e => e.AdjustmentNumber, "UQ_StockAdjustments_Number").IsUnique();

            entity.Property(e => e.AdjustmentDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.AdjustmentNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_StockAdjustments_CreatedBy");

            entity.HasOne(d => d.DocumentStatus).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.DocumentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockAdjustments_Status");

            entity.HasOne(d => d.StockAdjustmentReason).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.StockAdjustmentReasonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockAdjustments_Reason");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockAdjustments)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockAdjustments_Warehouse");
        });

        modelBuilder.Entity<StockAdjustmentItem>(entity =>
        {
            entity.HasKey(e => e.StockAdjustmentItemId).HasName("PK__StockAdj__DF39A99D0F40A491");

            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Product).WithMany(p => p.StockAdjustmentItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockAdjustmentItems_Product");

            entity.HasOne(d => d.StockAdjustment).WithMany(p => p.StockAdjustmentItems)
                .HasForeignKey(d => d.StockAdjustmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockAdjustmentItems_Adjustment");
        });

        modelBuilder.Entity<StockAdjustmentReason>(entity =>
        {
            entity.HasKey(e => e.StockAdjustmentReasonId).HasName("PK__StockAdj__B68ED081076DB9B5");

            entity.HasIndex(e => e.ReasonCode, "UQ_StockAdjustmentReasons_Code").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ReasonCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ReasonName).HasMaxLength(150);
        });

        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.HasKey(e => e.StockBalanceId).HasName("PK__StockBal__956815C72039ED4B");

            entity.HasIndex(e => new { e.ProductId, e.WarehouseId }, "UQ_StockBalances_Product_Warehouse").IsUnique();

            entity.Property(e => e.AverageCost).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Product).WithMany(p => p.StockBalances)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockBalances_Product");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockBalances)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockBalances_Warehouse");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.HasKey(e => e.StockMovementId).HasName("PK__StockMov__E963E37CE11D88A8");

            entity.HasIndex(e => new { e.ProductId, e.WarehouseId, e.MovementDate }, "IX_StockMovements_Product_Warehouse_Date");

            entity.Property(e => e.MovementDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_StockMovements_CreatedBy");

            entity.HasOne(d => d.Product).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_Product");

            entity.HasOne(d => d.StockMovementType).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.StockMovementTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_Type");

            entity.HasOne(d => d.Warehouse).WithMany(p => p.StockMovements)
                .HasForeignKey(d => d.WarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockMovements_Warehouse");
        });

        modelBuilder.Entity<StockMovementType>(entity =>
        {
            entity.HasKey(e => e.StockMovementTypeId).HasName("PK__StockMov__689B4E59D6A32BD3");

            entity.HasIndex(e => e.MovementCode, "UQ_StockMovementTypes_Code").IsUnique();

            entity.Property(e => e.Direction)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.MovementCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MovementName).HasMaxLength(150);
        });

        modelBuilder.Entity<StockTransfer>(entity =>
        {
            entity.HasKey(e => e.StockTransferId).HasName("PK__StockTra__BF0F7F3215376F64");

            entity.HasIndex(e => e.TransferNumber, "UQ_StockTransfers_Number").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.TransferDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TransferNumber)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.StockTransfers)
                .HasForeignKey(d => d.CreatedByUserId)
                .HasConstraintName("FK_StockTransfers_CreatedBy");

            entity.HasOne(d => d.DocumentStatus).WithMany(p => p.StockTransfers)
                .HasForeignKey(d => d.DocumentStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransfers_Status");

            entity.HasOne(d => d.FromWarehouse).WithMany(p => p.StockTransferFromWarehouses)
                .HasForeignKey(d => d.FromWarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransfers_FromWarehouse");

            entity.HasOne(d => d.ToWarehouse).WithMany(p => p.StockTransferToWarehouses)
                .HasForeignKey(d => d.ToWarehouseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransfers_ToWarehouse");
        });

        modelBuilder.Entity<StockTransferItem>(entity =>
        {
            entity.HasKey(e => e.StockTransferItemId).HasName("PK__StockTra__99BA6C1E79F2EDD7");

            entity.Property(e => e.Quantity).HasColumnType("decimal(19, 4)");
            entity.Property(e => e.UnitCost).HasColumnType("decimal(19, 4)");

            entity.HasOne(d => d.Product).WithMany(p => p.StockTransferItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransferItems_Product");

            entity.HasOne(d => d.StockTransfer).WithMany(p => p.StockTransferItems)
                .HasForeignKey(d => d.StockTransferId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StockTransferItems_Transfer");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE666B40ABC39D7");

            entity.HasIndex(e => e.SupplierCode, "UQ_Suppliers_Code").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Phone)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SupplierCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SupplierName).HasMaxLength(300);
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.UnitId).HasName("PK__Units__44F5ECB58BAD296B");

            entity.HasIndex(e => e.UnitCode, "UQ_Units_Code").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.UnitCode)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.UnitName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C815E9FC8");

            entity.HasIndex(e => e.Username, "UQ_Users_Username").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastLoginAt).HasPrecision(0);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_Role");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.WarehouseId).HasName("PK__Warehous__2608AFF9D38D7326");

            entity.HasIndex(e => e.WarehouseCode, "UQ_Warehouses_Code").IsUnique();

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.WarehouseCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.WarehouseName).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
