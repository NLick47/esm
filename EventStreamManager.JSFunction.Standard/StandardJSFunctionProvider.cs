namespace EventStreamManager.JSFunction.Standard;

public class StandardJsFunctionProvider : IJsFunctionProvider
{
    
    public string Name => "Standard JS Functions";
    public string Description => "提供基础的JS扩展函数Base64、JSON等";
    public string Version => "1.0.0";

    private readonly IEnumerable<FunctionMetadata> _functions;

    public StandardJsFunctionProvider()
    {
        _functions = LoadFunctions();
    }
    
    
    public IEnumerable<FunctionMetadata> GetFunctions() => _functions;
   
    
    private IEnumerable<FunctionMetadata> LoadFunctions()
    {
        foreach (var func in Base64Functions.GetFunctions())
            yield return func;

        foreach (var func in JsonFunctions.GetFunctions())
            yield return func;

        foreach (var func in StringFunctions.GetFunctions())
            yield return func;

        foreach (var func in MathFunctions.GetFunctions())
            yield return func;

        foreach (var func in DateTimeFunctions.GetFunctions())
            yield return func;
    }
}