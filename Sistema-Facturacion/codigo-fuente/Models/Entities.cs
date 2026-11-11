using System.ComponentModel.DataAnnotations;

namespace SistemaFacturacionWinForms.Models;

public class User
{
    public int Id { get; set; }
    [MaxLength(100)] public string FullName { get; set; } = "";
    [MaxLength(50)] public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string PasswordSalt { get; set; } = "";
    [MaxLength(20)] public string Role { get; set; } = "Cajero";
    [MaxLength(20)] public string Shift { get; set; } = "Mañana";
    public bool IsActive { get; set; } = true;
}
public class Product
{
    public int Id { get; set; }
    [MaxLength(30)] public string Code { get; set; } = "";
    [MaxLength(150)] public string Name { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;
}
public class Customer
{
    public int Id { get; set; }
    [MaxLength(150)] public string Name { get; set; } = "";
    [MaxLength(30)] public string Phone { get; set; } = "";
    [MaxLength(120)] public string Email { get; set; } = "";
}
public class Invoice
{
    public int Id { get; set; }
    [MaxLength(5)] public string Number { get; set; } = "";
    public DateTime IssuedAt { get; set; }
    public int CashierId { get; set; }
    public User Cashier { get; set; } = null!;
    [MaxLength(20)] public string Shift { get; set; } = "";
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    [MaxLength(10)] public string PaymentMethod { get; set; } = "Contado";
    public decimal GrossTotal { get; set; }
    public decimal ItbisTotal { get; set; }
    public decimal AmountDue { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal Change { get; set; }
    public List<InvoiceLine> Lines { get; set; } = new();
    public Receivable? Receivable { get; set; }
}
public class InvoiceLine
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    [MaxLength(30)] public string ProductCode { get; set; } = "";
    [MaxLength(150)] public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Gross { get; set; }
    public decimal Itbis { get; set; }
    public decimal Total { get; set; }
}
public class Receivable
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = null!;
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public decimal OriginalAmount { get; set; }
    public decimal Balance { get; set; }
    public List<CollectionPayment> Payments { get; set; } = new();
}
public class CollectionPayment
{
    public int Id { get; set; }
    public int ReceivableId { get; set; }
    public Receivable Receivable { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CollectedAt { get; set; }
    public decimal Amount { get; set; }
}
public class InvoiceSequence
{
    public int Id { get; set; } = 1;
    public int NextNumber { get; set; } = 1;
}
