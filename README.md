# Event Stream Manager (ESM)

一个基于 JavaScript 脚本引擎的数据交换系统，用来验证和第三方系统集成的可行性方案。

这个项目最初是为了解决一个实际问题：我们有很多不同的业务系统需要对接，每次都要写大量的胶水代码。能不能让这个过程更灵活一点？于是就有了 ESM——用 JavaScript 脚本来处理数据流转，而不是每次都要重新编译部署。

## 能做什么

- 监听数据库事件（MySQL、SQL Server、PostgreSQL、SQLite、Oracle 都支持）
- 用 JavaScript 写处理逻辑，热更新不用重启
- 自动把处理后的数据推送到外部 HTTP 接口
- 自带重试机制，失败自动重发

技术栈是 .NET 6 + Jint（JavaScript 引擎）+ React 前端。前后端打包在一起，部署比较简单。

## 项目现状

说实话，这个项目功能都写完了，界面也做完了，但实际生产环境的测试还不够充分。我自己在几个场景跑过，但肯定还有很多边界情况没覆盖到。

如果你打算用在正式环境，建议：
- 先在测试环境跑一段时间，观察下内存和稳定性
- 从非核心业务开始接入，别一上来就接关键链路
- 开详细日志，方便出问题排查

**已知可能不太稳的地方：**
- 高并发下的表现还没充分验证
- 网络抖动、数据库断连后的恢复逻辑
- 长时间运行的内存占用情况
- JS 脚本的安全隔离（目前主要靠自觉）

## 架构概览

```
EventStreamManager/
├── EventStreamManager.WebApi/          # Web API主项目（前端打包后放这里）
├── EventStreamManager.EventProcessor/  # 事件处理核心
├── EventStreamManager.Infrastructure/  # 基础设施
├── EventStreamManager.JSFunction/      # JS函数接口定义
├── EventStreamManager.JSFunction.Loader/    # 插件加载器
├── EventStreamManager.JSFunction.Sql/       # SQL操作插件
├── EventStreamManager.JSFunction.Standard/  # 标准工具函数
└── EventStreamManager-UI/             # React前端
```

**核心流程：**
1. EventScanner 定期扫数据库，找新事件
2. ScriptExecutor 调用 Jint 执行你的 JS 脚本
3. 脚本返回要不要发送、发什么内容
4. InterfaceSender 负责 HTTP 推送，失败自动重试
5. HandleRecorder 记录处理结果，方便排查问题

## 快速开始

### 环境要求

- .NET 6.0 SDK
- Node.js 16+（开发前端需要）
- pnpm（或者 npm/yarn 也行）

### 开发模式

**后端：**
```bash
cd EventStreamManager.WebApi
dotnet restore
dotnet run
# 默认跑在 http://localhost:7138
```

**前端：**
```bash
cd EventStreamManager-UI
pnpm install
pnpm run dev
# 跑在 http://localhost:3000
```

### 生产部署

推荐把前端打包进后端，单服务部署：

```bash
# 打包前端
cd EventStreamManager-UI
pnpm install
pnpm run build

# 复制到后端 wwwroot
cp -r dist/* ../EventStreamManager.WebApi/wwwroot/

# 启动后端即可
cd ../EventStreamManager.WebApi
dotnet run
```

然后直接访问 http://localhost:7138 就行。

## 写脚本

每个处理器脚本需要返回一个 ProcessResult 对象：

```javascript
class ProcessResult {
    constructor() {
        this.needToSend = true;        // 要不要发HTTP请求
        this.reason = '';              // 不发的话原因是什么
        this.error = null;             // 异常信息
        this.requestInfo = null;       // 要发送的数据（JSON字符串）
    }

    setSuccess(requestInfo) {
        this.needToSend = true;
        this.reason = '';
        this.error = null;
        this.requestInfo = requestInfo;
        return this;
    }

    setFailure(reason, error) {
        this.needToSend = false;
        this.reason = reason;
        this.error = error;
        return this;
    }

    setNoSend(reason) {
        this.needToSend = false;
        this.reason = reason || '仅执行脚本，无需发送';
        return this;
    }
}
```

### 简单示例

```javascript
function process(data) {
    const result = new ProcessResult();
    
    console_log('收到事件:', data.Context.EventName);
    
    // 构造发送数据
    const payload = {
        eventId: data.Context.EventId,
        type: data.Context.EventType,
        timestamp: new Date().toISOString()
    };
    
    return result.setSuccess(JSON.stringify(payload));
}
```

### 查数据库示例

```javascript
function process(data) {
    const result = new ProcessResult();
    
    // 查用户信息
    const users = sql_query(
        'mysql',
        'Server=localhost;Database=test;Uid=root;Pwd=123456;',
        'SELECT * FROM Users WHERE Id = @id',
        { '@id': data.Context.EventId }
    );
    
    if (users.length === 0) {
        return result.setFailure('用户不存在');
    }
    
    // 更新状态
    sql_execute(
        'mysql',
        'Server=localhost;Database=test;Uid=root;Pwd=123456;',
        'UPDATE Users SET LastSync = NOW() WHERE Id = @id',
        { '@id': data.Context.EventId }
    );
    
    return result.setSuccess(JSON.stringify(users[0]));
}
```

### 可用的内置函数

**SQL 相关：**
- `sql_query(dbType, conn, sql, params)` - 查询，返回数组
- `sql_execute(dbType, conn, sql, params)` - 执行增删改
- `sql_scalar(dbType, conn, sql, params)` - 返回单个值
- `sql_bulk_insert(...)` - 批量插入
- `sql_transaction(...)` - 事务操作

**日志：**
- `console_log(...)` / `console_info(...)` / `console_warn(...)` / `console_error(...)` / `console_debug(...)`

## 数据格式

### 输入数据

```json
{
  "rows": [
    { "field1": "value1", "field2": "value2" }
  ],
  "database": { "type": "mysql" },
  "context": {
    "eventId": "123",
    "strEventReferenceId": "REF001",
    "eventType": "order.created",
    "eventName": "订单创建",
    "eventCode": "ORDER_001",
    "operatorName": "admin",
    "operatorCode": "ADMIN",
    "createDatetime": "2024-03-28T10:00:00",
    "extenData": ""
  },
  "processor": {
    "id": 1,
    "name": "订单处理器",
    "enabled": true
  }
}
```

### 输出数据（ProcessResult）

```json
{
  "needToSend": true,      // 是否发送
  "reason": "",            // 原因说明
  "error": null,           // 错误信息
  "requestInfo": "..."     // 发送内容（JSON字符串）
}
```

## 数据库表结构

系统会监听这个表：

```sql
CREATE TABLE tblevent (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    strEventReferenceId VARCHAR(100),
    EventType VARCHAR(50),
    EventName VARCHAR(200),
    EventCode VARCHAR(50),
    OperatorName VARCHAR(100),
    OperatorCode VARCHAR(50),
    CreateDatetime DATETIME,
    ExtenData TEXT,
    ProcessStatus INT DEFAULT 0
);
```

ProcessStatus 字段用来标记处理状态：
- `0` = 未处理
- `1` = 处理中
- `2` = 成功
- `3` = 失败

## 配置

`appsettings.json`：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "urls": "http://localhost:7138;"
}
```

## 扩展开发

如果想加自定义 JS 函数，实现 `IJsFunctionProvider` 接口：

```csharp
public class MyFunctions : IJsFunctionProvider
{
    public string Name => "My Functions";
    public string Description => "我的自定义函数";
    public string Version => "1.0.0";

    public IEnumerable<FunctionMetadata> GetFunctions()
    {
        yield return new FunctionMetadata
        {
            Name = "my_func",
            Category = "Custom",
            Description = "示例函数",
            FunctionDelegate = new Func<string, int>(input => input.Length),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "input", Type = typeof(string), Description = "输入字符串" }
            },
            ReturnType = typeof(int),
            Example = "var len = my_func('hello');"
        };
    }
}
```

## License

MIT

有什么使用问题或者建议，欢迎提 issue。
