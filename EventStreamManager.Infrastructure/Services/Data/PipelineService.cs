using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Services.Data;

public class PipelineService : IPipelineService
{
    private readonly IDataService _dataService;
    private const string FileName = "processorPipelines.json";

    public PipelineService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<List<ProcessorPipeline>> GetAllAsync()
    {
        return await _dataService.ReadAsync<ProcessorPipeline>(FileName);
    }

    public async Task<ProcessorPipeline?> GetByIdAsync(string id)
    {
        var list = await GetAllAsync();
        return list.FirstOrDefault(p => p.Id == id);
    }

    public async Task<ProcessorPipeline?> GetMatchingAsync(string eventCode, string databaseType)
    {
        var list = await GetAllAsync();
        return list
            .Where(p => p.Enabled)
            .Where(p => p.EventCodes.Count == 0 || p.EventCodes.Contains(eventCode))
            .Where(p => p.DatabaseTypes.Count == 0 || p.DatabaseTypes.Contains(databaseType))
            .FirstOrDefault();
    }

    public async Task<ProcessorPipeline> CreateAsync(ProcessorPipeline pipeline)
    {
        var list = await GetAllAsync();
        pipeline.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
        NormalizeStages(pipeline);
        list.Add(pipeline);
        await _dataService.WriteAsync(FileName, list);
        return pipeline;
    }

    public async Task<bool> UpdateAsync(string id, ProcessorPipeline pipeline)
    {
        if (id != pipeline.Id) return false;
        var list = await GetAllAsync();
        var index = list.FindIndex(p => p.Id == id);
        if (index < 0) return false;
        NormalizeStages(pipeline);
        list[index] = pipeline;
        await _dataService.WriteAsync(FileName, list);
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var list = await GetAllAsync();
        var item = list.FirstOrDefault(p => p.Id == id);
        if (item == null) return false;
        list.Remove(item);
        await _dataService.WriteAsync(FileName, list);
        return true;
    }

    public async Task<bool> ToggleAsync(string id)
    {
        var list = await GetAllAsync();
        var item = list.FirstOrDefault(p => p.Id == id);
        if (item == null) return false;
        item.Enabled = !item.Enabled;
        await _dataService.WriteAsync(FileName, list);
        return true;
    }

    private static void NormalizeStages(ProcessorPipeline pipeline)
    {
        var ordered = pipeline.Stages.OrderBy(s => s.SortOrder).ToList();
        for (int i = 0; i < ordered.Count; i++)
        {
            ordered[i].SortOrder = i;
        }
        pipeline.Stages = ordered;

        var senderCount = pipeline.Stages.Count(s => s.IsSender);
        if (senderCount == 0 && pipeline.Stages.Count > 0)
        {
            pipeline.Stages[^1].IsSender = true;
        }
    }
}
