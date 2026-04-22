using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Services.Data;

public class ProcessorVersionService : IProcessorVersionService
{
    private readonly IDataService _dataService;
    private readonly IProcessorService _processorService;
    private readonly ISqlTemplateService _sqlTemplateService;
    private const string FileName = "processor-versions.json";

    public ProcessorVersionService(
        IDataService dataService,
        IProcessorService processorService,
        ISqlTemplateService sqlTemplateService)
    {
        _dataService = dataService;
        _processorService = processorService;
        _sqlTemplateService = sqlTemplateService;
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
        if (processor == null) return null;

        string templateName = processor.Name;
        if (!string.IsNullOrEmpty(processor.SqlTemplateId))
        {
            if (processor.SqlTemplateType == SqlTemplateType.System)
            {
                var templates = await _sqlTemplateService.GetSystemTemplatesAsync();
                var t = templates.FirstOrDefault(x => x.Id == processor.SqlTemplateId);
                if (t != null) templateName = t.Name;
            }
            else
            {
                var templates = await _sqlTemplateService.GetCustomTemplatesAsync();
                var t = templates.FirstOrDefault(x => x.Id == processor.SqlTemplateId);
                if (t != null) templateName = t.Name;
            }
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
            DatabaseTypes = new List<string>(processor.DatabaseTypes),
            EventCodes = new List<string>(processor.EventCodes),
            Code = processor.Code,
            SqlTemplate = processor.SqlTemplate,
            SqlTemplateId = processor.SqlTemplateId,
            SqlTemplateType = processor.SqlTemplateType,
            SqlTemplateName = templateName,
            Enabled = processor.Enabled,
            Description = processor.Description,
            CreatedAt = DateTime.Now
        };

        allVersions.Add(version);
        await _dataService.WriteAsync(FileName, allVersions);
        return version;
    }

    public async Task<RollbackResult?> RollbackAsync(string processorId, string versionId, RollbackOptions? options = null)
    {
        options ??= new RollbackOptions();
        var result = new RollbackResult();

        var allVersions = await _dataService.ReadAsync<JsProcessorVersion>(FileName);
        var version = allVersions.FirstOrDefault(v => v.Id == versionId && v.ProcessorId == processorId);
        if (version == null) return null;

        var processor = await _processorService.GetByIdAsync(processorId);
        if (processor == null) return null;

        if (options.RestoreMetadata)
        {
            processor.Name = version.Name;
            processor.Enabled = version.Enabled;
            processor.Description = version.Description;
        }

        if (options.RestoreDatabaseTypes)
        {
            processor.DatabaseTypes = new List<string>(version.DatabaseTypes);
        }

        if (options.RestoreEventCodes)
        {
            processor.EventCodes = new List<string>(version.EventCodes);
        }

        if (options.RestoreCode)
        {
            processor.Code = version.Code;
        }

        if (options.RestoreSqlTemplate)
        {
            processor.SqlTemplateType = version.SqlTemplateType;
            processor.SqlTemplateId = version.SqlTemplateId;
            processor.SqlTemplate = version.SqlTemplate;

            if (version.SqlTemplateType == SqlTemplateType.Custom
                && !string.IsNullOrEmpty(version.SqlTemplateId))
            {
                var customTemplates = await _sqlTemplateService.GetCustomTemplatesAsync();
                if (!customTemplates.Any(t => t.Id == version.SqlTemplateId))
                {
                    await _sqlTemplateService.CreateCustomAsync(new CustomSqlTemplate
                    {
                        Id = version.SqlTemplateId,
                        Name = version.SqlTemplateName,
                        SqlTemplate = version.SqlTemplate
                    });
                    result.RecoveredTemplates.Add(version.SqlTemplateId);
                }
            }

            if (version.SqlTemplateType == SqlTemplateType.System
                && !string.IsNullOrEmpty(version.SqlTemplateId))
            {
                var systemTemplates = await _sqlTemplateService.GetSystemTemplatesAsync();
                if (!systemTemplates.Any(t => t.Id == version.SqlTemplateId))
                {
                }
            }
        }

        if (options.RestoreEventCodes)
        {
            var currentEventCodes = await _dataService.ReadTemplateAsync<EventCode>("eventcodes.json");
            var currentEventCodeStrings = currentEventCodes.Select(e => e.Code).ToHashSet();

            result.MissingEventCodes = version.EventCodes
                .Where(ec => !currentEventCodeStrings.Contains(ec))
                .ToList();
        }

        await _processorService.UpdateAsync(processorId, processor);

        result.Version = version;
        return result;
    }
}
