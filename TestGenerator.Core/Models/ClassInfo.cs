namespace TestGenerator.Core.Models;

public class ClassInfo
{
    public string Namespace { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<MethodInfo> PublicMethods { get; set; } = new();
    public List<ConstructorParameterInfo> ConstructorParameters { get; set; } = new();
    public string SourceFilePath { get; set; } = string.Empty;
}

public class MethodInfo
{
    public string Name { get; set; } = string.Empty;
    public string ReturnType { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public bool IsVoid => ReturnType == "void";
}

public class ParameterInfo
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class ConstructorParameterInfo
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsInterface => Type.StartsWith("I");
}