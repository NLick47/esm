using System.Data.Common;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

namespace EventStreamManager.JSFunction.Sql;

/// <summary>
/// 极简SQL操作JS函数提供者
/// </summary>
public class SimpleSqlJsFunctionProvider : IJsFunctionProvider
{
    public string Name => "Simple SQL Functions";
    public string Description => "极简SQL操作函数，支持MySQL、SQL Server、PostgreSQL、SQLite、Oracle";
    public string Version => "2.1.0";

    static SimpleSqlJsFunctionProvider()
    {
        try
        {
            _ = typeof(SqlClientFactory).FullName;
            _ = typeof(MySqlClientFactory).FullName;
            _ = typeof(NpgsqlFactory).FullName;
            _ = typeof(SqliteFactory).FullName;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"数据库驱动初始化失败: {ex.Message}");
        }
    }
    
    public IEnumerable<FunctionMetadata> GetFunctions()
    {
      
        yield return new FunctionMetadata
        {
            Name = "sql_query",
            Category = "SQL",
            Description = "执行SQL查询，返回结果集。自动创建和释放连接。",
            FunctionDelegate = new Func<string, string, string, object?, List<Dictionary<string, object>>>(
                (dbType, connectionString, sql, parameters) =>
                {
                    using var connection = CreateConnection(dbType, connectionString);
                    connection.Open();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = sql;
                    
                    if (parameters != null)
                    {
                        AddParameters(command, parameters, dbType);
                    }
                    
                    var results = new List<Dictionary<string, object>>();
                    using var reader = command.ExecuteReader();
                    
                    while (reader.Read())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var value = reader.GetValue(i);
                            row[reader.GetName(i)] = (value == DBNull.Value ? null : value)!;
                        }
                        results.Add(row);
                    }
                    
                    return results;
                }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "dbType", Type = typeof(string), Description = "数据库类型: sqlserver, mysql, postgresql, sqlite, oracle" },
                new() { Name = "connectionString", Type = typeof(string), Description = "数据库连接字符串" },
                new() { Name = "sql", Type = typeof(string), Description = "SQL查询语句" },
                new() { Name = "parameters", Type = typeof(object), IsOptional = true, Description = "查询参数(可选)，如: { '@age': 18 } 或 Oracle使用 ':age'" }
            },
            ReturnType = typeof(List<Dictionary<string, object>>),
            Example = @"var users = sql_query('mysql', 'Server=localhost;Database=test;Uid=root;Pwd=123456;', 
                'SELECT * FROM Users WHERE Age > @age', { '@age': 18 });"
        };

        // 执行增删改（INSERT/UPDATE/DELETE）
        yield return new FunctionMetadata
        {
            Name = "sql_execute",
            Category = "SQL",
            Description = "执行非查询SQL（INSERT/UPDATE/DELETE），返回影响行数。自动创建和释放连接。",
            FunctionDelegate = new Func<string, string, string, object?, int>(
                (dbType, connectionString, sql, parameters) =>
                {
                    using var connection = CreateConnection(dbType, connectionString);
                    connection.Open();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = sql;
                    
                    if (parameters != null)
                    {
                        AddParameters(command, parameters, dbType);
                    }
                    
                    return command.ExecuteNonQuery();
                }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "dbType", Type = typeof(string), Description = "数据库类型" },
                new() { Name = "connectionString", Type = typeof(string), Description = "连接字符串" },
                new() { Name = "sql", Type = typeof(string), Description = "SQL语句" },
                new() { Name = "parameters", Type = typeof(object), IsOptional = true, Description = "参数(可选)" }
            },
            ReturnType = typeof(int),
            Example = @"var affected = sql_execute('mysql', 'Server=localhost;Database=test;Uid=root;Pwd=123456;',
                'UPDATE Users SET Age = @age WHERE Id = @id', { '@age': 25, '@id': 1 });"
        };

        // 3. 获取单个值（标量）
        yield return new FunctionMetadata
        {
            Name = "sql_scalar",
            Category = "SQL",
            Description = "执行查询并返回第一行第一列的值。自动创建和释放连接。",
            FunctionDelegate = new Func<string, string, string, object?, object?>(
                (dbType, connectionString, sql, parameters) =>
                {
                    using var connection = CreateConnection(dbType, connectionString);
                    connection.Open();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = sql;
                    
                    if (parameters != null)
                    {
                        AddParameters(command, parameters, dbType);
                    }
                    
                    var result = command.ExecuteScalar();
                    return result == DBNull.Value ? null : result;
                }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "dbType", Type = typeof(string), Description = "数据库类型" },
                new() { Name = "connectionString", Type = typeof(string), Description = "连接字符串" },
                new() { Name = "sql", Type = typeof(string), Description = "SQL查询语句" },
                new() { Name = "parameters", Type = typeof(object), IsOptional = true, Description = "参数(可选)" }
            },
            ReturnType = typeof(object),
            Example = @"var count = sql_scalar('mysql', 'Server=localhost;Database=test;Uid=root;Pwd=123456;',
                'SELECT COUNT(*) FROM Users');"
        };

        // 4. 批量插入（使用事务）
        yield return new FunctionMetadata
        {
            Name = "sql_bulk_insert",
            Category = "SQL",
            Description = "批量插入数据，使用事务提高性能。自动创建和释放连接。",
            FunctionDelegate = new Func<string, string, string, List<object>, List<string>, int>(
                (dbType, connectionString, tableName, data, columns) =>
                {
                    if (data == null || data.Count == 0)
                        return 0;
                    
                    using var connection = CreateConnection(dbType, connectionString);
                    connection.Open();
                    
                    using var transaction = connection.BeginTransaction();
                    
                    // 根据数据库类型生成不同的参数占位符
                    string paramPrefix = GetParameterPrefix(dbType);
                    var columnList = string.Join(", ", columns);
                    var paramPlaceholders = string.Join(", ", columns.Select((_, i) => $"{paramPrefix}p{i}"));
                    var sql = $"INSERT INTO {tableName} ({columnList}) VALUES ({paramPlaceholders})";
                    
                    int affectedRows = 0;
                    
                    try
                    {
                        foreach (var row in data)
                        {
                            using var command = connection.CreateCommand();
                            command.CommandText = sql;
                            command.Transaction = transaction;
                            
                            var rowDict = row as Dictionary<string, object>;
                            if (rowDict == null)
                                continue;
                            
                            for (int i = 0; i < columns.Count; i++)
                            {
                                var param = command.CreateParameter();
                                param.ParameterName = $"{paramPrefix}p{i}";
                                param.Value = rowDict.ContainsKey(columns[i]) ? rowDict[columns[i]] : DBNull.Value;
                                command.Parameters.Add(param);
                            }
                            
                            affectedRows += command.ExecuteNonQuery();
                        }
                        
                        transaction.Commit();
                        return affectedRows;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "dbType", Type = typeof(string), Description = "数据库类型" },
                new() { Name = "connectionString", Type = typeof(string), Description = "连接字符串" },
                new() { Name = "tableName", Type = typeof(string), Description = "表名" },
                new() { Name = "data", Type = typeof(List<object>), Description = "要插入的数据列表" },
                new() { Name = "columns", Type = typeof(List<string>), Description = "列名列表" }
            },
            ReturnType = typeof(int),
            Example = @"var inserted = sql_bulk_insert('mysql', 'Server=localhost;Database=test;Uid=root;Pwd=123456;',
                'Users', [{Name:'John',Age:25},{Name:'Jane',Age:30}], ['Name','Age']);"
        };

        // 事务操作（多个SQL语句）
        yield return new FunctionMetadata
        {
            Name = "sql_transaction",
            Category = "SQL",
            Description = "在事务中执行多个SQL语句，保证原子性。自动创建和释放连接。",
            FunctionDelegate = new Func<string, string, List<object>, bool>(
                (dbType, connectionString, sqlStatements) =>
                {
                    using var connection = CreateConnection(dbType, connectionString);
                    connection.Open();
                    
                    using var transaction = connection.BeginTransaction();
                    
                    try
                    {
                        foreach (var stmt in sqlStatements)
                        {
                            string sql = "";
                            object? parameters = null;
                            
                            if (stmt is Dictionary<string, object> dict)
                            {
                                sql = dict["sql"]?.ToString() ?? "";
                                parameters = dict.ContainsKey("parameters") ? dict["parameters"] : null;
                            }
                            else if (stmt is string str)
                            {
                                sql = str;
                            }
                            else
                            {
                                throw new Exception($"无效的SQL语句格式: {stmt}");
                            }
                            
                            using var command = connection.CreateCommand();
                            command.CommandText = sql;
                            command.Transaction = transaction;
                            
                            if (parameters != null)
                            {
                                AddParameters(command, parameters, dbType);
                            }
                            
                            command.ExecuteNonQuery();
                        }
                        
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "dbType", Type = typeof(string), Description = "数据库类型" },
                new() { Name = "connectionString", Type = typeof(string), Description = "连接字符串" },
                new() { Name = "sqlStatements", Type = typeof(List<object>), Description = "SQL语句列表，每个可以是字符串或{sql, parameters}对象" }
            },
            ReturnType = typeof(bool),
            Example = @"var success = sql_transaction('mysql', 'Server=localhost;Database=test;Uid=root;Pwd=123456;', [
                'UPDATE Accounts SET Balance = Balance - 100 WHERE UserId = 1',
                { sql: 'UPDATE Accounts SET Balance = Balance + @amount WHERE UserId = @userId', 
                  parameters: { '@amount': 100, '@userId': 2 } }
            ]);"
        };
        
        // 6. 测试连接
        yield return new FunctionMetadata
        {
            Name = "sql_test_connection",
            Category = "SQL",
            Description = "测试数据库连接是否正常",
            FunctionDelegate = new Func<string, string, bool>(
                (dbType, connectionString) =>
                {
                    try
                    {
                        using var connection = CreateConnection(dbType, connectionString);
                        connection.Open();
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "dbType", Type = typeof(string), Description = "数据库类型" },
                new() { Name = "connectionString", Type = typeof(string), Description = "连接字符串" }
            },
            ReturnType = typeof(bool),
            Example = @"var isConnected = sql_test_connection('mysql', 'Server=localhost;Database=test;Uid=root;Pwd=123456;');"
        };
        
        // Oracle序列获取（获取下一个序列值）
        yield return new FunctionMetadata
        {
            Name = "oracle_nextval",
            Category = "SQL",
            Description = "获取Oracle序列的下一个值",
            FunctionDelegate = new Func<string, string, string, long>(
                (connectionString, sequenceName, dbType) =>
                {
                    using var connection = CreateConnection("oracle", connectionString);
                    connection.Open();
                    
                    using var command = connection.CreateCommand();
                    command.CommandText = $"SELECT {sequenceName}.NEXTVAL FROM DUAL";
                    
                    var result = command.ExecuteScalar();
                    return Convert.ToInt64(result);
                }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "connectionString", Type = typeof(string), Description = "Oracle连接字符串" },
                new() { Name = "sequenceName", Type = typeof(string), Description = "序列名称" },
                new() { Name = "dbType", Type = typeof(string), Description = "数据库类型，固定为'oracle'" }
            },
            ReturnType = typeof(long),
            Example = @"var nextId = oracle_nextval('User Id=scott;Password=tiger;Data Source=localhost:1521/XE', 'SEQ_USER_ID', 'oracle');"
        };
    }

    /// <summary>
    /// 创建数据库连接
    /// </summary>
    private DbConnection CreateConnection(string dbType, string connectionString)
    {
        DbConnection connection = dbType.ToLower() switch
        {
            "sqlserver" or "mssql" => new SqlConnection(connectionString),
            "mysql" => new MySqlConnection(connectionString),
            "postgresql" or "postgres" => new NpgsqlConnection(connectionString),
            "sqlite" => new SqliteConnection(connectionString),
            "oracle" => new OracleConnection(connectionString),
            _ => throw new ArgumentException($"不支持的数据库类型: {dbType}。支持的类型: sqlserver, mysql, postgresql, sqlite, oracle")
        };
        
        return connection;
    }

    /// <summary>
    /// 获取参数前缀
    /// </summary>
    private string GetParameterPrefix(string dbType)
    {
        return dbType.ToLower() switch
        {
            "sqlserver" or "mssql" => "@",
            "mysql" => "@",
            "postgresql" or "postgres" => "@",
            "sqlite" => "@",
            "oracle" => ":",
            _ => "@"
        };
    }

    /// <summary>
    /// 标准化参数名称（根据数据库类型）
    /// </summary>
    private string NormalizeParameterName(string paramName, string dbType)
    {
        if (string.IsNullOrEmpty(paramName))
            return paramName;
            
        string prefix = GetParameterPrefix(dbType);
        
        // 如果参数已经有正确的前缀，直接返回
        if (paramName.StartsWith(prefix))
            return paramName;
            
        // 移除其他可能的前缀
        paramName = paramName.TrimStart('@', ':', '?');
        
        // 添加正确的前缀
        return prefix + paramName;
    }

    /// <summary>
    /// 添加参数到命令对象
    /// </summary>
    private void AddParameters(DbCommand command, object parameters, string dbType)
    {
        if (parameters == null) return;
        
        var paramDict = parameters as Dictionary<string, object>;
        if (paramDict == null)
        {
            try
            {
                var json = JsonSerializer.Serialize(parameters);
                paramDict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            }
            catch
            {
                throw new ArgumentException("参数格式错误，请使用字典格式，如: { '@name': 'value' } 或对于Oracle使用 { ':name': 'value' }");
            }
        }
        
        if (paramDict != null)
        {
            foreach (var param in paramDict)
            {
                var dbParam = command.CreateParameter();
                // 标准化参数名称
                dbParam.ParameterName = NormalizeParameterName(param.Key, dbType);
                dbParam.Value = param.Value ?? DBNull.Value;
                
                // Oracle特殊处理：对于VARCHAR2类型，需要指定大小
                if (dbType.ToLower() == "oracle" && dbParam is OracleParameter oracleParam)
                {
                    if (param.Value is string strValue && strValue.Length > 0)
                    {
                        oracleParam.Size = strValue.Length;
                    }
                }
                
                command.Parameters.Add(dbParam);
            }
        }
    }
}