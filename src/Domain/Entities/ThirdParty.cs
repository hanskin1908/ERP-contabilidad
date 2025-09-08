namespace Domain.Entities;

public class ThirdParty
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string Nit { get; set; } = string.Empty;
    public string? Dv { get; set; }
    public string Tipo { get; set; } = "otro"; // cliente|proveedor|ambos|otro
    public string RazonSocial { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

