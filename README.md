# Event Stream Manager (ESM)

基于JavaScript脚本引擎和事件驱动架构的数据交换系统，用于验证与第三方系统集成方案的可行性。

## ✨ 核心特性

- 🚀 **事件驱动架构** - 自动监听和处理数据库中的事件
- 📜 **JavaScript脚本引擎** - 使用Jint引擎执行自定义JS脚本
- 🔌 **可扩展插件系统** - 支持SQL插件、标准函数插件等扩展
- 🌐 **接口发送器** - 支持HTTP/HTTPS接口调用，带重试机制
- 💾 **多数据库支持** - MySQL、SQL Server、PostgreSQL、SQLite、Oracle
- 🎯 **灵活的数据处理** - 通过JS脚本实现复杂的数据转换和业务逻辑
- 📊 **完整的监控和日志** - 事件处理日志、执行统计、服务状态监控
- 🎨 **现代化UI** - 基于React + TypeScript + Vite的前端管理界面
- 📦 **一体化部署** - 前端可打包到后端wwwroot，简化部署配置

## ⚠️ 项目状态

> **功能完善，前端完成，测试覆盖有限**

本项目已完成**所有核心功能开发和前端UI实现**，具备完整的数据交换能力。但由于**实际测试场景和上线案例较少**，可能存在未发现的问题。

### 📋 当前状态

| 方面 | 状态 |
|------|------|
| 功能开发 | ✅ 已完成 |
| 前端UI | ✅ 已完成 |
| 核心功能 | ✅ 可用 |
| 测试覆盖 | ⚠️ 有限 |
| 生产环境 | ⚠️ 建议充分验证后使用 |

### 🔍 可能存在的问题领域

由于测试覆盖有限，以下领域可能存在未发现的问题：

- **边界场景**：某些特殊边界条件可能未覆盖到
- **异常处理**：网络中断、数据库连接异常等极端情况处理需要更多验证
- **性能表现**：高并发场景和长时间运行的表现需要实际测试验证
- **内存管理**：长时间运行的内存使用情况需要监控
- **脚本安全**：JavaScript脚本的安全性和资源隔离需要实际使用检验

### ✅ 建议使用方式

1. **测试环境验证**
   ```
   测试环境 → 功能验证 → 性能测试 → 异常场景测试 → 小范围试用 → 逐步扩大
   ```

2. **逐步上线策略**
   - 先接入非关键业务
   - 监控系统运行状态
   - 收集问题和反馈
   - 逐步扩大使用范围
   - 建议开启详细日志

3. **问题反馈**
   - 使用过程中遇到的问题请及时记录
   - 保存完整的错误堆栈和上下文信息
   - 建议开启详细日志模式
   - 定期检查系统运行状态

### 💬 说明

本项目已具备完整的功能和前端，可用于实际的数据交换场景。但由于实际测试案例有限，建议在正式用于生产环境前进行充分的测试和验证。

---

## 🏗️ 项目架构

```
EventStreamManager/
├── EventStreamManager.WebApi/          # Web API主项目（包含前端wwwroot）
├── EventStreamManager.EventProcessor/  # 事件处理器核心
├── EventStreamManager.Infrastructure/  # 基础设施层
├── EventStreamManager.JSFunction/      # JS函数核心接口
├── EventStreamManager.JSFunction.Loader/    # JS函数加载器
├── EventStreamManager.JSFunction.Sql/       # SQL操作插件
├── EventStreamManager.JSFunction.Standard/  # 标准函数插件
└── EventStreamManager-UI/             # 前端UI（React）
```

### 核心模块说明

#### 1. EventStreamManager.WebApi
- ASP.NET Core Web API
- 提供RESTful接口管理事件监听器、处理器、脚本等
- 包含调试接口用于脚本测试
- **支持前端静态文件托管** - 可将前端打包到wwwroot目录

#### 2. EventStreamManager.EventProcessor
- **EventProcessorService** - 后台服务，管理所有事件处理器
- **DatabaseTypeProcessor** - 按数据库类型分组的处理器
- **EventScanner** - 事件扫描器，定期扫描数据库中的新事件
- **ScriptExecutor** - 脚本执行器，执行JS处理器
- **InterfaceSender** - 接口发送器，发送HTTP请求
- **HandleRecorder** - 处理记录器，记录事件处理结果

#### 3. EventStreamManager.Infrastructure
- **JavaScriptExecutionService** - JavaScript执行服务，基于Jint引擎
- **HttpSendService** - HTTP发送服务
- **IDataService** - 数据存储服务接口
- **数据实体和模型定义**

#### 4. JSFunction插件系统
- **JSFunction.Loader** - 动态加载JS函数插件
- **JSFunction.Sql** - SQL操作插件（支持查询、执行、批量操作、事务）
- **JSFunction.Standard** - 标准工具函数插件

## 🚀 快速开始

### 环境要求

- **.NET 6.0 SDK** 或更高版本
- **Node.js** 16+ （用于前端UI开发）
- **pnpm** （推荐的包管理器）

### 开发模式启动

#### 后端启动

```bash
# 进入WebApi目录
cd EventStreamManager.WebApi

# 还原依赖
dotnet restore

# 运行API
dotnet run
```

服务将在 `http://localhost:7138` 启动

#### 前端启动（开发模式）

```bash
# 进入UI目录
cd EventStreamManager-UI

# 安装依赖
pnpm install

# 启动开发服务器
pnpm run dev
```

UI将在 `http://localhost:3000` 启动

### 生产模式部署（推荐）

#### 1. 打包前端

```bash
# 进入UI目录
cd EventStreamManager-UI

# 安装依赖
pnpm install

# 打包构建
pnpm run build
```

打包后的文件将生成在 `EventStreamManager-UI/dist` 目录。

#### 2. 复制到后端wwwroot

```bash
# 将前端打包文件复制到后端项目的wwwroot目录
cp -r EventStreamManager-UI/dist/* EventStreamManager.WebApi/wwwroot/
```

#### 3. 启动后端服务

```bash
# 进入WebApi目录
cd EventStreamManager.WebApi

# 运行服务
dotnet run
```

现在可以直接访问 `http://localhost:7138` 使用完整系统。

## 📝 脚本编写指南

### ProcessResult类定义

每个事件处理器脚本必须返回一个 `ProcessResult` 对象，用于指示处理结果：

```javascript
class ProcessResult {
    constructor() {
        this.needToSend = true;        // 是否需要发送到接口
        this.reason = '';              // 不发送的原因
        this.error = null;             // 异常信息
        this.requestInfo = null;       // 请求数据
    }

    // 设置成功并发送
    setSuccess(requestInfo) {
        this.needToSend = true;
        this.reason = '';
        this.error = null;
        this.requestInfo = requestInfo;
        return this;
    }

    // 设置失败
    setFailure(reason, error = null) {
        this.needToSend = false;
        this.reason = reason;
        this.error = error;
        this.requestInfo = null;
        return this;
    }

    // 设置不发送（仅执行脚本）
    setNoSend(reason = '') {
        this.needToSend = false;
        this.reason = reason || '仅执行脚本，无需发送';
        this.error = null;
        this.requestInfo = null;
        return this;
    }
}
```

### 基础处理器示例

```javascript
/**
 * 数据处理函数
 * @param {Object} data - 输入数据
 * @returns {ProcessResult} 处理结果
 */
function process(data) {
    const result = new ProcessResult();

    console_log('收到事件:', data.Context.EventName);

    // 你的业务逻辑处理
    const transformedData = {
        eventId: data.Context.EventId,
        eventType: data.Context.EventType,
        processedAt: new Date().toISOString()
    };

    return result.setSuccess(JSON.stringify(transformedData));
}
```

### 使用SQL插件示例

```javascript
function process(data) {
    const result = new ProcessResult();

    // 查询用户信息
    const users = sql_query(
        'mysql',
        'Server=localhost;Database=test;Uid=root;Pwd=123456;',
        'SELECT * FROM Users WHERE Id = @id',
        { '@id': data.Context.EventId }
    );

    console_log('查询结果:', users);

    // 更新数据
    sql_execute(
        'mysql',
        'Server=localhost;Database=test;Uid=root;Pwd=123456;',
        'UPDATE Users SET Status = @status WHERE Id = @id',
        { '@status': 'processed', '@id': data.Context.EventId }
    );

    return result.setNoSend('数据已处理');
}
```

### 可用的内置函数

#### SQL函数
- `sql_query(dbType, connectionString, sql, parameters)` - 执行查询
- `sql_execute(dbType, connectionString, sql, parameters)` - 执行增删改
- `sql_scalar(dbType, connectionString, sql, parameters)` - 获取单个值
- `sql_bulk_insert(dbType, connectionString, tableName, data, columns)` - 批量插入
- `sql_transaction(dbType, connectionString, sqlStatements)` - 事务操作
- `sql_test_connection(dbType, connectionString)` - 测试连接
- `oracle_nextval(connectionString, sequenceName, dbType)` - Oracle序列

#### Console函数
- `console_log(...args)` - 输出日志
- `console_info(...args)` - 输出信息
- `console_warn(...args)` - 输出警告
- `console_error(...args)` - 输出错误
- `console_debug(...args)` - 输出调试信息
- `console_clear()` - 清空控制台

## 🎯 数据流

```
[数据库事件表]
    ↓
[EventScanner - 扫描新事件]
    ↓
[ScriptExecutor - 执行JS脚本]
    ↓
[脚本处理逻辑]
    ↓
[需要发送?] ──否──> [记录日志] ──> [结束]
    ↓是
[InterfaceSender - 发送HTTP请求]
    ↓
[记录发送结果]
    ↓
[结束]
```

## 📊 事件数据结构

### 输入数据结构

```javascript
{
  "rows": [
    {
      "field1": "value1",
      "field2": "value2"
    }
  ],
  "database": {
    "type": "mysql"
  },
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

### ProcessResult结构

```javascript
{
  "needToSend": true,      // 是否发送到接口
  "reason": "",            // 不发送的原因
  "error": null,           // 错误信息
  "requestInfo": null      // 发送给接口的数据（JSON字符串）
}
```

## 📝 事件表结构

系统期望在监听的数据库中有以下事件表结构：

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

## 🔐 配置说明

### appsettings.json

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

## 🛠️ 开发指南

### 添加新的JS函数插件

1. 创建实现 `IJsFunctionProvider` 接口的类
2. 在 `GetFunctions()` 方法中定义你的函数
3. 注册到 `JSFunctionRegistry`

示例：

```csharp
public class CustomFunctionProvider : IJsFunctionProvider
{
    public string Name => "Custom Functions";
    public string Description => "自定义函数插件";
    public string Version => "1.0.0";

    public IEnumerable<FunctionMetadata> GetFunctions()
    {
        yield return new FunctionMetadata
        {
            Name = "my_custom_function",
            Category = "Custom",
            Description = "我的自定义函数",
            FunctionDelegate = new Func<string, int>(input => input.Length),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "input", Type = typeof(string), Description = "输入字符串" }
            },
            ReturnType = typeof(int),
            Example = "var len = my_custom_function('hello');"
        };
    }
}
```

### 前端开发说明

前端项目基于 React + TypeScript + Vite，开发时需注意：

1. API请求地址配置：开发模式使用 `http://localhost:7138`
2. 生产模式下，前端打包后通过wwwroot访问，API相对路径即可
3. 使用UI组件库和状态管理方案保持一致性

## 📄 许可证

本项目采用 MIT 许可证。