using System.Data;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionWinForms.Data;
using SistemaFacturacionWinForms.Models;

namespace SistemaFacturacionWinForms.Services;

public class CartLine
{
    public int ProductId { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Gross => Money(UnitPrice * Quantity);
    public decimal Itbis => Money(Gross * .18m);
    public decimal Total => Gross + Itbis;
    public static decimal Money(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

public static class BillingService
{
    public static Invoice CreateInvoice(string connection, User cashier, List<CartLine> cart,
        int? customerId, string method, decimal paid)
    {
        if (cart.Count == 0) throw new InvalidOperationException("Agrega al menos un producto.");
        if (method != "Contado" && method != "Crédito") throw new InvalidOperationException("Forma de pago inválida.");
        if (method == "Crédito" && customerId == null) throw new InvalidOperationException("Selecciona un cliente para facturar a crédito.");
        if (cart.Any(x => x.Quantity <= 0)) throw new InvalidOperationException("Las cantidades deben ser positivas.");

        using var db = new AppDbContext(connection);
        using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
        var sequence = db.InvoiceSequences.Single(x => x.Id == 1);
        if (sequence.NextNumber > 99999) throw new InvalidOperationException("Se agotó la numeración de cinco dígitos.");
        string number = sequence.NextNumber.ToString("D5");
        sequence.NextNumber++;

        var invoice = new Invoice
        {
            Number = number, IssuedAt = DateTime.Now, CashierId = cashier.Id,
            Shift = cashier.Shift, CustomerId = customerId, PaymentMethod = method
        };
        foreach (var entry in cart)
        {
            Product product = db.Products.Single(x => x.Id == entry.ProductId);
            if (!product.IsActive || product.Stock < entry.Quantity)
                throw new InvalidOperationException($"Stock insuficiente para {product.Name} (disponible: {product.Stock}).");
            product.Stock -= entry.Quantity;
            // El precio se lee nuevamente de la base de datos para no confiar en datos de la interfaz.
            decimal gross = CartLine.Money(product.UnitPrice * entry.Quantity);
            decimal tax = CartLine.Money(gross * 0.18m);
            invoice.Lines.Add(new InvoiceLine
            {
                ProductId = product.Id, ProductCode = product.Code, ProductName = product.Name,
                Quantity = entry.Quantity, UnitPrice = product.UnitPrice,
                Gross = gross, Itbis = tax, Total = gross + tax
            });
        }
        invoice.GrossTotal = invoice.Lines.Sum(x => x.Gross);
        invoice.ItbisTotal = invoice.Lines.Sum(x => x.Itbis);
        invoice.AmountDue = invoice.GrossTotal + invoice.ItbisTotal;
        invoice.AmountPaid = method == "Contado" ? paid : 0m;
        if (method == "Contado" && paid < invoice.AmountDue)
            throw new InvalidOperationException("El monto recibido es menor que el total a pagar.");
        invoice.Change = method == "Contado" ? paid - invoice.AmountDue : 0m;
        db.Invoices.Add(invoice);
        if (method == "Crédito")
        {
            invoice.Receivable = new Receivable
            {
                CustomerId = customerId!.Value, OriginalAmount = invoice.AmountDue,
                Balance = invoice.AmountDue
            };
        }
        db.SaveChanges();
        tx.Commit();
        // La transacción ya se confirmó. Si falla la consulta posterior,
        // devolver el comprobante que acabamos de guardar evita una venta duplicada.
        try
        {
            var saved = db.Invoices.AsNoTracking().Include(x => x.Lines)
                .Include(x => x.Cashier).Include(x => x.Customer)
                .SingleOrDefault(x => x.Id == invoice.Id);
            if (saved != null) return saved;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error al leer factura {invoice.Number} después del commit: {ex}");
        }
        invoice.Cashier = cashier;
        if (customerId.HasValue)
        {
            try { invoice.Customer = db.Customers.Find(customerId.Value); }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"No se pudo recuperar el cliente de la factura: {ex}"); }
        }
        return invoice;
    }

    public static void ReceivePayment(string connection, int receivableId, int userId, decimal amount)
    {
        if (amount <= 0) throw new InvalidOperationException("El abono debe ser mayor que cero.");
        using var db = new AppDbContext(connection);
        using var tx = db.Database.BeginTransaction(IsolationLevel.Serializable);
        var debt = db.Receivables.Single(x => x.Id == receivableId);
        if (amount > debt.Balance) throw new InvalidOperationException("El abono supera el saldo pendiente.");
        debt.Balance -= amount;
        db.CollectionPayments.Add(new CollectionPayment
        {
            ReceivableId = debt.Id, UserId = userId, Amount = amount, CollectedAt = DateTime.Now
        });
        db.SaveChanges();
        tx.Commit();
    }
}
