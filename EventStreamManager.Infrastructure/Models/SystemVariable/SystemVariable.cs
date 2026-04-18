namespace EventStreamManager.Infrastructure.Models.SystemVariable;

/// <summary>
/// 系统变量模型，用于持久化存储可在JS脚本中使用的全局变量
/// </summary>
public class SystemVariable
{
    /// <summary>
    /// 唯一标识
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 变量键名（唯一）
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 变量值
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// 变量描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 变量分类
    /// </summary>
    public string Category { get; set; } = "General";

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
