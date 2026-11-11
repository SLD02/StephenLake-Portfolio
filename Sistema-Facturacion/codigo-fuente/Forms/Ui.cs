using System.Drawing;
using System.Windows.Forms;

namespace SistemaFacturacionWinForms.Forms;

public static class Ui
{
    public static readonly Color Navy = Color.FromArgb(25, 40, 66);
    public static readonly Color Blue = Color.FromArgb(32, 104, 196);
    public static readonly Color Surface = Color.FromArgb(245, 248, 252);
    public static readonly Font FontBody = new("Segoe UI", 10f);

    public static void Configure(Form f, string title, int w = 1150, int h = 760)
    {
        f.Text = title; f.Width = w; f.Height = h; f.StartPosition = FormStartPosition.CenterScreen;
        f.BackColor = Surface; f.Font = FontBody; f.MinimumSize = new Size(Math.Min(w, 830), Math.Min(h, 560));
    }
    public static Label Label(string text, int x, int y, int width = 130) =>
        new() { Text = text, Left = x, Top = y, Width = width, Height = 27, AutoEllipsis = true };
    public static TextBox TextBox(int x, int y, int width = 200, bool password = false) =>
        new() { Left = x, Top = y, Width = width, Height = 28, UseSystemPasswordChar = password };
    public static Button Button(string text, int x, int y, int width = 135, bool primary = true)
    {
        var b = new Button
        {
            Text = text, Left = x, Top = y, Width = width, Height = 36,
            BackColor = primary ? Blue : Color.White,
            ForeColor = primary ? Color.White : Navy,
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand
        };
        b.FlatAppearance.BorderColor = primary ? Blue : Color.LightGray;
        return b;
    }
    public static DataGridView Grid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false, BackgroundColor = Color.White,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BorderStyle = BorderStyle.None, AutoGenerateColumns = true,
            EnableHeadersVisualStyles = false
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = Navy;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersHeight = 37;
        grid.RowTemplate.Height = 31;
        return grid;
    }
    public static Panel Top(int h) => new() { Dock = DockStyle.Top, Height = h, BackColor = Color.White, AutoScroll = true };
    public static Panel Bottom(int h) => new() { Dock = DockStyle.Bottom, Height = h, BackColor = Color.White, AutoScroll = true };
    public static ComboBox Combo(int x, int y, int width = 200) =>
        new() { Left = x, Top = y, Width = width, DropDownStyle = ComboBoxStyle.DropDownList };
    public static NumericUpDown Number(int x, int y, int width = 150, int decimals = 0, decimal maximum = 9999999) =>
        new() { Left = x, Top = y, Width = width, Minimum = 0, Maximum = maximum,
            DecimalPlaces = decimals, ThousandsSeparator = true };
    public static void Error(Exception ex)
    {
        // Mostrar contexto y pila: el texto anterior ocultaba el origen real del error.
        var details = ex.ToString();
        var log = "";
        try
        {
            var folder = System.IO.Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SistemaFacturacionWinForms");
            System.IO.Directory.CreateDirectory(folder);
            log = System.IO.Path.Combine(folder, "errores.log");
            System.IO.File.AppendAllText(log,
                $"\n===== {DateTime.Now:yyyy-MM-dd HH:mm:ss} =====\n{details}\n");
        }
        catch { /* Si no se puede escribir el log, igual mostraremos el error. */ }

        using var dialog = new Form();
        Configure(dialog, "Detalles del error - Sistema de facturación", 850, 530);
        dialog.MinimumSize = new Size(650, 400);
        var notice = new Label
        {
            Dock = DockStyle.Top, Height = 105, Padding = new Padding(12),
            Text = "Ocurrió un problema. Copia estos detalles para identificar la línea exacta. " +
                "Antes de repetir una venta, revisa Historial para comprobar si se guardó.\n\n" +
                (log.Length > 0 ? $"Archivo de diagnóstico: {log}" : "No se pudo guardar el archivo de diagnóstico.")
        };
        var box = new TextBox
        {
            Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both,
            WordWrap = false, Dock = DockStyle.Fill, Text = details,
            Font = new Font("Consolas", 9f)
        };
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 56 };
        var copy = Button("Copiar detalles", 12, 10, 180);
        copy.Click += (_, _) =>
        {
            try { Clipboard.SetText(details); Info("Detalles copiados. Puedes enviarlos para revisión."); }
            catch { Info("No se pudo copiar. Revisa el archivo de diagnóstico."); }
        };
        var close = Button("Cerrar", 206, 10, 125, false);
        close.Click += (_, _) => dialog.Close();
        footer.Controls.Add(copy); footer.Controls.Add(close);
        dialog.Controls.Add(box); dialog.Controls.Add(footer); dialog.Controls.Add(notice);
        dialog.ShowDialog();
    }
    public static void Info(string message) => MessageBox.Show(message, "Sistema de facturación", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static string Money(decimal amount) => $"RD$ {amount:N2}";
}
