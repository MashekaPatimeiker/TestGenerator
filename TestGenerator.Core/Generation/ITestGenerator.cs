using TestGenerator.Core.Models;

namespace TestGenerator.Core.Generation;

public interface ITestGenerator
{
    string GenerateTestClass(ClassInfo classInfo);
}