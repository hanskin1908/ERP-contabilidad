namespace Application.Dtos;

public record IncomeStatementDto(DateOnly From, DateOnly To, decimal Ingresos, decimal Costos, decimal Gastos, decimal Utilidad);

public record BalanceSheetDto(DateOnly AsOf, decimal Activos, decimal Pasivos, decimal Patrimonio);

public record TrialBalanceRowDto(string AccountCode, string AccountName, decimal Debits, decimal Credits, decimal Balance);

public record JournalRowDto(DateOnly Date, long Number, string Type, string AccountCode, string AccountName, string? Description, string? Category, string? ThirdName, decimal Debit, decimal Credit);

public record LedgerRowDto(DateOnly Date, long Number, string Type, string AccountCode, string AccountName, string? Description, string? Category, string? ThirdName, decimal Debit, decimal Credit, decimal RunningBalance);
