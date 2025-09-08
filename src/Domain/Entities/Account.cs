namespace Domain.Entities;

public class Account
{
    public long Id { get; set; }
    public long CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short Level { get; set; }
    public char Nature { get; set; } // 'D' o 'C'
    public long? ParentId { get; set; }
    public bool IsPostable { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

