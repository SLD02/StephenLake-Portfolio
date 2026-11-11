using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Printing;
using SistemaFacturacionWinForms.Models;

namespace SistemaFacturacionWinForms.Services;

public static class InvoicePrinter
{
    public static void Preview(Invoice invoice)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));
        if (invoice.Lines == null || invoice.Lines.Count == 0)
            throw new InvalidOperationException("La factura no contiene productos. Revisa los detalles guardados en SQL Server.");
        using var document = new PrintDocument();
        // PrintPreviewDialog necesita una impresora configurada, incluso para mostrar la vista previa.
        if (!document.PrinterSettings.IsValid)
        {
            var installed = System.Drawing.Printing.PrinterSettings.InstalledPrinters
                .Cast<string>().ToList();
            var alternative = installed.FirstOrDefault(x =>
                x.Equals("Microsoft Print to PDF", StringComparison.OrdinalIgnoreCase))
                ?? installed.FirstOrDefault();
            if (alternative == null)
                throw new InvalidOperationException(
                    "Windows no tiene ninguna impresora instalada. Activa 'Microsoft Print to PDF' o instala una impresora y vuelve a abrir la factura desde Historial.");
            document.PrinterSettings.PrinterName = alternative;
        }
        document.DocumentName = $"Factura {invoice.Number}";
        document.DefaultPageSettings.Margins = new Margins(48, 48, 48, 48);
        int index = 0;
        document.BeginPrint += (_, _) => index = 0;
        document.PrintPage += (_, args) =>
        {
            var g = args.Graphics ?? throw new InvalidOperationException("Windows no pudo inicializar los gráficos para imprimir.");
            float x = args.MarginBounds.Left;
            float y = args.MarginBounds.Top;
            float w = args.MarginBounds.Width;
            using var header = new Font("Segoe UI", 17, FontStyle.Bold);
            using var strong = new Font("Segoe UI", 10, FontStyle.Bold);
            using var normal = new Font("Segoe UI", 10);
            using var tiny = new Font("Segoe UI", 9);
            void Row(string label, string value, float offset = 0)
            {
                g.DrawString(label, normal, Brushes.Black, x + offset, y);
                g.DrawString(value, normal, Brushes.Black, x + w - 210, y);
                y += 24;
            }
            if (index == 0)
            {
                g.DrawString("FACTURA DE VENTA", header, Brushes.Black, x, y); y += 40;
                g.DrawString($"Factura No. {invoice.Number}", strong, Brushes.Black, x, y); y += 28;
                Row("Fecha", invoice.IssuedAt.ToString("dd/MM/yyyy"));
                Row("Hora", invoice.IssuedAt.ToString("hh:mm:ss tt"));
                Row("Cajero", invoice.Cashier?.FullName ?? "No disponible");
                Row("Turno", invoice.Shift);
                Row("Tipo de pago", invoice.PaymentMethod);
                Row("Cliente", invoice.Customer?.Name ?? "Consumidor final");
                y += 12;
            }
            g.DrawLine(Pens.Gray, x, y, x + w, y); y += 8;
            g.DrawString("CÓDIGO / PRODUCTO", strong, Brushes.Black, x, y);
            g.DrawString("CANT.", strong, Brushes.Black, x + w - 270, y);
            g.DrawString("PRECIO", strong, Brushes.Black, x + w - 192, y);
            g.DrawString("TOTAL", strong, Brushes.Black, x + w - 97, y);
            y += 25;
            while (index < invoice.Lines.Count)
            {
                var line = invoice.Lines[index];
                g.DrawString($"{line.ProductCode} - {line.ProductName}", tiny, Brushes.Black,
                    new RectangleF(x, y, w - 280, 44));
                g.DrawString(line.Quantity.ToString(), tiny, Brushes.Black, x + w - 265, y);
                g.DrawString(line.UnitPrice.ToString("N2"), tiny, Brushes.Black, x + w - 190, y);
                g.DrawString(line.Total.ToString("N2"), tiny, Brushes.Black, x + w - 95, y);
                y += 39; index++;
                if (y > args.MarginBounds.Bottom - 155)
                {
                    args.HasMorePages = true;
                    return;
                }
            }
            y += 8;
            g.DrawLine(Pens.Gray, x, y, x + w, y); y += 15;
            Row("Total bruto", UiMoney(invoice.GrossTotal));
            Row("Total ITBIS (18%)", UiMoney(invoice.ItbisTotal));
            g.DrawString("TOTAL A PAGAR", strong, Brushes.Black, x, y);
            g.DrawString(UiMoney(invoice.AmountDue), strong, Brushes.Black, x + w - 210, y); y += 30;
            if (invoice.PaymentMethod == "Contado")
            {
                Row("Monto recibido", UiMoney(invoice.AmountPaid));
                Row("Devuelta", UiMoney(invoice.Change));
            }
            else Row("Estado", "CUENTA POR COBRAR");
            y += 22;
            g.DrawString("Gracias por su compra.", normal, Brushes.Black, x, y);
            args.HasMorePages = false;
        };
        using var dialog = new PrintPreviewDialog
        {
            Document = document, Width = 1000, Height = 730,
            StartPosition = FormStartPosition.CenterScreen,
            Text = $"Vista previa - Factura {invoice.Number}"
        };
        dialog.ShowDialog();
    }

    private static string UiMoney(decimal n) => $"RD$ {n:N2}";
}
