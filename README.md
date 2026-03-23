# ConsultaCtaCte

Herramienta de línea de comandos en .NET 10 para consultar el estado de cuenta corriente de un empleado (legajo) contra una base de datos SQL Server.

## Descripción

Dado un número de legajo, la aplicación consulta la tabla `ctacte` y muestra en consola los comprobantes pendientes del cliente correspondiente al grupo `RRHH`, con información de fechas, montos, saldos y tipo de comprobante. El resultado se presenta en formato de tabla alineada.

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server accesible desde la máquina donde se ejecuta la herramienta
- Base de datos con las tablas `ctacte`, `TC` y `Clientes`

## Configuración

1. Copiar el archivo de ejemplo y completar con los datos reales:

```bash
cp appsettings.example.json appsettings.json
```

2. Editar `appsettings.json` con la cadena de conexión correcta:

```json
{
  "ConnectionStrings": {
    "Default": "Server=MI_SERVIDOR;Database=MI_BASE;User ID=MI_USUARIO;Password=MI_CLAVE;TrustServerCertificate=true;"
  }
}
```

> **Importante:** `appsettings.json` contiene credenciales y está excluido del repositorio. Nunca lo subas al control de versiones.

### Alternativa: variable de entorno

La cadena de conexión también puede configurarse mediante variable de entorno, sin necesidad del archivo `appsettings.json`:

```bash
export ConnectionStrings__Default="Server=MI_SERVIDOR;Database=MI_BASE;User ID=MI_USUARIO;Password=MI_CLAVE;TrustServerCertificate=true;"
```

## Uso

```bash
dotnet run -- <legajo>
```

O bien, compilado:

```bash
ConsultaCtaCte <legajo>
```

### Ejemplo

```
ConsultaCtaCte 1234
```

**Salida esperada:**

```
RS              | Limite   | Emision    | Vencimiento | comprobante | observaciones | monto    | Saldo    | cc  | afecta
----------------+----------+------------+-------------+-------------+---------------+----------+----------+-----+-------
GARCIA JUAN     | 5000,00  | 01/03/2026 | 31/03/2026  | FC-0001234  | Cuota marzo   | 1200,00  | 1200,00  | FC  | 0
...

(2 filas)
```

Si no se encuentran registros para el legajo indicado, se muestra un mensaje informativo.

## Estructura del proyecto

```
ConsultaCtaCte/
├── Program.cs                  # Lógica principal
├── ConsultaCtaCte.csproj       # Definición del proyecto
├── appsettings.example.json    # Plantilla de configuración (sin credenciales)
├── appsettings.json            # Configuración real (excluida del repo)
└── README.md
```

## Dependencias

| Paquete | Versión |
|---|---|
| Microsoft.Data.SqlClient | 6.1.4 |
| Microsoft.Extensions.Configuration | 10.0.5 |
| Microsoft.Extensions.Configuration.Json | 10.0.5 |
| Microsoft.Extensions.Configuration.EnvironmentVariables | 10.0.5 |

## Compilar y publicar

```bash
# Ejecutar directamente
dotnet run -- <legajo>

# Compilar
dotnet build

# Publicar ejecutable autocontenido (Windows x64)
dotnet publish -c Release -r win-x64 --self-contained
```
