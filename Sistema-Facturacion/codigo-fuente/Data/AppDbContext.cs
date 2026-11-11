using Microsoft.EntityFrameworkCore;
using SistemaFacturacionWinForms.Models;

namespace SistemaFacturacionWinForms.Data;

public class AppDbContext : DbContext
{
    private readonly string _connectionString;
    public AppDbContext(string connectionString) => _connectionString = connectionString;

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Receivable> Receivables => Set<Receivable>();
    public DbSet<CollectionPayment> CollectionPayments => Set<CollectionPayment>();
    public DbSet<InvoiceSequence> InvoiceSequences => Set<InvoiceSequence>();

    protected override void OnConfiguring(DbContextOptionsBuilder b)
    {
        b.UseSqlServer(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<InvoiceSequence>().Property(x => x.Id).ValueGeneratedNever();
        model.Entity<User>().HasIndex(x => x.Username).IsUnique();
        model.Entity<Product>().HasIndex(x => x.Code).IsUnique();
        model.Entity<Invoice>().HasIndex(x => x.Number).IsUnique();
        model.Entity<Receivable>().HasIndex(x => x.InvoiceId).IsUnique();

        model.Entity<Product>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        model.Entity<Invoice>().Property(x => x.GrossTotal).HasPrecision(18, 2);
        model.Entity<Invoice>().Property(x => x.ItbisTotal).HasPrecision(18, 2);
        model.Entity<Invoice>().Property(x => x.AmountDue).HasPrecision(18, 2);
        model.Entity<Invoice>().Property(x => x.AmountPaid).HasPrecision(18, 2);
        model.Entity<Invoice>().Property(x => x.Change).HasPrecision(18, 2);
        model.Entity<InvoiceLine>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        model.Entity<InvoiceLine>().Property(x => x.Gross).HasPrecision(18, 2);
        model.Entity<InvoiceLine>().Property(x => x.Itbis).HasPrecision(18, 2);
        model.Entity<InvoiceLine>().Property(x => x.Total).HasPrecision(18, 2);
        model.Entity<Receivable>().Property(x => x.OriginalAmount).HasPrecision(18, 2);
        model.Entity<Receivable>().Property(x => x.Balance).HasPrecision(18, 2);
        model.Entity<CollectionPayment>().Property(x => x.Amount).HasPrecision(18, 2);

        model.Entity<Invoice>().HasOne(x => x.Cashier).WithMany().HasForeignKey(x => x.CashierId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Invoice>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<InvoiceLine>().HasOne(x => x.Invoice).WithMany(x => x.Lines).HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<InvoiceLine>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Receivable>().HasOne(x => x.Invoice).WithOne(x => x.Receivable).HasForeignKey<Receivable>(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Receivable>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<CollectionPayment>().HasOne(x => x.Receivable).WithMany(x => x.Payments).HasForeignKey(x => x.ReceivableId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<CollectionPayment>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
