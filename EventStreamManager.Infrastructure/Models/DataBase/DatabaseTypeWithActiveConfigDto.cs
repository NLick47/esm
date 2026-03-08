namespace EventStreamManager.Infrastructure.Models.DataBase;

public class DatabaseTypeWithActiveConfigDto
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DatabaseConfig? ActiveConfig { get; set; }
}