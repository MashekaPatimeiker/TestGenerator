using NUnit.Framework;
using TestGenerator.Core.Generation;
using TestGenerator.Core.Pipeline;
using TestGenerator.Core.Models;

namespace TestGenerator.Tests.Pipeline;

[TestFixture]
public class TestGenerationPipelineTests
{
    private string _tempInputDir;
    private string _tempOutputDir;
    
    [SetUp]
    public void SetUp()
    {
        _tempInputDir = Path.Combine(Path.GetTempPath(), $"TestInput_{Guid.NewGuid()}");
        _tempOutputDir = Path.Combine(Path.GetTempPath(), $"TestOutput_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempInputDir);
        Directory.CreateDirectory(_tempOutputDir);
    }
    
    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_tempInputDir))
            Directory.Delete(_tempInputDir, true);
        if (Directory.Exists(_tempOutputDir))
            Directory.Delete(_tempOutputDir, true);
    }
    
    [Test]
    public async Task ProcessAsync_WithSingleFile_ShouldGenerateTestFile()
    {
        var testCode = @"
namespace MyApp
{
    public class MyClass
    {
        public void DoSomething() { }
    }
}";
        var inputFile = Path.Combine(_tempInputDir, "MyClass.cs");
        await File.WriteAllTextAsync(inputFile, testCode);
        
        var pipeline = new TestGenerationPipeline(new NUnitTestGenerator());
        
        await pipeline.ProcessAsync(new[] { inputFile }, _tempOutputDir);
        
        var outputFile = Path.Combine(_tempOutputDir, "MyClassTests.cs");
        Assert.That(File.Exists(outputFile), Is.True);
        
        var content = await File.ReadAllTextAsync(outputFile);
        Assert.That(content, Does.Contain("[TestFixture]"));
        Assert.That(content, Does.Contain("public class MyClassTests"));
    }
    
    [Test]
    public async Task ProcessAsync_WithMultipleClassesInOneFile_ShouldGenerateMultipleTestFiles()
    {
        var testCode = @"
namespace MyApp
{
    public class ClassA { public void MethodA() { } }
    public class ClassB { public void MethodB() { } }
    public class ClassC { public void MethodC() { } }
}";
        var inputFile = Path.Combine(_tempInputDir, "Multiple.cs");
        await File.WriteAllTextAsync(inputFile, testCode);
        
        var pipeline = new TestGenerationPipeline(new NUnitTestGenerator());
        
        await pipeline.ProcessAsync(new[] { inputFile }, _tempOutputDir);
        
        Assert.That(File.Exists(Path.Combine(_tempOutputDir, "ClassATests.cs")), Is.True);
        Assert.That(File.Exists(Path.Combine(_tempOutputDir, "ClassBTests.cs")), Is.True);
        Assert.That(File.Exists(Path.Combine(_tempOutputDir, "ClassCTests.cs")), Is.True);
    }
    
    [Test]
    public async Task ProcessAsync_WithMultipleInputFiles_ShouldProcessAll()
    {
        var file1 = Path.Combine(_tempInputDir, "File1.cs");
        var file2 = Path.Combine(_tempInputDir, "File2.cs");
        
        await File.WriteAllTextAsync(file1, "public class Foo { public void Bar() { } }");
        await File.WriteAllTextAsync(file2, "public class Baz { public void Qux() { } }");
        
        var pipeline = new TestGenerationPipeline(new NUnitTestGenerator());
        
        await pipeline.ProcessAsync(new[] { file1, file2 }, _tempOutputDir);
        
        
        Assert.That(File.Exists(Path.Combine(_tempOutputDir, "FooTests.cs")), Is.True);
        Assert.That(File.Exists(Path.Combine(_tempOutputDir, "BazTests.cs")), Is.True);
    }
    
    [Test]
    public async Task ProcessAsync_WithParallelismLimits_ShouldRespectMaxDegree()
    {
        var files = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            var filePath = Path.Combine(_tempInputDir, $"Class{i}.cs");
            await File.WriteAllTextAsync(filePath, $"public class Class{i} {{ public void Method{i}() {{ }} }}");
            files.Add(filePath);
        }
        
        var pipeline = new TestGenerationPipeline(
            new NUnitTestGenerator(),
            maxReadingFiles: 1,
            maxProcessingTasks: 1,
            maxWritingFiles: 1);
        
        Assert.DoesNotThrowAsync(async () => 
            await pipeline.ProcessAsync(files, _tempOutputDir));
        
        for (int i = 0; i < 10; i++)
        {
            Assert.That(File.Exists(Path.Combine(_tempOutputDir, $"Class{i}Tests.cs")), Is.True);
        }
    }
}