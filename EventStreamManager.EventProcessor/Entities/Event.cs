namespace EventStreamManager.EventProcessor.Entities;


public class Event
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 医院ID
    /// </summary>
    public long IntHospitalID { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 事件引用ID
    /// </summary>
    public string StrEventReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// 事件名称
    /// </summary>
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// 事件代码
    /// </summary>
    public string EventCode { get; set; } = string.Empty;

    /// <summary>
    /// 操作员名称
    /// </summary>
    public string OperatorName { get; set; } = string.Empty;

    /// <summary>
    /// 操作员代码
    /// </summary>
    public string OperatorCode { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateDatetime { get; set; }

    /// <summary>
    /// 扩展数据
    /// </summary>
    public string? ExtenData { get; set; }

    /// <summary>
    /// 创建方式
    /// </summary>
    public byte CreateWay { get; set; }
}