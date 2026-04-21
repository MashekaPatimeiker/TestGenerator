using TestGenerator.Core.Generation;
using TestGenerator.Core.Pipeline;

namespace TestGenerator.Console;

class Program
{
    static async Task Main(string[] args)
    {
        if (args.Length < 2)
        {
            System.Console.WriteLine("Usage: dotnet run -- <input-pattern> <output-directory>");
            System.Console.WriteLine("Example: dotnet run -- \"*.cs\" \"./GeneratedTests\"");
            return;
        }
        
        var inputPattern = args[0];
        var outputDirectory = args[1];
        
        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), inputPattern, SearchOption.AllDirectories);
        
        if (files.Length == 0)
        {
            System.Console.WriteLine($"No files found: {inputPattern}");
            return;
        }
        
        System.Console.WriteLine($"Found {files.Length} files");
        
        var pipeline = new TestGenerationPipeline(new NUnitTestGenerator());
        await pipeline.ProcessAsync(files, outputDirectory);
        
        System.Console.WriteLine("Done!");
    }
}