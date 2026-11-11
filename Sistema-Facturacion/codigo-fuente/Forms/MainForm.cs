using System.Windows.Forms;
using SistemaFacturacionWinForms.Data;
using SistemaFacturacionWinForms.Models;
using SistemaFacturacionWinForms.Services;

namespace SistemaFacturacionWinForms.Forms;

public partial class MainForm : Form
{
    private readonly string _connection;
    private readonly User _user;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill, Font = new System.Drawing.Font("Segoe UI", 10) };

    private readonly List<CartLine> _cart = new();
    private readonly ComboBox _saleProduct = Ui.Combo(110, 58, 365);
    private readonly ComboBox _saleCustomer = Ui.Combo(110, 105, 365);
    private readonly ComboBox _saleMethod = Ui.Combo(610, 105, 190);
    private readonly NumericUpDown _saleQuantity = Ui.Number(610, 58, 100);
    private readonly NumericUpDown _salePaid = Ui.Number(875, 105, 155, 2, 99999999);
    private readonly Label _saleTotals = Ui.Label("", 10, 14, 900);
    private readonly DataGridView _saleGrid = Ui.Grid();

    private readonly DataGridView _usersGrid = Ui.Grid();
    private readonly DataGridView _productsGrid = Ui.Grid();
    private readonly DataGridView _customersGrid = Ui.Grid();
    private readonly DataGridView _receivablesGrid = Ui.Grid();
    private readonly DataGridView _historyGrid = Ui.Grid();
    private readonly DataGridView _reportGrid = Ui.Grid();
    private readonly ComboBox _reportType = Ui.Combo(105, 20, 270);
    private readonly DateTimePicker _reportDate = new() { Left = 500, Top = 20, Width = 175, Format = DateTimePickerFormat.Short };
    private readonly Label _reportSummary = Ui.Label("", 15, 75, 1030);
    private readonly DateTimePicker _historyDate = new() { Left = 110, Top = 21, Width = 170, Format = DateTimePickerFormat.Short };

    private record Option<T>(T Value, string Label)
    {
        public override string ToString() => Label;
    }

    public MainForm(string connection, User user)
    {
        _connection = connection; _user = user;
        Ui.Configure(this, "Sistema de Facturación | Cajeros y Turnos", 1250, 790);
        WindowState = FormWindowState.Maximized;
        var header = new Panel { Dock = DockStyle.Top, Height = 74, BackColor = Ui.Navy };
        header.Controls.Add(new Label
        {
            Text = "SISTEMA DE FACTURACIÓN", Left = 22, Top = 11, Width = 530, Height = 34,
            ForeColor = System.Drawing.Color.White,
            Font = new System.Drawing.Font("Segoe UI", 19, System.Drawing.FontStyle.Bold)
        });
        header.Controls.Add(new Label
        {
            Text = $"{_user.FullName} | {_user.Role} | Turno: {_user.Shift}", Left = 25, Top = 46,
            Width = 750, Height = 22, ForeColor = System.Drawing.Color.Gainsboro
        });
        var exit = Ui.Button("Cerrar sesión", 1040, 21, 155, false);
        exit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        exit.Click += (_, _) => Close();
        header.Controls.Add(exit);
        Controls.Add(_tabs); Controls.Add(header);
        _tabs.TabPages.Add(BuildSales());
        _tabs.TabPages.Add(BuildHistory());
        if (_user.Role == "Administrador")
        {
            _tabs.TabPages.Add(BuildUsers());
            _tabs.TabPages.Add(BuildProducts());
            _tabs.TabPages.Add(BuildCustomers());
            _tabs.TabPages.Add(BuildReceivables());
            _tabs.TabPages.Add(BuildReports());
        }
        _tabs.SelectedIndexChanged += (_, _) => RefreshSelectedTab();
        RefreshSelectedTab();
    }

    private void RefreshSelectedTab()
    {
        if (_tabs.SelectedTab == null) return;
        try
        {
            switch (_tabs.SelectedTab.Text)
            {
                case "Nueva venta": LoadSaleOptions(); break;
                case "Historial": LoadHistory(); break;
                case "Cajeros": LoadUsers(); break;
                case "Productos": LoadProducts(); break;
                case "Clientes": LoadCustomers(); break;
                case "Cuentas por cobrar": LoadReceivables(); break;
                case "Reportes": LoadReport(); break;
            }
        }
        catch (Exception ex) { Ui.Error(ex); }
    }

    private static int? SelectedId(DataGridView grid)
    {
        if (grid.SelectedRows.Count < 1 || !grid.Columns.Contains("Id")) return null;
        return Convert.ToInt32(grid.SelectedRows[0].Cells["Id"].Value);
    }
}
