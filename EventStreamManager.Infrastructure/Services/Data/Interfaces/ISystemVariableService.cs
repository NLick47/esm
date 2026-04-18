using EventStreamManager.Infrastructure.Models.SystemVariable;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

/// <summary>
/// 系统变量服务接口
/// </summary>
public interface ISystemVariableService
{
    /// <summary>
    /// 获取所有系统变量
    /// </summary>
    Task<List<SystemVariable>> GetAllAsync();

    /// <summary>
    /// 根据键名获取变量
    /// </summary>
    Task<SystemVariable?> GetByKeyAsync(string key);

    /// <summary>
    /// 根据ID获取变量
    /// </summary>
    Task<SystemVariable?> GetByIdAsync(string id);

    /// <summary>
    /// 设置或更新变量
    /// </summary>
    Task<SystemVariable> SetAsync(string key, string value, string description = "", string category = "General");

    /// <summary>
    /// 根据ID删除变量
    /// </summary>
    Task<bool> DeleteAsync(string id);

    /// <summary>
    /// 根据键名删除变量
    /// </summary>
    Task<bool> DeleteByKeyAsync(string key);
}
