using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionWinForms.Data;
using SistemaFacturacionWinForms.Models;
using SistemaFacturacionWinForms.Services;

namespace SistemaFacturacionWinForms.Forms;

public partial class MainForm
{
    private TabPage BuildSales()
    {
        var page = new TabPage("Nueva venta") { BackColor = Ui.Surface };
        var top = Ui.Top(170);
        top.Controls.Add(new Label { Text = "FACTURA NUEVA", Left = 15, Top = 13,
            Width = 500, Height = 35, Font = new System.Drawing.Font("Segoe UI", 16, System.Drawing.FontStyle.Bold),
            ForeColor = Ui.Navy });
        top.Controls.Add(Ui.Label("Producto:", 15, 60, 85));
        top.Controls.Add(_saleProduct);
        top.Controls.Add(Ui.Label("Cantidad:", 510, 60, 95));
        _saleQuantity.Value = 1;
        _saleQuantity.Minimum = 1;
        top.Controls.Add(_saleQuantity);
        var add = Ui.Button("Agregar", 730, 53, 135);
        add.Click += (_, _) => AddProduct(); top.Controls.Add(add);
        var remove = Ui.Button("Quitar fila", 882, 53, 135, false);
        remove.Click += (_, _) => RemoveProduct(); top.Controls.Add(remove);
        top.Controls.Add(Ui.Label("Cliente:", 15, 107, 85));
        top.Controls.Add(_saleCustomer);
        top.Controls.Add(Ui.Label("Pago:", 510, 107, 90));
        _saleMethod.Items.AddRange(new object[] { "Contado", "Crédito" });
        _saleMethod.SelectedIndex = 0;
        _saleMethod.SelectedIndexChanged += (_, _) => UpdateSaleTotals();
        top.Controls.Add(_saleMethod);
        top.Controls.Add(Ui.Label("Recibido:", 805, 107, 90));
        _salePaid.ValueChanged += (_, _) => UpdateSaleTotals();
        top.Controls.Add(_salePaid);

        var bottom = Ui.Bottom(125);
        _saleTotals.Font = new System.Drawing.Font("Segoe UI", 13, System.Drawing.FontStyle.Bold);
        _saleTotals.Width = 970; _saleTotals.Height = 64;
        bottom.Controls.Add(_saleTotals);
        var sell = Ui.Button("FACTURAR E IMPRIMIR", 15, 77, 245);
        sell.Click += (_, _) => Sell(); bottom.Controls.Add(sell);
        var clear = Ui.Button("Limpiar venta", 275, 77, 185, false);
        clear.Click += (_, _) => { _cart.Clear(); RenderCart(); }; bottom.Controls.Add(clear);
        page.Controls.Add(_saleGrid); page.Controls.Add(bottom); page.Controls.Add(top);
        RenderCart();
        return page;
    }

    private void LoadSaleOptions()
    {
        using var db = new AppDbContext(_connection);
        int? selectedProduct = (_saleProduct.SelectedItem as Option<Product>)?.Value.Id;
        // Option<Customer?> y Option<Customer> son el mismo tipo en tiempo de ejecución.
        // La opción "Cliente no registrado" contiene Value = null.
        int? selectedCustomer = (_saleCustomer.SelectedItem as Option<Customer?>)?.Value?.Id;
        var products = db.Products.AsNoTracking().Where(p => p.IsActive && p.Stock > 0)
            .OrderBy(p => p.Name).ToList();
        _saleProduct.Items.Clear();
        foreach (var p in products) _saleProduct.Items.Add(new Option<Product>(p, $"{p.Code} | {p.Name} | {Ui.Money(p.UnitPrice)} | Stock: {p.Stock}"));
        if (_saleProduct.Items.Count > 0)
        {
            var index = _saleProduct.Items.Cast<Option<Product>>().ToList().FindIndex(x => x.Value.Id == selectedProduct);
            _saleProduct.SelectedIndex = index >= 0 ? index : 0;
        }
        var customers = db.Customers.AsNoTracking().OrderBy(x => x.Name).ToList();
        _saleCustomer.Items.Clear();
        _saleCustomer.Items.Add(new Option<Customer?>(null, "Cliente no registrado (solo contado)"));
        foreach (var c in customers)
            _saleCustomer.Items.Add(new Option<Customer?>(c, $"{c.Name} | {c.Phone}"));
        // Evitar consultar Id del cliente no registrado (Value = null).
        if (selectedCustomer.HasValue)
        {
            var customerIndex = _saleCustomer.Items.Cast<Option<Customer?>>()
                .ToList().FindIndex(x => x.Value?.Id == selectedCustomer);
            _saleCustomer.SelectedIndex = customerIndex >= 0 ? customerIndex : 0;
        }
        else
        {
            _saleCustomer.SelectedIndex = 0;
        }
    }

    private void AddProduct()
    {
        if (_saleProduct.SelectedItem is not Option<Product> opt) { Ui.Info("Primero registra productos con stock."); return; }
        Product product = opt.Value;
        int quantity = (int)_saleQuantity.Value;
        var line = _cart.FirstOrDefault(x => x.ProductId == product.Id);
        int totalQuantity = quantity + (line?.Quantity ?? 0);
        if (totalQuantity > product.Stock) { Ui.Info("No hay suficiente inventario disponible."); return; }
        if (line != null) { line.Quantity = totalQuantity; line.UnitPrice = product.UnitPrice; }
        else _cart.Add(new CartLine { ProductId = product.Id, Code = product.Code,
            Name = product.Name, UnitPrice = product.UnitPrice, Quantity = quantity });
        RenderCart();
    }

    private void RemoveProduct()
    {
        int index = _saleGrid.CurrentRow?.Index ?? -1;
        if (index >= 0 && index < _cart.Count) _cart.RemoveAt(index);
        RenderCart();
    }

    private void RenderCart()
    {
        _saleGrid.DataSource = _cart.Select(x => new
        {
            Código = x.Code, Producto = x.Name, Cantidad = x.Quantity,
            Precio = x.UnitPrice, Bruto = x.Gross, ITBIS = x.Itbis, Total = x.Total
        }).ToList();
        UpdateSaleTotals();
    }

    private void UpdateSaleTotals()
    {
        bool cash = _saleMethod.SelectedItem?.ToString() != "Crédito";
        _salePaid.Enabled = cash;
        decimal gross = _cart.Sum(x => x.Gross), tax = _cart.Sum(x => x.Itbis), total = gross + tax;
        _saleTotals.Text = $"Bruto: {Ui.Money(gross)}    ITBIS (18%): {Ui.Money(tax)}    A pagar: {Ui.Money(total)}\n" +
            (cash ? $"Recibido: {Ui.Money(_salePaid.Value)}    Devuelta: {Ui.Money(Math.Max(0, _salePaid.Value - total))}" : "Venta a crédito: se agregará a cuentas por cobrar.");
    }

    private void Sell()
    {
        // Los errores de uso se muestran como avisos, no como excepciones técnicas.
        if (_cart.Count == 0)
        {
            Ui.Info("Primero selecciona un producto, indica la cantidad y pulsa 'Agregar'. " +
                "Cuando el producto aparezca en la tabla, pulsa 'Facturar e imprimir'.");
            return;
        }

        // Puede estar seleccionada la opción sin cliente: nunca acceder a .Id si Value es null.
        int? customerId = (_saleCustomer.SelectedItem as Option<Customer?>)?.Value?.Id;
        string method = _saleMethod.SelectedItem?.ToString() ?? "Contado";
        if (method == "Crédito" && customerId == null)
        {
            Ui.Info("Para una venta a crédito, primero registra un cliente y selecciónalo.");
            return;
        }

        if (method == "Contado" && _salePaid.Value < _cart.Sum(x => x.Total))
        {
            Ui.Info("El monto recibido es menor que el total a pagar. Indica el dinero recibido antes de facturar.");
            return;
        }

        Invoice invoice;
        try
        {
            invoice = BillingService.CreateInvoice(_connection, _user, _cart, customerId, method, _salePaid.Value);
        }
        catch (Exception ex)
        {
            Ui.Error(new InvalidOperationException(
                "No se pudo confirmar el resultado de la facturación. Revisa Historial antes de volver a pulsar Facturar.", ex));
            return;
        }

        // El guardado está confirmado en este punto. La impresión o una recarga de controles
        // no debe presentarse como si la factura hubiera fallado (evita facturar dos veces).
        _cart.Clear();
        try
        {
            _salePaid.Value = 0;
            RenderCart();
            LoadSaleOptions();
        }
        catch (Exception refreshError)
        {
            Ui.Error(new InvalidOperationException(
                $"La factura {invoice.Number} SÍ se guardó. Falló solamente la actualización de la pantalla.",
                refreshError));
        }
        Ui.Info($"Factura {invoice.Number} guardada correctamente. Total: {Ui.Money(invoice.AmountDue)}" +
            (method == "Contado" ? $"\nDevuelta: {Ui.Money(invoice.Change)}" : "\nRegistrada como cuenta por cobrar."));
        try
        {
            InvoicePrinter.Preview(invoice);
        }
        catch (Exception printError)
        {
            Ui.Error(new InvalidOperationException(
                $"La factura {invoice.Number} SÍ se guardó en SQL Server. Solo falló la vista previa o impresión. " +
                "Puedes reimprimir desde Historial; no vuelvas a facturarla.", printError));
        }
    }

    private TabPage BuildHistory()
    {
        var page = new TabPage("Historial") { BackColor = Ui.Surface };
        var top = Ui.Top(74);
        top.Controls.Add(Ui.Label("Fecha:", 20, 24, 85));
        top.Controls.Add(_historyDate);
        var filter = Ui.Button("Consultar", 305, 17, 130);
        filter.Click += (_, _) => LoadHistory(); top.Controls.Add(filter);
        var print = Ui.Button("Reimprimir selección", 452, 17, 205, false);
        print.Click += (_, _) => ReprintSelected(); top.Controls.Add(print);
        page.Controls.Add(_historyGrid); page.Controls.Add(top);
        return page;
    }

    private void LoadHistory()
    {
        var day = _historyDate.Value.Date;
        using var db = new AppDbContext(_connection);
        var query = db.Invoices.AsNoTracking().Include(x => x.Cashier).Include(x => x.Customer)
            .Where(i => i.IssuedAt >= day && i.IssuedAt < day.AddDays(1));
        if (_user.Role != "Administrador") query = query.Where(i => i.CashierId == _user.Id);
        _historyGrid.DataSource = query.OrderByDescending(i => i.IssuedAt).ToList()
            .Select(x => new { Id = x.Id, Factura = x.Number, Fecha = x.IssuedAt,
                Cajero = x.Cashier.FullName, Turno = x.Shift,
                Cliente = x.Customer?.Name ?? "Consumidor final", Tipo = x.PaymentMethod,
                Bruto = x.GrossTotal, ITBIS = x.ItbisTotal, Total = x.AmountDue }).ToList();
        if (_historyGrid.Columns.Contains("Id")) _historyGrid.Columns["Id"].Visible = false;
    }

    private void ReprintSelected()
    {
        int? id = SelectedId(_historyGrid);
        if (id == null) { Ui.Info("Selecciona una factura."); return; }
        try
        {
            using var db = new AppDbContext(_connection);
            var invoice = db.Invoices.AsNoTracking().Include(i => i.Lines)
                .Include(i => i.Cashier).Include(i => i.Customer).Single(i => i.Id == id);
            InvoicePrinter.Preview(invoice);
        }
        catch (Exception ex) { Ui.Error(ex); }
    }
}
