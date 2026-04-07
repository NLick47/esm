namespace EventStreamManager.Infrastructure.Models.DataBase;

public static  class DatabaseTypeExtensions
{
    public static string GetDisplayName(this DriverType type)
    {
        return type switch
        {
            DriverType.SqlServer => "SQL Server",
            DriverType.MySql => "MySQL",
            DriverType.PostgreSql => "PostgreSQL",
            DriverType.Oracle => "Oracle",
            DriverType.SqLite => "SQLite",
            _ => type.ToString()
        };
    }
    
    public static string GetTestQuery(this DriverType type)
    {
        return type switch
        {
            DriverType.Oracle => "SELECT 1 FROM DUAL",
            DriverType.SqLite => "SELECT 1",
            DriverType.SqlServer => "SELECT 1",
            DriverType.MySql => "SELECT 1",
            DriverType.PostgreSql => "SELECT 1",
            _ => "SELECT 1"
        };
    }
    
    public static SqlSugar.DbType ToSqlSugarDbType(this DriverType type)
    {
        return type switch
        {
            DriverType.SqlServer => SqlSugar.DbType.SqlServer,
            DriverType.MySql => SqlSugar.DbType.MySql,
            DriverType.PostgreSql => SqlSugar.DbType.PostgreSQL,
            DriverType.Oracle => SqlSugar.DbType.Oracle,
            DriverType.SqLite => SqlSugar.DbType.Sqlite,
            _ => throw new NotSupportedException($"不支持的数据库类型: {type}")
        };
    }
}