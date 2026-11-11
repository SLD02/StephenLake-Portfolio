using System.Windows.Forms;
using SistemaFacturacionWinForms.Data;
using SistemaFacturacionWinForms.Models;
using SistemaFacturacionWinForms.Services;

namespace SistemaFacturacionWinForms.Forms;

public class LoginForm : Form
{
    private readonly TextBox _user = Ui.TextBox(155, 95, 235);
    private readonly TextBox _pass = Ui.TextBox(155, 145, 235, true);
    private readonly string _connection;
    public User? CurrentUser { get; private set; }

    public LoginForm(string connection)
    {
        _connection = connection;
        Ui.Configure(this, "Acceso - Sistema de facturación", 470, 340);
        MinimumSize = MaximumSize = new System.Drawing.Size(470, 340);
        Controls.Add(new Label { Text = "FACTURACIÓN | ACCESO", Left = 48, Top = 28,
            Width = 370, Height = 38, Font = new System.Drawing.Font("Segoe UI", 17, System.Drawing.FontStyle.Bold),
            ForeColor = Ui.Navy });
        Controls.Add(Ui.Label("Usuario:", 44, 97, 100));
        Controls.Add(Ui.Label("Contraseña:", 44, 147, 105));
        Controls.Add(_user); Controls.Add(_pass);
        var enter = Ui.Button("Iniciar sesión", 155, 205, 235);
        enter.Click += (_, _) => Login();
        Controls.Add(enter); AcceptButton = enter;
        var config = Ui.Button("Cambiar servidor SQL", 155, 252, 235, false);
        config.Click += (_, _) => { DialogResult = DialogResult.Retry; Close(); };
        Controls.Add(config);
    }
    private void Login()
    {
        try
        {
            using var db = new AppDbContext(_connection);
            var user = db.Users.SingleOrDefault(x => x.Username == _user.Text.Trim() && x.IsActive);
            if (user is null || !Security.Verify(user, _pass.Text))
                throw new InvalidOperationException("Credenciales incorrectas o cuenta desactivada.");
            CurrentUser = user;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { Ui.Error(ex); }
    }
}
