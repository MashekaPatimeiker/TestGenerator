using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGenerator.Core.Models;

namespace TestGenerator.Core.Parsing;

public class CodeParser
{
    public async Task<List<ClassInfo>> ParseFileAsync(string filePath)
    {
        var code = await File.ReadAllTextAsync(filePath);
        var tree = CSharpSyntaxTree.ParseText(code);
        var root = await tree.GetRootAsync();
        
        if (root is not CompilationUnitSyntax compilationUnit)
            return new List<ClassInfo>();
        
        var classes = new List<ClassInfo>();
        
        var classDeclarations = compilationUnit.DescendantNodes()
            .OfType<ClassDeclarationSyntax>();
        
        foreach (var classDecl in classDeclarations)
        {
            var classInfo = ParseClass(classDecl, filePath);
            classes.Add(classInfo);
        }
        
        return classes;
    }
    
    private ClassInfo ParseClass(ClassDeclarationSyntax classDecl, string filePath)
    {
        var classInfo = new ClassInfo
        {
            Name = classDecl.Identifier.Text,
            SourceFilePath = filePath,
            Namespace = GetNamespace(classDecl)
        };
        
        var methods = classDecl.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword));
        
        foreach (var method in methods)
        {
            var methodInfo = new MethodInfo
            {
                Name = method.Identifier.Text,
                ReturnType = method.ReturnType.ToString()
            };
            
            foreach (var param in method.ParameterList.Parameters)
            {
                methodInfo.Parameters.Add(new ParameterInfo
                {
                    Type = param.Type?.ToString() ?? "object",
                    Name = param.Identifier.Text
                });
            }
            
            classInfo.PublicMethods.Add(methodInfo);
        }
        
        var constructors = classDecl.DescendantNodes()
            .OfType<ConstructorDeclarationSyntax>();
        
        foreach (var ctor in constructors)
        {
            if (ctor.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                foreach (var param in ctor.ParameterList.Parameters)
                {
                    classInfo.ConstructorParameters.Add(new ConstructorParameterInfo
                    {
                        Type = param.Type?.ToString() ?? "object",
                        Name = param.Identifier.Text
                    });
                }
                break;
            }
        }
        
        return classInfo;
    }
    
    private string GetNamespace(ClassDeclarationSyntax classDecl)
    {
        return classDecl.Parent switch
        {
            NamespaceDeclarationSyntax namespaceDecl => namespaceDecl.Name.ToString(),
            FileScopedNamespaceDeclarationSyntax fileScopedNamespace => fileScopedNamespace.Name.ToString(),
            _ => string.Empty
        };
    }
}