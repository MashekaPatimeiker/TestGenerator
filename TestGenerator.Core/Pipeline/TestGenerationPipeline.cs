using System.Threading.Tasks.Dataflow;
using TestGenerator.Core.Generation;
using TestGenerator.Core.Models;
using TestGenerator.Core.Parsing;

namespace TestGenerator.Core.Pipeline;

public class TestGenerationPipeline
{
    private readonly ITestGenerator _testGenerator;
    private readonly CodeParser _parser;
    private readonly int _maxReadingFiles;
    private readonly int _maxProcessingTasks;
    private readonly int _maxWritingFiles;
    
    public TestGenerationPipeline(
        ITestGenerator testGenerator,
        int maxReadingFiles = 2,
        int maxProcessingTasks = 0,
        int maxWritingFiles = 2)
    {
        _testGenerator = testGenerator;
        _parser = new CodeParser();
        _maxReadingFiles = maxReadingFiles;
        _maxProcessingTasks = maxProcessingTasks == 0 ? Environment.ProcessorCount : maxProcessingTasks;
        _maxWritingFiles = maxWritingFiles;
    }
    
    public async Task ProcessAsync(IEnumerable<string> inputFiles, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        
        var readingBlock = new TransformBlock<string, (string FilePath, List<ClassInfo> Classes)>(
            async filePath =>
            {
                Console.WriteLine($"[READ] {Path.GetFileName(filePath)}");
                var classes = await _parser.ParseFileAsync(filePath);
                return (filePath, classes);
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _maxReadingFiles,
                BoundedCapacity = _maxReadingFiles * 2
            });
        
        var generationBlock = new TransformManyBlock<(string FilePath, List<ClassInfo> Classes), (ClassInfo Class, string TestCode)>(
            input =>
            {
                var results = new List<(ClassInfo, string)>();
                foreach (var classInfo in input.Classes)
                {
                    Console.WriteLine($"[GEN] {classInfo.Name}");
                    var testCode = _testGenerator.GenerateTestClass(classInfo);
                    results.Add((classInfo, testCode));
                }
                return results;
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _maxProcessingTasks,
                BoundedCapacity = _maxProcessingTasks * 2
            });
        
        var writingBlock = new ActionBlock<(ClassInfo Class, string TestCode)>(
            async item =>
            {
                var outputPath = Path.Combine(outputDirectory, $"{item.Class.Name}Tests.cs");
                Console.WriteLine($"[WRITE] {item.Class.Name}Tests.cs");
                await File.WriteAllTextAsync(outputPath, item.TestCode);
            },
            new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = _maxWritingFiles,
                BoundedCapacity = _maxWritingFiles * 2
            });
        
        readingBlock.LinkTo(generationBlock, new DataflowLinkOptions { PropagateCompletion = true });
        generationBlock.LinkTo(writingBlock, new DataflowLinkOptions { PropagateCompletion = true });
        
        foreach (var file in inputFiles)
        {
            await readingBlock.SendAsync(file);
        }
        
        readingBlock.Complete();
        await writingBlock.Completion;
    }
}