namespace Domain.Entities;

public class Company
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Nit { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

