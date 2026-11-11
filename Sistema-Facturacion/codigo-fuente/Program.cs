using System.Windows.Forms;
using SistemaFacturacionWinForms.Data;
using SistemaFacturacionWinForms.Forms;
using SistemaFacturacionWinForms.Models;
using SistemaFacturacionWinForms.Services;

namespace SistemaFacturacionWinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var settings = DatabaseSettings.Load();
        while (true)
        {
            try
            {
                if (settings is null)
                {
                    using var connection = new ConnectionForm();
                    if (connection.ShowDialog() != DialogResult.OK) return;
                    settings = connection.Settings!;
                }
                using (var db = new AppDbContext(settings.ConnectionString))
                {
                    db.Database.EnsureCreated();
                    if (!db.InvoiceSequences.Any())
                    {
                        db.InvoiceSequences.Add(new InvoiceSequence { Id = 1, NextNumber = 1 });
                        db.SaveChanges();
                    }
                    if (!db.Users.Any())
                    {
                        using var setup = new AdminSetupForm(settings.ConnectionString);
                        if (setup.ShowDialog() != DialogResult.OK) return;
                    }
                }
                using var login = new LoginForm(settings.ConnectionString);
                var result = login.ShowDialog();
                if (result == DialogResult.Retry) { settings = null; continue; }
                if (result != DialogResult.OK || login.CurrentUser == null) return;
                using var main = new MainForm(settings.ConnectionString, login.CurrentUser);
                Application.Run(main);
                // Al cerrar la ventana principal, regresa al acceso para otro cajero.
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión o base de datos:\n{ex.GetBaseException().Message}\n\nRevisa el servidor SQL.",
                    "SQL Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                settings = null;
            }
        }
    }
}
