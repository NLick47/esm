namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IDataService
{
    Task<List<T>> ReadAsync<T>(string fileName);
    Task WriteAsync<T>(string fileName, List<T> data);

    void ClearCache(string fileName = null);
    
    Task<List<T>> ReadTemplateAsync<T>(string templateFileName);
    
    Task<T> ReadTemplateSingleAsync<T>(string templateFileName) where T : class;
}