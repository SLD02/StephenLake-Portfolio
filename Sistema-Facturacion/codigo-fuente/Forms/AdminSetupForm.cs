using System.Windows.Forms;
using SistemaFacturacionWinForms.Data;
using SistemaFacturacionWinForms.Models;
using SistemaFacturacionWinForms.Services;

namespace SistemaFacturacionWinForms.Forms;

public class AdminSetupForm : Form
{
    private readonly TextBox _name = Ui.TextBox(180, 75, 280);
    private readonly TextBox _user = Ui.TextBox(180, 125, 280);
    private readonly TextBox _pass = Ui.TextBox(180, 175, 280, true);
    private readonly string _connection;
    public AdminSetupForm(string connection)
    {
        _connection = connection;
        Ui.Configure(this, "Primer inicio: crear administrador", 550, 330);
        MinimumSize = MaximumSize = new System.Drawing.Size(550, 330);
        Controls.Add(Ui.Label("Administrador:", 28, 75, 145));
        Controls.Add(Ui.Label("Usuario:", 28, 125, 145));
        Controls.Add(Ui.Label("Contraseña:", 28, 175, 145));
        Controls.Add(_name); Controls.Add(_user); Controls.Add(_pass);
        Controls.Add(new Label { Left = 28, Top = 20, Width = 465, Height = 37,
            Text = "Crea las credenciales iniciales. No hay contraseña por defecto." });
        var save = Ui.Button("Crear administrador", 180, 225, 220);
        save.Click += (_, _) => Save(); Controls.Add(save); AcceptButton = save;
    }
    private void Save()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_name.Text) || string.IsNullOrWhiteSpace(_user.Text))
                throw new InvalidOperationException("Completa el nombre y usuario.");
            using var db = new AppDbContext(_connection);
            if (db.Users.Any()) throw new InvalidOperationException("Ya se configuró un administrador.");
            var admin = new User { FullName = _name.Text.Trim(), Username = _user.Text.Trim(),
                Role = "Administrador", Shift = "Administración" };
            Security.SetPassword(admin, _pass.Text);
            db.Users.Add(admin); db.SaveChanges();
            Ui.Info("Administrador creado. Guarda tus credenciales.");
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { Ui.Error(ex); }
    }
}
