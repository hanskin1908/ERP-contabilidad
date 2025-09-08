using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<ThirdParty> ThirdParties => Set<ThirdParty>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalLine> JournalLines => Set<JournalLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<CompanyConfig> CompanyConfigs => Set<CompanyConfig>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Company>(e =>
        {
            e.ToTable("company");
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Nit).HasMaxLength(30);
            e.HasData(new Company
            {
                Id = 1,
                Name = "Demo",
                Nit = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        });

        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("app_user");
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(200);
            e.Property(x => x.Role).HasMaxLength(50).IsRequired();
            e.HasIndex(x => new { x.CompanyId, x.Email }).IsUnique();
        });

        modelBuilder.Entity<ThirdParty>(e =>
        {
            e.ToTable("third_party");
            e.Property(x => x.Nit).HasMaxLength(30).IsRequired();
            e.Property(x => x.Dv).HasMaxLength(2);
            e.Property(x => x.Tipo).HasMaxLength(20).IsRequired();
            e.Property(x => x.RazonSocial).HasMaxLength(200).IsRequired();
            e.Property(x => x.Direccion).HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Telefono).HasMaxLength(50);
            e.HasIndex(x => new { x.CompanyId, x.Nit }).IsUnique();
        });

        modelBuilder.Entity<Account>(e =>
        {
            e.ToTable("account");
            e.Property(x => x.Code).HasMaxLength(20).IsRequired();
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Nature).HasMaxLength(1).IsRequired();
            e.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
            e.HasOne<Account>().WithMany().HasForeignKey(x => x.ParentId);
        });

        modelBuilder.Entity<JournalEntry>(e =>
        {
            e.ToTable("journal_entry");
            e.Property(x => x.Type).HasMaxLength(20).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.DocumentType).HasMaxLength(50);
            e.Property(x => x.DocumentNumber).HasMaxLength(50);
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.TotalDebit).HasColumnType("numeric(18,2)");
            e.Property(x => x.TotalCredit).HasColumnType("numeric(18,2)");
            e.HasIndex(x => new { x.CompanyId, x.Type, x.Number }).IsUnique();
        });

        modelBuilder.Entity<JournalLine>(e =>
        {
            e.ToTable("journal_line", tb =>
            {
                tb.HasCheckConstraint("CK_JournalLine_Positive", "\"Debit\" >= 0 AND \"Credit\" >= 0");
                tb.HasCheckConstraint("CK_JournalLine_NotBothZero", "NOT (\"Debit\" = 0 AND \"Credit\" = 0)");
            });
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.Debit).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            e.Property(x => x.Credit).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            e.HasOne<JournalEntry>().WithMany().HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId);
            e.HasIndex(x => x.AccountId).HasDatabaseName("ix_journal_line_account");
            e.HasIndex(x => x.JournalEntryId).HasDatabaseName("ix_journal_line_journal");
        });

        modelBuilder.Entity<Invoice>(e =>
        {
            e.ToTable("invoice");
            e.Property(x => x.Type).HasMaxLength(20).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(10).IsRequired();
            e.Property(x => x.Status).HasMaxLength(20).IsRequired();
            e.Property(x => x.Subtotal).HasColumnType("numeric(18,2)");
            e.Property(x => x.TaxTotal).HasColumnType("numeric(18,2)");
            e.Property(x => x.Total).HasColumnType("numeric(18,2)");
            e.HasIndex(x => new { x.CompanyId, x.Type, x.Number }).IsUnique();
            e.HasIndex(x => x.ThirdPartyId).HasDatabaseName("ix_invoice_third_party");
        });

        modelBuilder.Entity<InvoiceLine>(e =>
        {
            e.ToTable("invoice_line");
            e.Property(x => x.ItemName).HasMaxLength(200).IsRequired();
            e.Property(x => x.Quantity).HasColumnType("numeric(18,2)").HasDefaultValue(1);
            e.Property(x => x.UnitPrice).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            e.Property(x => x.Discount).HasColumnType("numeric(18,2)").HasDefaultValue(0);
            e.Property(x => x.TaxRate).HasColumnType("numeric(5,2)").HasDefaultValue(0);
            e.Property(x => x.Total).HasColumnType("numeric(18,2)");
            e.HasOne<Invoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<Account>().WithMany().HasForeignKey(x => x.AccountId);
        });

        modelBuilder.Entity<Attachment>(e =>
        {
            e.ToTable("attachment");
            e.Property(x => x.EntityType).HasMaxLength(50).IsRequired();
            e.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            e.Property(x => x.ContentType).HasMaxLength(100);
            e.Property(x => x.Url).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<CompanyConfig>(e =>
        {
            e.ToTable("company_config");
            e.HasKey(x => x.CompanyId);
        });
    }
}
