using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeding;

public static class PucSeeder
{
    private record Row(string Code, string Name, short Level, char Nature, string? ParentCode, bool IsPostable);

    public static async Task SeedAsync(ApplicationDbContext db, long companyId, CancellationToken ct)
    {
        if (await db.Accounts.AnyAsync(a => a.CompanyId == companyId, ct)) return;
        var rows = LoadRows();

        var codeToId = new Dictionary<string, long>();
        foreach (var r in rows)
        {
            long? parentId = null;
            if (!string.IsNullOrWhiteSpace(r.ParentCode))
            {
                if (!codeToId.TryGetValue(r.ParentCode!, out var pid))
                {
                    throw new InvalidOperationException($"PUC: parent {r.ParentCode} not found for {r.Code}");
                }
                parentId = pid;
            }

            var acc = new Account
            {
                CompanyId = companyId,
                Code = r.Code,
                Name = r.Name,
                Level = r.Level,
                Nature = r.Nature,
                ParentId = parentId,
                IsPostable = r.IsPostable,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            db.Accounts.Add(acc);
            await db.SaveChangesAsync(ct);
            codeToId[r.Code] = acc.Id;
        }
    }

    private static List<Row> LoadRows()
    {
        // Semilla PUC base (clases 1–9) + cuentas comunes. Extensible por CSV futuro.
        var L = new List<Row>();
        // Clases
        L.Add(new("1", "ACTIVO", 1, 'D', null, false));
        L.Add(new("2", "PASIVO", 1, 'C', null, false));
        L.Add(new("3", "PATRIMONIO", 1, 'C', null, false));
        L.Add(new("4", "INGRESOS", 1, 'C', null, false));
        L.Add(new("5", "GASTOS", 1, 'D', null, false));
        L.Add(new("6", "COSTO DE VENTAS", 1, 'D', null, false));
        L.Add(new("7", "COSTOS DE PRODUCCIÓN", 1, 'D', null, false));
        L.Add(new("8", "CUENTAS DE ORDEN DEUDORAS", 1, 'D', null, false));
        L.Add(new("9", "CUENTAS DE ORDEN ACREEDORAS", 1, 'C', null, false));

        // Grupos y cuentas comunes (selección representativa)
        L.Add(new("11", "Disponible", 2, 'D', "1", false));
        L.Add(new("1105", "Caja", 3, 'D', "11", true));
        L.Add(new("1110", "Bancos", 3, 'D', "11", true));
        L.Add(new("12", "Deudores", 2, 'D', "1", false));
        L.Add(new("1305", "Clientes", 3, 'D', "12", true));
        L.Add(new("13", "Inventarios", 2, 'D', "1", false));
        L.Add(new("1435", "Mercancías no fabricadas", 3, 'D', "13", true));
        L.Add(new("14", "Propiedad, planta y equipo", 2, 'D', "1", false));
        L.Add(new("1524", "Equipo de oficina", 3, 'D', "14", true));

        L.Add(new("21", "Obligaciones financieras", 2, 'C', "2", false));
        L.Add(new("2105", "Bancos nacionales", 3, 'C', "21", true));
        L.Add(new("22", "Proveedores", 2, 'C', "2", false));
        L.Add(new("2205", "Proveedores nacionales", 3, 'C', "22", true));
        L.Add(new("24", "Impuestos, gravámenes y tasas", 2, 'C', "2", false));
        L.Add(new("2408", "IVA por pagar", 3, 'C', "24", true));

        L.Add(new("31", "Capital social", 2, 'C', "3", false));
        L.Add(new("3105", "Capital suscrito y pagado", 3, 'C', "31", true));
        L.Add(new("36", "Resultados del ejercicio", 2, 'C', "3", false));
        L.Add(new("3605", "Utilidad del ejercicio", 3, 'C', "36", true));

        L.Add(new("41", "Operacionales", 2, 'C', "4", false));
        L.Add(new("4135", "Ingresos operacionales", 3, 'C', "41", true));

        L.Add(new("51", "Gastos de administración", 2, 'D', "5", false));
        L.Add(new("5105", "Gastos de personal", 3, 'D', "51", true));
        L.Add(new("5135", "Gastos generales", 3, 'D', "51", true));
        L.Add(new("52", "Gastos de ventas", 2, 'D', "5", false));
        L.Add(new("5205", "Publicidad y mercadeo", 3, 'D', "52", true));

        L.Add(new("61", "Costo de ventas", 2, 'D', "6", false));
        L.Add(new("6135", "Costo mercancías vendidas", 3, 'D', "61", true));

        // Algunas cuentas de orden (referenciales)
        L.Add(new("81", "Deudoras fiscales", 2, 'D', "8", false));
        L.Add(new("9105", "Acreedoras contingentes", 2, 'C', "9", false));

        return L;
    }
}

