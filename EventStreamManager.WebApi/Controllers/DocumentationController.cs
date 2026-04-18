using EventStreamManager.JSFunction.Runtime;
using EventStreamManager.JSFunction;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentationController : BaseController
{
    private readonly JsFunctionRegistry _functionRegistry;
    private readonly IEnumerable<IJsFunctionProvider> _providers;
    private readonly ILogger<DocumentationController> _logger;

    public DocumentationController(
        JsFunctionRegistry functionRegistry,
        IEnumerable<IJsFunctionProvider> providers,
        ILogger<DocumentationController> logger)
    {
        _functionRegistry = functionRegistry;
        _providers = providers;
        _logger = logger;
    }

    /// <summary>
    /// 获取所有可用的库
    /// </summary>
    [HttpGet("libraries")]
    public IActionResult GetLibraries()
    {
        try
        {
            var libraries = _providers.Select(p => new
            {
                name = p.Name,
                description = p.Description,
                version = p.Version,
                functionCount = p.GetFunctions().Count()
            }).ToList();

            return Ok(libraries, "获取库列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取库列表失败");
            return Fail("获取库列表失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 获取所有分类
    /// </summary>
    [HttpGet("categories")]
    public IActionResult GetCategories()
    {
        try
        {
            var categories = _functionRegistry.GetAllCategories().ToList();
            return Ok(categories, "获取分类列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取分类列表失败");
            return Fail("获取分类列表失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 获取所有函数
    /// </summary>
    [HttpGet("functions")]
    public IActionResult GetFunctions([FromQuery] string? library = null, [FromQuery] string? category = null)
    {
        try
        {
            var functions = _functionRegistry.GetAvailableFunctions();

            // 按库过滤
            if (!string.IsNullOrEmpty(library))
            {
                functions = functions.Where(f => f.ProviderName.Equals(library, StringComparison.OrdinalIgnoreCase));
            }

            // 按分类过滤
            if (!string.IsNullOrEmpty(category))
            {
                functions = functions.Where(f => f.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var result = functions.Select(f => new
            {
                name = f.Name,
                description = f.Description,
                category = f.Category,
                example = f.Example,
                providerName = f.ProviderName,
                providerVersion = f.ProviderVersion,
                returnType = f.ReturnType.Name,
                parameters = f.Parameters.Select(p => new
                {
                    name = p.Name,
                    type = p.Type.Name,
                    description = p.Description,
                    isOptional = p.IsOptional,
                    defaultValue = p.DefaultValue
                })
            }).ToList();

            return Ok(result, "获取函数列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取函数列表失败");
            return Fail("获取函数列表失败: " + ex.Message);
        }
    }

    /// <summary>
    /// 获取完整的文档结构
    /// </summary>
    [HttpGet("structure")]
    public IActionResult GetStructure()
    {
        try
        {
            var libraries = _providers.Select(provider =>
            {
                var functions = provider.GetFunctions().ToList();
                var categories = functions
                    .GroupBy(f => f.Category)
                    .Select(g => new
                    {
                        name = g.Key,
                        functions = g.Select(f => new
                        {
                            name = f.Name,
                            description = f.Description,
                            example = f.Example,
                            returnType = f.ReturnType.Name,
                            parameters = f.Parameters.Select(p => new
                            {
                                name = p.Name,
                                type = p.Type.Name,
                                description = p.Description,
                                isOptional = p.IsOptional,
                                defaultValue = p.DefaultValue
                            })
                        })
                    });

                return new
                {
                    name = provider.Name,
                    description = provider.Description,
                    version = provider.Version,
                    categories
                };
            }).ToList();

            return Ok(libraries, "获取文档结构成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文档结构失败");
            return Fail("获取文档结构失败: " + ex.Message);
        }
    }
}
