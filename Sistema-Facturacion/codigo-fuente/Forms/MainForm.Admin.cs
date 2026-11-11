using System.Windows.Forms;
using Microsoft.EntityFrameworkCore;
using SistemaFacturacionWinForms.Data;
using SistemaFacturacionWinForms.Models;
using SistemaFacturacionWinForms.Services;

namespace SistemaFacturacionWinForms.Forms;

public partial class MainForm
{
    private TabPage BuildUsers()
    {
        var page = new TabPage("Cajeros") { BackColor = Ui.Surface };
        var top = Ui.Top(185);
        top.Controls.Add(new Label { Text = "CAJEROS Y TURNOS", Left = 15, Top = 8, Width = 400,
            Height = 30, Font = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold) });
        var name = Ui.TextBox(110, 48, 240); var username = Ui.TextBox(430, 48, 150);
        var pass = Ui.TextBox(680, 48, 170, true); var shift = Ui.Combo(950, 48, 145);
        shift.Items.AddRange(new object[] { "Mañana", "Tarde", "Noche" }); shift.SelectedIndex = 0;
        top.Controls.Add(Ui.Label("Nombre:", 15, 50, 90)); top.Controls.Add(name);
        top.Controls.Add(Ui.Label("Usuario:", 365, 50, 65)); top.Controls.Add(username);
        top.Controls.Add(Ui.Label("Clave:", 615, 50, 65)); top.Controls.Add(pass);
        top.Controls.Add(Ui.Label("Turno:", 890, 50, 60)); top.Controls.Add(shift);
        var save = Ui.Button("Crear cajero", 18, 101, 160);
        save.Click += (_, _) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(username.Text))
                    throw new InvalidOperationException("Completa nombre y usuario.");
                using var db = new AppDbContext(_connection);
                if (db.Users.Any(x => x.Username == username.Text.Trim())) throw new InvalidOperationException("Este usuario ya existe.");
                var cashier = new User { FullName = name.Text.Trim(), Username = username.Text.Trim(),
                    Shift = shift.Text, Role = "Cajero" };
                Security.SetPassword(cashier, pass.Text);
                db.Users.Add(cashier); db.SaveChanges();
                name.Clear(); username.Clear(); pass.Clear(); LoadUsers();
            }
            catch (Exception ex) { Ui.Error(ex); }
        };
        top.Controls.Add(save);
        var toggle = Ui.Button("Activar / desactivar", 192, 101, 204, false);
        toggle.Click += (_, _) =>
        {
            try
            {
                int? id = SelectedId(_usersGrid);
                if (id == null) return;
                using var db = new AppDbContext(_connection);
                var selected = db.Users.Single(x => x.Id == id);
                if (selected.Role == "Administrador") throw new InvalidOperationException("No puedes desactivar al administrador desde esta pantalla.");
                selected.IsActive = !selected.IsActive; db.SaveChanges(); LoadUsers();
            }
            catch (Exception ex) { Ui.Error(ex); }
        }; top.Controls.Add(toggle);
        var passwordReset = Ui.Button("Cambiar clave seleccionada", 410, 101, 230, false);
        passwordReset.Click += (_, _) =>
        {
            try
            {
                int? id = SelectedId(_usersGrid);
                if (id == null) throw new InvalidOperationException("Selecciona un cajero.");
                if (string.IsNullOrWhiteSpace(pass.Text)) throw new InvalidOperationException("Escribe la nueva contraseña en Clave.");
                using var db = new AppDbContext(_connection);
                var selected = db.Users.Single(x => x.Id == id);
                if (selected.Role == "Administrador") throw new InvalidOperationException("Esta pantalla solo cambia claves de cajeros.");
                Security.SetPassword(selected, pass.Text); db.SaveChanges(); pass.Clear();
                Ui.Info("Contraseña actualizada.");
            }
            catch (Exception ex) { Ui.Error(ex); }
        }; top.Controls.Add(passwordReset);
        top.Controls.Add(new Label { Left = 15, Top = 151, Width = 930, Height = 25,
            Text = "Selecciona un cajero de la tabla para activar/desactivar o actualizar su contraseña." });
        page.Controls.Add(_usersGrid); page.Controls.Add(top);
        return page;
    }

    private void LoadUsers()
    {
        using var db = new AppDbContext(_connection);
        _usersGrid.DataSource = db.Users.AsNoTracking().OrderBy(x => x.Role).ThenBy(x => x.FullName).ToList()
            .Select(x => new { Id = x.Id, Nombre = x.FullName, Usuario = x.Username,
                Rol = x.Role, Turno = x.Shift, Activo = x.IsActive ? "Sí" : "No" }).ToList();
        if (_usersGrid.Columns.Contains("Id")) _usersGrid.Columns["Id"].Visible = false;
    }

    private TabPage BuildProducts()
    {
        var page = new TabPage("Productos") { BackColor = Ui.Surface };
        var top = Ui.Top(160);
        top.Controls.Add(new Label { Text = "INVENTARIO DE PRODUCTOS", Left = 18, Top = 9, Width = 530,
            Height = 31, Font = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold) });
        var code = Ui.TextBox(95, 53, 125); var name = Ui.TextBox(315, 53, 290);
        var price = Ui.Number(690, 53, 145, 2, 99999999);
        var stock = Ui.Number(935, 53, 140);
        top.Controls.Add(Ui.Label("Código:", 17, 55, 75)); top.Controls.Add(code);
        top.Controls.Add(Ui.Label("Nombre:", 239, 55, 75)); top.Controls.Add(name);
        top.Controls.Add(Ui.Label("Precio RD$:", 610, 55, 80)); top.Controls.Add(price);
        top.Controls.Add(Ui.Label("Stock:", 868, 55, 62)); top.Controls.Add(stock);
        var add = Ui.Button("Agregar producto", 18, 102, 183);
        add.Click += (_, _) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(code.Text) || string.IsNullOrWhiteSpace(name.Text))
                    throw new InvalidOperationException("Código y nombre son obligatorios.");
                if (price.Value <= 0) throw new InvalidOperationException("El precio debe ser mayor que cero.");
                using var db = new AppDbContext(_connection);
                db.Products.Add(new Product { Code = code.Text.Trim(), Name = name.Text.Trim(),
                    UnitPrice = price.Value, Stock = (int)stock.Value });
                db.SaveChanges(); code.Clear(); name.Clear(); price.Value = 0; stock.Value = 0; LoadProducts();
            }
            catch (Exception ex) { Ui.Error(ex); }
        }; top.Controls.Add(add);
        var edit = Ui.Button("Actualizar seleccionado", 220, 102, 219, false);
        edit.Click += (_, _) =>
        {
            try
            {
                int? id = SelectedId(_productsGrid);
                if (id == null) throw new InvalidOperationException("Selecciona un producto en la tabla.");
                if (string.IsNullOrWhiteSpace(code.Text) || string.IsNullOrWhiteSpace(name.Text) || price.Value <= 0)
                    throw new InvalidOperationException("Completa código, nombre y precio válido.");
                using var db = new AppDbContext(_connection);
                var product = db.Products.Single(p => p.Id == id);
                product.Code = code.Text.Trim(); product.Name = name.Text.Trim();
                product.UnitPrice = price.Value; product.Stock = (int)stock.Value;
                db.SaveChanges(); LoadProducts(); LoadSaleOptions();
            }
            catch (Exception ex) { Ui.Error(ex); }
        }; top.Controls.Add(edit);
        var toggle = Ui.Button("Activar / desactivar", 457, 102, 197, false);
        toggle.Click += (_, _) =>
        {
            try
            {
                int? id = SelectedId(_productsGrid);
                if (id == null) return;
                using var db = new AppDbContext(_connection);
                var selected = db.Products.Single(p => p.Id == id);
                selected.IsActive = !selected.IsActive; db.SaveChanges(); LoadProducts();
            }
            catch (Exception ex) { Ui.Error(ex); }
        }; top.Controls.Add(toggle);
        _productsGrid.SelectionChanged += (_, _) =>
        {
            if (_productsGrid.SelectedRows.Count < 1) return;
            var row = _productsGrid.SelectedRows[0];
            if (!_productsGrid.Columns.Contains("Código") || row.Cells["Código"].Value is not string) return;
            code.Text = row.Cells["Código"].Value?.ToString();
            name.Text = row.Cells["Nombre"].Value?.ToString();
            price.Value = Math.Clamp(Convert.ToDecimal(row.Cells["Precio"].Value), price.Minimum, price.Maximum);
            stock.Value = Math.Clamp(Convert.ToDecimal(row.Cells["Existencia"].Value), stock.Minimum, stock.Maximum);
        };
        page.Controls.Add(_productsGrid); page.Controls.Add(top);
        return page;
    }

    private void LoadProducts()
    {
        using var db = new AppDbContext(_connection);
        _productsGrid.DataSource = db.Products.AsNoTracking().OrderBy(x => x.Name).ToList()
            .Select(x => new { Id = x.Id, Código = x.Code, Nombre = x.Name,
                Precio = x.UnitPrice, Existencia = x.Stock, Activo = x.IsActive ? "Sí" : "No" }).ToList();
        if (_productsGrid.Columns.Contains("Id")) _productsGrid.Columns["Id"].Visible = false;
    }

    private TabPage BuildCustomers()
    {
        var page = new TabPage("Clientes") { BackColor = Ui.Surface };
        var top = Ui.Top(155);
        top.Controls.Add(new Label { Text = "CLIENTES", Left = 18, Top = 10, Width = 400,
            Height = 31, Font = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold) });
        var name = Ui.TextBox(105, 53, 310); var phone = Ui.TextBox(505, 53, 190);
        var email = Ui.TextBox(780, 53, 260);
        top.Controls.Add(Ui.Label("Nombre:", 18, 54, 85)); top.Controls.Add(name);
        top.Controls.Add(Ui.Label("Teléfono:", 425, 54, 80)); top.Controls.Add(phone);
        top.Controls.Add(Ui.Label("Correo:", 715, 54, 66)); top.Controls.Add(email);
        var add = Ui.Button("Registrar cliente", 18, 102, 185);
        add.Click += (_, _) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name.Text)) throw new InvalidOperationException("Escribe el nombre del cliente.");
                using var db = new AppDbContext(_connection);
                db.Customers.Add(new Customer { Name = name.Text.Trim(), Phone = phone.Text.Trim(), Email = email.Text.Trim() });
                db.SaveChanges(); name.Clear(); phone.Clear(); email.Clear(); LoadCustomers();
            }
            catch (Exception ex) { Ui.Error(ex); }
        }; top.Controls.Add(add);
        var update = Ui.Button("Actualizar seleccionado", 225, 102, 230, false);
        update.Click += (_, _) =>
        {
            try
            {
                int? id = SelectedId(_customersGrid);
                if (id == null) throw new InvalidOperationException("Selecciona un cliente.");
                if (string.IsNullOrWhiteSpace(name.Text)) throw new InvalidOperationException("Escribe el nombre del cliente.");
                using var db = new AppDbContext(_connection);
                var selected = db.Customers.Single(c => c.Id == id);
                selected.Name = name.Text.Trim(); selected.Phone = phone.Text.Trim(); selected.Email = email.Text.Trim();
                db.SaveChanges(); LoadCustomers();
            }
            catch (Exception ex) { Ui.Error(ex); }
        }; top.Controls.Add(update);
        _customersGrid.SelectionChanged += (_, _) =>
        {
            if (_customersGrid.SelectedRows.Count < 1) return;
            var row = _customersGrid.SelectedRows[0];
            if (!_customersGrid.Columns.Contains("Nombre") || row.Cells["Nombre"].Value is not string) return;
            name.Text = row.Cells["Nombre"].Value.ToString();
            phone.Text = row.Cells["Teléfono"].Value?.ToString();
            email.Text = row.Cells["Correo"].Value?.ToString();
        };
        page.Controls.Add(_customersGrid); page.Controls.Add(top);
        return page;
    }

    private void LoadCustomers()
    {
        using var db = new AppDbContext(_connection);
        _customersGrid.DataSource = db.Customers.AsNoTracking().OrderBy(x => x.Name).ToList()
            .Select(x => new { Id = x.Id, Nombre = x.Name, Teléfono = x.Phone, Correo = x.Email }).ToList();
        if (_customersGrid.Columns.Contains("Id")) _customersGrid.Columns["Id"].Visible = false;
    }

    private TabPage BuildReceivables()
    {
        var page = new TabPage("Cuentas por cobrar") { BackColor = Ui.Surface };
        var top = Ui.Top(110);
        top.Controls.Add(new Label { Text = "CUENTAS POR COBRAR", Left = 15, Top = 11,
            Width = 430, Height = 32, Font = new System.Drawing.Font("Segoe UI", 15, System.Drawing.FontStyle.Bold) });
        top.Controls.Add(Ui.Label("Abono RD$:", 18, 64, 110));
        var amount = Ui.Number(128, 61, 190, 2, 99999999);
        top.Controls.Add(amount);
        var collect = Ui.Button("Registrar abono", 343, 57, 190);
        collect.Click += (_, _) =>
        {
            try
            {
                int? id = SelectedId(_receivablesGrid);
                if (id == null) throw new InvalidOperationException("Selecciona una cuenta por cobrar.");
                BillingService.ReceivePayment(_connection, id.Value, _user.Id, amount.Value);
                amount.Value = 0; LoadReceivables();
                Ui.Info("Abono registrado.");
            }
            catch (Exception ex) { Ui.Error(ex); }
        };
        top.Controls.Add(collect);
        page.Controls.Add(_receivablesGrid); page.Controls.Add(top);
        return page;
    }

    private void LoadReceivables()
    {
        using var db = new AppDbContext(_connection);
        _receivablesGrid.DataSource = db.Receivables.AsNoTracking()
            .Include(x => x.Customer).Include(x => x.Invoice).OrderByDescending(x => x.Balance)
            .ToList().Select(x => new
            {
                Id = x.Id, Factura = x.Invoice.Number, Cliente = x.Customer.Name,
                Fecha = x.Invoice.IssuedAt, Original = x.OriginalAmount,
                Pendiente = x.Balance, Estado = x.Balance == 0 ? "Pagada" : "Pendiente"
            }).ToList();
        if (_receivablesGrid.Columns.Contains("Id")) _receivablesGrid.Columns["Id"].Visible = false;
    }
}
