using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Services.Data;

public class ProcessorVersionService : IProcessorVersionService
{
    private readonly IDataService _dataService;
    private readonly IProcessorService _processorService;
    private const string FileName = "processor-versions.json";

    public ProcessorVersionService(IDataService dataService, IProcessorService processorService)
    {
        _dataService = dataService;
        _processorService = processorService;
    }

    public async Task<List<JsProcessorVersion>> GetVersionsAsync(string processorId)
    {
        var allVersions = await _dataService.ReadAsync<JsProcessorVersion>(FileName);
        return allVersions
            .Where(v => v.ProcessorId == processorId)
            .OrderByDescending(v => v.Version)
            .ToList();
    }

    public async Task<JsProcessorVersion?> GetVersionAsync(string versionId)
    {
        var allVersions = await _dataService.ReadAsync<JsProcessorVersion>(FileName);
        return allVersions.FirstOrDefault(v => v.Id == versionId);
    }

    public async Task<JsProcessorVersion?> CommitAsync(string processorId, string commitMessage)
    {
        var processor = await _processorService.GetByIdAsync(processorId);
        if (processor == null)
        {
            return null;
        }

        var allVersions = await _dataService.ReadAsync<JsProcessorVersion>(FileName);
        var processorVersions = allVersions.Where(v => v.ProcessorId == processorId).ToList();
        var nextVersion = processorVersions.Count > 0 ? processorVersions.Max(v => v.Version) + 1 : 1;

        var version = new JsProcessorVersion
        {
            Id = Guid.NewGuid().ToString(),
            ProcessorId = processorId,
            Version = nextVersion,
            CommitMessage = commitMessage,
            Name = processor.Name,
            Code = processor.Code,
            SqlTemplate = processor.SqlTemplate,
            SqlTemplateId = processor.SqlTemplateId,
            SqlTemplateType = processor.SqlTemplateType,
            CreatedAt = DateTime.Now
        };

        allVersions.Add(version);
        await _dataService.WriteAsync(FileName, allVersions);
        return version;
    }

    public async Task<JsProcessorVersion?> RollbackAsync(string processorId, string versionId)
    {
        var allVersions = await _dataService.ReadAsync<JsProcessorVersion>(FileName);
        var version = allVersions.FirstOrDefault(v => v.Id == versionId && v.ProcessorId == processorId);
        if (version == null)
        {
            return null;
        }

        var processor = await _processorService.GetByIdAsync(processorId);
        if (processor == null)
        {
            return null;
        }

        // Update processor to the version's state
        processor.Code = version.Code;
        processor.SqlTemplateId = version.SqlTemplateId;
        processor.SqlTemplateType = version.SqlTemplateType;
        // Note: SqlTemplate is loaded dynamically, so we don't persist it directly
        
        await _processorService.UpdateAsync(processorId, processor);
        return version;
    }
}
