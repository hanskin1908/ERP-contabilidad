namespace Application.Dtos;

public record AccountDto(
    long Id,
    string Code,
    string Name,
    short Level,
    char Nature,
    long? ParentId,
    bool IsPostable,
    bool IsActive
);

public class CreateAccountRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public short Level { get; set; }
    public char Nature { get; set; }
    public long? ParentId { get; set; }
    public bool IsPostable { get; set; }
}

public class UpdateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public short Level { get; set; }
    public char Nature { get; set; }
    public long? ParentId { get; set; }
    public bool IsPostable { get; set; }
    public bool IsActive { get; set; }
}

