using NUnit.Framework;
using TestGenerator.Core.Parsing;
using TestGenerator.Core.Models;

namespace TestGenerator.Tests.Parsing;

[TestFixture]
public class CodeParserTests
{
    private CodeParser _parser;
    private string _testFilePath;
    
    [SetUp]
    public void SetUp()
    {
        _parser = new CodeParser();
        _testFilePath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.cs");
    }
    
    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testFilePath))
            File.Delete(_testFilePath);
    }
    
    [Test]
    public async Task ParseFileAsync_WhenFileHasSingleClass_ShouldReturnOneClass()
    {
        var code = @"
using System;

namespace MyNamespace
{
    public class MyClass
    {
        public void MyMethod() { }
    }
}";
        await File.WriteAllTextAsync(_testFilePath, code);
        
        var result = await _parser.ParseFileAsync(_testFilePath);
        
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].Name, Is.EqualTo("MyClass"));
        Assert.That(result[0].Namespace, Is.EqualTo("MyNamespace"));
    }
    
    [Test]
    public async Task ParseFileAsync_WhenFileHasMultipleClasses_ShouldReturnAllClasses()
    {
        var code = @"
namespace MyNamespace
{
    public class ClassA { }
    public class ClassB { }
    internal class ClassC { }
}";
        await File.WriteAllTextAsync(_testFilePath, code);
        
        var result = await _parser.ParseFileAsync(_testFilePath);
        
        Assert.That(result.Count, Is.EqualTo(2)); 
        Assert.That(result.Any(c => c.Name == "ClassA"));
        Assert.That(result.Any(c => c.Name == "ClassB"));
    }
    
    [Test]
    public async Task ParseFileAsync_WhenClassHasPublicMethods_ShouldParseThem()
    {
        var code = @"
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public void Clear() { }
    private void Helper() { }
}";
        await File.WriteAllTextAsync(_testFilePath, code);
        
        var result = await _parser.ParseFileAsync(_testFilePath);
        
        var calculator = result[0];
        Assert.That(calculator.PublicMethods.Count, Is.EqualTo(2));
        Assert.That(calculator.PublicMethods.Any(m => m.Name == "Add"));
        Assert.That(calculator.PublicMethods.Any(m => m.Name == "Clear"));
    }
    
    [Test]
    public async Task ParseFileAsync_WhenMethodHasParameters_ShouldParseThem()
    {
        var code = @"
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public string Concat(string first, string second) => first + second;
}";
        await File.WriteAllTextAsync(_testFilePath, code);
        
        var result = await _parser.ParseFileAsync(_testFilePath);
        
        var addMethod = result[0].PublicMethods.First(m => m.Name == "Add");
        Assert.That(addMethod.Parameters.Count, Is.EqualTo(2));
        Assert.That(addMethod.Parameters[0].Name, Is.EqualTo("a"));
        Assert.That(addMethod.Parameters[0].Type, Is.EqualTo("int"));
        Assert.That(addMethod.Parameters[1].Name, Is.EqualTo("b"));
        Assert.That(addMethod.Parameters[1].Type, Is.EqualTo("int"));
        
        var concatMethod = result[0].PublicMethods.First(m => m.Name == "Concat");
        Assert.That(concatMethod.Parameters.Count, Is.EqualTo(2));
        Assert.That(concatMethod.Parameters[0].Type, Is.EqualTo("string"));
        Assert.That(concatMethod.Parameters[1].Type, Is.EqualTo("string"));
    }
    
    [Test]
    public async Task ParseFileAsync_WhenClassHasConstructor_ShouldParseParameters()
    {
        var code = @"
public interface ILogger { }
public interface IRepository { }
public class Service
{
    public Service(ILogger logger, IRepository repo) { }
    public void DoWork() { }
}";
        await File.WriteAllTextAsync(_testFilePath, code);
    
        var result = await _parser.ParseFileAsync(_testFilePath);
    
        var service = result.First(c => c.Name == "Service");
        Assert.That(service.ConstructorParameters, Is.Not.Empty);
        Assert.That(service.ConstructorParameters.Count, Is.EqualTo(2));
        Assert.That(service.ConstructorParameters[0].Type, Is.EqualTo("ILogger"));
        Assert.That(service.ConstructorParameters[0].IsInterface, Is.True);
    }
    
    [Test]
    public void ParseFileAsync_WhenFileDoesNotExist_ShouldThrowFileNotFoundException()
    {
        var nonExistentFile = "nonexistent.cs";
    
        var ex = Assert.ThrowsAsync<FileNotFoundException>(async () => 
            await _parser.ParseFileAsync(nonExistentFile));
    
        Assert.That(ex.Message, Does.Contain("nonexistent.cs"));
    }
}