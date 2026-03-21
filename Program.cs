using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true)
    .AddEnvironmentVariables()
    .Build();

// --- Configuración ---
// Ajustá estos valores a tu entorno
string connectionString = config.GetSection("ConnectionStrings")["Default"];

if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("Error: No se pudo cargar la cadena de conexión desde appsettings.json");
    return;
}

// --- Validar parámetro ---
if (args.Length == 0 || !int.TryParse(args[0], out int legajo))
{
    Console.WriteLine("Uso: ConsultaCtaCte <legajo>");
    Console.WriteLine("  Ejemplo: ConsultaCtaCte 1234");
    return;
}

// --- Consulta SQL (la lógica de tu SP, embebida) ---
string query = @"
SELECT  CL.RS,
        'Limite'       = CL.LimiteCredito1,
        'Emision'      = CC.fecha_emision,
        'Vencimiento'  = CC.fecha_venc,
        CC.comprobante,
        observaciones  = LTRIM(RTRIM(CC.observaciones)),
        CC.monto,
        CC.Saldo,
        TC.cc,
        'afecta'       = ISNULL(CC.no_Cuenta_Limite, 0)
FROM ctacte CC
INNER JOIN TC TC ON CC.tipo = TC.codigo
INNER JOIN Clientes CL ON CC.Cliente = CL.Codigo
WHERE CC.cliente = @Leg
  AND CC.presupuesto = 0
  AND CL.GRUPO = 'RRHH'
ORDER BY CC.Fecha_Emision DESC";

// --- Ejecución y formateo ---
try
{
    using SqlConnection conn = new(connectionString);
    conn.Open();

    using SqlCommand cmd = new(query, conn);
    cmd.Parameters.AddWithValue("@Leg", legajo);

    using SqlDataReader reader = cmd.ExecuteReader();

    if (!reader.HasRows)
    {
        Console.WriteLine($"No se encontraron registros para el legajo {legajo}.");
        return;
    }

    // Leer todos los datos primero para calcular anchos de columna
    int fieldCount = reader.FieldCount;
    string[] headers = new string[fieldCount];
    for (int i = 0; i < fieldCount; i++)
        headers[i] = reader.GetName(i);

    // Almacenar filas en lista
    List<string[]> rows = new();
    while (reader.Read())
    {
        string[] row = new string[fieldCount];
        for (int i = 0; i < fieldCount; i++)
        {
            object val = reader.GetValue(i);
            row[i] = val == DBNull.Value ? "(null)" : FormatValue(val);
        }
        rows.Add(row);
    }

    // Calcular ancho óptimo por columna
    int[] widths = new int[fieldCount];
    for (int i = 0; i < fieldCount; i++)
    {
        widths[i] = headers[i].Length;
        foreach (var row in rows)
        {
            if (row[i].Length > widths[i])
                widths[i] = row[i].Length;
        }
    }

    // Imprimir encabezado
    Console.WriteLine();
    PrintRow(headers, widths);
    PrintSeparator(widths);

    // Imprimir datos
    foreach (var row in rows)
        PrintRow(row, widths);

    Console.WriteLine($"\n({rows.Count} fila{(rows.Count != 1 ? "s" : "")})");
}
catch (SqlException ex)
{
    Console.WriteLine($"Error SQL: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}

// --- Funciones auxiliares ---

static string FormatValue(object val) => val switch
{
    DateTime dt => dt.ToString("dd/MM/yyyy"),
    decimal d => d.ToString("N2"),
    double d => d.ToString("N2"),
    _ => val.ToString() ?? ""
};

static void PrintRow(string[] values, int[] widths)
{
    for (int i = 0; i < values.Length; i++)
    {
        if (i > 0) Console.Write(" | ");
        Console.Write(values[i].PadRight(widths[i]));
    }
    Console.WriteLine();
}

static void PrintSeparator(int[] widths)
{
    for (int i = 0; i < widths.Length; i++)
    {
        if (i > 0) Console.Write("-+-");
        Console.Write(new string('-', widths[i]));
    }
    Console.WriteLine();
}
