using EventStreamManager.Infrastructure.Models.Execution.Debug;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services.Mock;

public class MockDataGenerator
{
    private readonly ILogger<MockDataGenerator> _logger;
    private readonly Random _random = new();

    public MockDataGenerator(ILogger<MockDataGenerator> logger)
    {
        _logger = logger;
    }
    
    
    public class ExamineTemplate
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public object SampleData { get; set; } = new();
    }

    public List<ExamineTemplate> GetExamineTemplates(string databaseType)
     {
        var templates = new List<ExamineTemplate>();

        switch (databaseType.ToLower())
        {
            case "ultrasound":
                templates.Add(new ExamineTemplate
                {
                    Id = "US-001",
                    Name = "常规超声检查",
                    Description = "标准的超声检查数据模板",
                    SampleData = new
                    {
                        strExamineId = "US-20240306-001",
                        strPatientId = "P001",
                        strPatientName = "张三",
                        strExamType = "腹部超声",
                        dtCreateTime = DateTime.Now.AddHours(-2),
                        strReportStatus = "已完成"
                    }
                });
                templates.Add(new ExamineTemplate
                {
                    Id = "US-002",
                    Name = "心脏超声检查",
                    Description = "心脏超声检查数据模板",
                    SampleData = new
                    {
                        strExamineId = "US-20240306-002",
                        strPatientId = "P002",
                        strPatientName = "李四",
                        strExamType = "心脏超声",
                        dtCreateTime = DateTime.Now.AddHours(-1),
                        strReportStatus = "审核中"
                    }
                });
                break;

            case "radiology":
                templates.Add(new ExamineTemplate
                {
                    Id = "XR-001",
                    Name = "胸部X光检查",
                    Description = "胸部X光检查数据模板",
                    SampleData = new
                    {
                        strExamineId = "XR-20240306-001",
                        strPatientId = "P003",
                        strPatientName = "王五",
                        strExamType = "胸部X光",
                        dtCreateTime = DateTime.Now.AddHours(-3),
                        strReportStatus = "已完成"
                    }
                });
                break;

            case "endoscopy":
                templates.Add(new ExamineTemplate
                {
                    Id = "EN-001",
                    Name = "胃镜检查",
                    Description = "胃镜检查数据模板",
                    SampleData = new
                    {
                        strExamineId = "EN-20240306-001",
                        strPatientId = "P004",
                        strPatientName = "赵六",
                        strExamType = "胃镜检查",
                        dtCreateTime = DateTime.Now.AddHours(-1.5),
                        strReportStatus = "进行中"
                    }
                });
                break;
        }

        return templates;
    }
    /// <summary>
    /// 生成模拟数据
    /// </summary>
    public MockEventData GenerateMockData(string? eventId, string eventCode, string databaseType)
    {
        var data = new MockEventData
        {
            EventId = string.IsNullOrEmpty(eventId) ? GenerateEventId() : eventId,
            EventCode = eventCode,
            DatabaseType = databaseType,
            EventTime = DateTime.Now.AddMinutes(-_random.Next(1, 60)),
            Data = GenerateDataByType(databaseType, eventCode)
        };

        _logger.LogDebug("生成模拟数据: {EventId}, 类型: {DatabaseType}", data.EventId, databaseType);
        return data;
    }

    /// <summary>
    /// 生成事件ID
    /// </summary>
    private string GenerateEventId()
    {
        var prefix = _random.Next(1000, 9999);
        var suffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        return $"EVT{prefix}{suffix}";
    }

    /// <summary>
    /// 根据数据库类型生成数据
    /// </summary>
    private Dictionary<string, object> GenerateDataByType(string databaseType, string eventCode)
    {
        return databaseType.ToLower() switch
        {
            "ultrasound" => GenerateUltrasoundData(eventCode),
            "radiology" => GenerateRadiologyData(eventCode),
            "endoscopy" => GenerateEndoscopyData(eventCode),
            _ => GenerateDefaultData(eventCode)
        };
    }

    private Dictionary<string, object> GenerateUltrasoundData(string eventCode)
    {
        var patients = new[] { "张三", "李四", "王五", "赵六", "钱七" };
        var doctors = new[] { "张医生", "李医生", "王医生" };
        var devices = new[] { "超声仪A", "超声仪B", "便携超声" };

        return new Dictionary<string, object>
        {
            ["patientId"] = $"P{_random.Next(10000, 99999)}",
            ["patientName"] = patients[_random.Next(patients.Length)],
            ["patientAge"] = _random.Next(18, 80),
            ["patientGender"] = _random.Next(0, 2) == 0 ? "男" : "女",
            ["visitId"] = $"V{DateTime.Now:yyyyMMdd}{_random.Next(1000, 9999)}",
            ["doctor"] = doctors[_random.Next(doctors.Length)],
            ["device"] = devices[_random.Next(devices.Length)],
            ["examType"] = GetRandomExamType(),
            ["findings"] = GetRandomFindings(),
            ["images"] = _random.Next(1, 10),
            ["reportStatus"] = _random.Next(0, 3) switch
            {
                0 => "草稿",
                1 => "已审核",
                _ => "已发布"
            },
            ["strSource"] = GetRandomSource(),
            ["examTime"] = DateTime.Now.AddMinutes(-_random.Next(30, 120)).ToString("yyyy-MM-dd HH:mm:ss"),
            ["reportTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["eventCode"] = eventCode
        };
    }

    private Dictionary<string, object> GenerateRadiologyData(string eventCode)
    {
        return new Dictionary<string, object>
        {
            ["patientId"] = $"P{_random.Next(10000, 99999)}",
            ["patientName"] = GetRandomName(),
            ["examId"] = $"XR{DateTime.Now:yyyyMMdd}{_random.Next(100, 999)}",
            ["examType"] = GetRandomXRayType(),
            ["bodyPart"] = GetRandomBodyPart(),
            ["doctor"] = GetRandomDoctor(),
            ["technician"] = GetRandomTechnician(),
            ["images"] = _random.Next(1, 5),
            ["radiationDose"] = Math.Round(_random.NextDouble() * 5 + 0.1, 2),
            ["findings"] = GetRandomRadiologyFindings(),
            ["strSource"] = GetRandomSource(),
            ["examTime"] = DateTime.Now.AddMinutes(-_random.Next(30, 180)).ToString("yyyy-MM-dd HH:mm:ss"),
            ["eventCode"] = eventCode
        };
    }

    private Dictionary<string, object> GenerateEndoscopyData(string eventCode)
    {
        return new Dictionary<string, object>
        {
            ["patientId"] = $"P{_random.Next(10000, 99999)}",
            ["patientName"] = GetRandomName(),
            ["examId"] = $"EN{DateTime.Now:yyyyMMdd}{_random.Next(100, 999)}",
            ["examType"] = GetRandomEndoscopyType(),
            ["doctor"] = GetRandomDoctor(),
            ["assistant"] = GetRandomNurse(),
            ["findings"] = GetRandomEndoscopyFindings(),
            ["biopsy"] = _random.Next(0, 2) == 1,
            ["biopsySites"] = _random.Next(0, 3),
            ["images"] = _random.Next(0, 10),
            ["strSource"] = GetRandomSource(),
            ["examTime"] = DateTime.Now.AddMinutes(-_random.Next(30, 240)).ToString("yyyy-MM-dd HH:mm:ss"),
            ["eventCode"] = eventCode
        };
    }

    private Dictionary<string, object> GenerateDefaultData(string eventCode)
    {
        return new Dictionary<string, object>
        {
            ["eventId"] = Guid.NewGuid().ToString(),
            ["eventCode"] = eventCode,
            ["timestamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            ["data"] = new
            {
                id = _random.Next(1000, 9999),
                name = GetRandomName(),
                value = _random.NextDouble() * 100
            }
        };
    }

    private string GetRandomName()
    {
        var names = new[] { "张三", "李四", "王五", "赵六", "钱七", "孙八", "周九" };
        return names[_random.Next(names.Length)];
    }

    private string GetRandomDoctor()
    {
        var doctors = new[] { "张医生", "李医生", "王医生", "赵医生", "刘医生" };
        return doctors[_random.Next(doctors.Length)];
    }

    private string GetRandomTechnician()
    {
        var techs = new[] { "技师A", "技师B", "技师C" };
        return techs[_random.Next(techs.Length)];
    }

    private string GetRandomNurse()
    {
        var nurses = new[] { "护士A", "护士B", "护士C" };
        return nurses[_random.Next(nurses.Length)];
    }

    private string GetRandomExamType()
    {
        var types = new[] { "腹部超声", "心脏超声", "产科超声", "血管超声", "浅表器官" };
        return types[_random.Next(types.Length)];
    }

    private string GetRandomXRayType()
    {
        var types = new[] { "胸部X光", "腹部X光", "骨骼X光", "牙科X光" };
        return types[_random.Next(types.Length)];
    }

    private string GetRandomEndoscopyType()
    {
        var types = new[] { "胃镜", "肠镜", "支气管镜", "腹腔镜" };
        return types[_random.Next(types.Length)];
    }

    private string GetRandomBodyPart()
    {
        var parts = new[] { "胸部", "腹部", "头部", "四肢", "脊柱" };
        return parts[_random.Next(parts.Length)];
    }

    private string GetRandomFindings()
    {
        var findings = new[]
        {
            "未见明显异常",
            "轻度脂肪肝",
            "胆囊息肉",
            "肾囊肿",
            "子宫肌瘤"
        };
        return findings[_random.Next(findings.Length)];
    }

    private string GetRandomRadiologyFindings()
    {
        var findings = new[]
        {
            "未见异常",
            "肺部结节",
            "骨折线",
            "关节退行性变",
            "炎症表现"
        };
        return findings[_random.Next(findings.Length)];
    }

    private string GetRandomEndoscopyFindings()
    {
        var findings = new[]
        {
            "未见异常",
            "浅表性胃炎",
            "胃溃疡",
            "结肠息肉",
            "食管炎"
        };
        return findings[_random.Next(findings.Length)];
    }

    private string GetRandomSource()
    {
        var sources = new[] { "门诊", "住院", "体检", "急诊" };
        return sources[_random.Next(sources.Length)];
    }

    /// <summary>
    /// 获取数据模板
    /// </summary>
    public object GetTemplates(string databaseType)
    {
        return databaseType.ToLower() switch
        {
            "ultrasound" => new
            {
                Fields = new[]
                {
                    new { Name = "patientId", Type = "string", Description = "患者ID" },
                    new { Name = "patientName", Type = "string", Description = "患者姓名" },
                    new { Name = "patientAge", Type = "number", Description = "患者年龄" },
                    new { Name = "patientGender", Type = "string", Description = "患者性别" },
                    new { Name = "examType", Type = "string", Description = "检查类型" },
                    new { Name = "findings", Type = "string", Description = "检查所见" },
                    new { Name = "strSource", Type = "string", Description = "数据来源（门诊/住院/体检）" }
                }
            },
            "radiology" => new
            {
                Fields = new[]
                {
                    new { Name = "patientId", Type = "string", Description = "患者ID" },
                    new { Name = "examType", Type = "string", Description = "检查类型" },
                    new { Name = "bodyPart", Type = "string", Description = "检查部位" },
                    new { Name = "findings", Type = "string", Description = "所见" },
                    new { Name = "radiationDose", Type = "number", Description = "辐射剂量" }
                }
            },
            "endoscopy" => new
            {
                Fields = new[]
                {
                    new { Name = "patientId", Type = "string", Description = "患者ID" },
                    new { Name = "examType", Type = "string", Description = "检查类型" },
                    new { Name = "findings", Type = "string", Description = "所见" },
                    new { Name = "biopsy", Type = "boolean", Description = "是否活检" }
                }
            },
            _ => new
            {
                Fields = new[]
                {
                    new { Name = "eventId", Type = "string", Description = "事件ID" },
                    new { Name = "eventCode", Type = "string", Description = "事件码" },
                    new { Name = "timestamp", Type = "string", Description = "时间戳" }
                }
            }
        };
    }
}