namespace Application.Dtos;

public record ThirdPartyDto(
    long Id,
    string Nit,
    string? Dv,
    string Tipo,
    string RazonSocial,
    string? Direccion,
    string? Email,
    string? Telefono,
    bool IsActive
);

public class CreateThirdPartyRequest
{
    public string Nit { get; set; } = string.Empty;
    public string? Dv { get; set; }
    public string Tipo { get; set; } = "otro"; // cliente|proveedor|ambos|otro
    public string RazonSocial { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
}

public class UpdateThirdPartyRequest
{
    public string Tipo { get; set; } = "otro";
    public string RazonSocial { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public bool IsActive { get; set; } = true;
}

