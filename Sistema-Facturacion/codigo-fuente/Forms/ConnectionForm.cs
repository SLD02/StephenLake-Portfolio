using System.Windows.Forms;
using SistemaFacturacionWinForms.Services;
using SistemaFacturacionWinForms.Data;

namespace SistemaFacturacionWinForms.Forms;

public class ConnectionForm : Form
{
    private readonly TextBox _server = Ui.TextBox(190, 72, 310);
    private readonly TextBox _database = Ui.TextBox(190, 124, 310);
    public DatabaseSettings? Settings { get; private set; }

    public ConnectionForm(DatabaseSettings? current = null)
    {
        Ui.Configure(this, "Conectar con SQL Server", 560, 340);
        MinimumSize = MaximumSize = new System.Drawing.Size(560, 340);
        Controls.Add(Ui.Label("Servidor SQL:", 30, 73, 145));
        Controls.Add(_server);
        Controls.Add(Ui.Label("Base de datos:", 30, 125, 145));
        Controls.Add(_database);
        _server.Text = current?.Server ?? @".\SQLEXPRESS";
        _database.Text = current?.Database ?? "FacturacionTurnosDB";
        var tip = new Label
        {
            Left = 30, Top = 15, Width = 480, Height = 48,
            Text = "Autenticación de Windows. Si usas LocalDB: (localdb)\\MSSQLLocalDB\nLa base de datos se crea automáticamente si tienes permiso."
        };
        Controls.Add(tip);
        var btn = Ui.Button("Conectar y continuar", 185, 190, 220);
        btn.Click += (_, _) => Connect();
        Controls.Add(btn);
        AcceptButton = btn;
    }
    private void Connect()
    {
        try
        {
            string name = _database.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^[A-Za-z][A-Za-z0-9_]{0,60}$"))
                throw new InvalidOperationException("Usa un nombre de base de datos sin espacios, de 1 a 61 caracteres.");
            if (string.IsNullOrWhiteSpace(_server.Text)) throw new InvalidOperationException("Indica el servidor SQL.");
            var settings = new DatabaseSettings { Server = _server.Text.Trim(), Database = name };
            using var db = new AppDbContext(settings.ConnectionString);
            db.Database.EnsureCreated();
            if (!db.InvoiceSequences.Any())
            {
                db.InvoiceSequences.Add(new Models.InvoiceSequence { Id = 1, NextNumber = 1 });
                db.SaveChanges();
            }
            settings.Save();
            Settings = settings;
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex) { Ui.Error(ex); }
    }
}
