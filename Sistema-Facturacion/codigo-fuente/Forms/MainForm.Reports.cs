using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionWinForms.Data;
using SistemaFacturacionWinForms.Models;

namespace SistemaFacturacionWinForms.Forms;

public partial class MainForm
{
    private TabPage BuildReports()
    {
        var page = new TabPage("Reportes") { BackColor = Ui.Surface };
        var top = Ui.Top(125);
        top.Controls.Add(Ui.Label("Reporte:", 18, 22, 80));
        _reportType.Items.AddRange(new object[]
        {
            "Cuadre del día", "Ventas por cajero", "Inventario en stock",
            "Facturas crédito o contado", "5 clientes con más ventas"
        });
        _reportType.SelectedIndex = 0;
        top.Controls.Add(_reportType);
        top.Controls.Add(Ui.Label("Fecha:", 430, 22, 65)); top.Controls.Add(_reportDate);
        var generate = Ui.Button("Generar reporte", 712, 16, 180);
        generate.Click += (_, _) => LoadReport(); top.Controls.Add(generate);
        _reportSummary.Font = new System.Drawing.Font("Segoe UI", 12, System.Drawing.FontStyle.Bold);
        _reportSummary.Height = 40; top.Controls.Add(_reportSummary);
        page.Controls.Add(_reportGrid); page.Controls.Add(top);
        return page;
    }

    private void LoadReport()
    {
        if (_reportType.SelectedItem == null) return;
        try
        {
            var date = _reportDate.Value.Date;
            string report = _reportType.SelectedItem.ToString()!;
            using var db = new AppDbContext(_connection);
            _reportSummary.Text = "";
            if (report == "Inventario en stock")
            {
                var rows = db.Products.AsNoTracking().OrderBy(x => x.Name).ToList();
                _reportGrid.DataSource = rows.Select(x => new
                {
                    Código = x.Code, Producto = x.Name, Precio = x.UnitPrice,
                    Existencia = x.Stock, ValorInventario = x.Stock * x.UnitPrice,
                    Estado = x.IsActive ? "Activo" : "Inactivo"
                }).ToList();
                _reportSummary.Text = $"Total de artículos en stock: {rows.Sum(x => x.Stock):N0}   |   Valor del inventario: {Ui.Money(rows.Sum(x => x.UnitPrice * x.Stock))}";
                return;
            }
            if (report == "5 clientes con más ventas")
            {
                var salesByCustomer = db.Invoices.AsNoTracking().Include(x => x.Customer)
                    .Where(x => x.CustomerId != null).ToList()
                    .GroupBy(x => new { x.CustomerId, Name = x.Customer!.Name })
                    .Select(g => new { Cliente = g.Key.Name, Facturas = g.Count(),
                        TotalComprado = g.Sum(x => x.AmountDue) })
                    .OrderByDescending(x => x.TotalComprado).Take(5).ToList();
                _reportGrid.DataSource = salesByCustomer;
                _reportSummary.Text = "Top 5 histórico: se cuentan las facturas asociadas a clientes registrados.";
                return;
            }

            var invoices = db.Invoices.AsNoTracking().Include(x => x.Cashier).Include(x => x.Customer)
                .Where(x => x.IssuedAt >= date && x.IssuedAt < date.AddDays(1))
                .OrderBy(x => x.IssuedAt).ToList();
            var collections = db.CollectionPayments.AsNoTracking().Where(x => x.CollectedAt >= date && x.CollectedAt < date.AddDays(1)).ToList();
            if (report == "Cuadre del día")
            {
                var cash = invoices.Where(x => x.PaymentMethod == "Contado").ToList();
                var credit = invoices.Where(x => x.PaymentMethod == "Crédito").ToList();
                _reportGrid.DataSource = invoices.GroupBy(x => new { x.CashierId, x.Cashier.FullName, x.Shift })
                    .Select(g => new
                    {
                        Cajero = g.Key.FullName, Turno = g.Key.Shift,
                        Facturas = g.Count(), VentasContado = g.Where(x => x.PaymentMethod == "Contado").Sum(x => x.AmountDue),
                        VentasCredito = g.Where(x => x.PaymentMethod == "Crédito").Sum(x => x.AmountDue),
                        ITBIS = g.Sum(x => x.ItbisTotal), TotalFacturado = g.Sum(x => x.AmountDue)
                    }).OrderBy(x => x.Turno).ToList();
                _reportSummary.Text = $"{date:dd/MM/yyyy}  |  Facturas: {invoices.Count}  |  Contado: {Ui.Money(cash.Sum(x => x.AmountDue))}  |  Crédito: {Ui.Money(credit.Sum(x => x.AmountDue))}  |  Abonos: {Ui.Money(collections.Sum(x => x.Amount))}  |  ITBIS: {Ui.Money(invoices.Sum(x => x.ItbisTotal))}";
            }
            else if (report == "Ventas por cajero")
            {
                _reportGrid.DataSource = invoices.GroupBy(x => new { x.CashierId, x.Cashier.FullName, x.Shift })
                    .Select(g => new
                    {
                        Cajero = g.Key.FullName, Turno = g.Key.Shift, CantidadFacturas = g.Count(),
                        Bruto = g.Sum(x => x.GrossTotal), ITBIS = g.Sum(x => x.ItbisTotal),
                        Total = g.Sum(x => x.AmountDue)
                    }).OrderByDescending(x => x.Total).ToList();
                _reportSummary.Text = $"{date:dd/MM/yyyy} | Ventas por cajero | Total facturado: {Ui.Money(invoices.Sum(x => x.AmountDue))}";
            }
            else if (report == "Facturas crédito o contado")
            {
                _reportGrid.DataSource = invoices.Select(x => new
                {
                    Factura = x.Number, Fecha = x.IssuedAt, Tipo = x.PaymentMethod,
                    Cajero = x.Cashier.FullName, Cliente = x.Customer?.Name ?? "Consumidor final",
                    Bruto = x.GrossTotal, ITBIS = x.ItbisTotal, Total = x.AmountDue,
                    Pagado = x.AmountPaid, Devuelta = x.Change
                }).ToList();
                _reportSummary.Text = $"{date:dd/MM/yyyy} | Contado: {invoices.Count(x => x.PaymentMethod == "Contado")} | Crédito: {invoices.Count(x => x.PaymentMethod == "Crédito")}";
            }
        }
        catch (Exception ex) { Ui.Error(ex); }
    }
}
