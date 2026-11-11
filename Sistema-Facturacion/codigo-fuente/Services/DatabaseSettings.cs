using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace SistemaFacturacionWinForms.Services;

public class DatabaseSettings
{
    public string Server { get; set; } = @".\SQLEXPRESS";
    public string Database { get; set; } = "FacturacionTurnosDB";

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SistemaFacturacionTurnos", "dbconfig.json");

    public string ConnectionString => new SqlConnectionStringBuilder
    {
        DataSource = Server, InitialCatalog = Database,
        IntegratedSecurity = true, TrustServerCertificate = true, ConnectTimeout = 6,
        MultipleActiveResultSets = true
    }.ConnectionString;

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
    }

    public static DatabaseSettings? Load()
    {
        try { return File.Exists(FilePath) ? JsonSerializer.Deserialize<DatabaseSettings>(File.ReadAllText(FilePath)) : null; }
        catch { return null; }
    }
}
