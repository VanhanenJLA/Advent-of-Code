using System.Reflection;
using Common;

namespace CLI;

public interface ISolutionResolver
{
    ISolution Resolve(int year, int day);
}

public class SolutionResolver : ISolutionResolver
{
    public ISolution Resolve(int year, int day)
    {
        var assembly = Assembly.Load("Solutions");
        var solutionName = $"Advent_of_Code._{year}._{day}.Solution";
        var type = assembly.GetType(solutionName)
                   ?? throw new InvalidOperationException($"Solution '{solutionName}' not found.");

        return Activator.CreateInstance(type) as ISolution
               ?? throw new InvalidOperationException($"Solution '{solutionName}' does not implement {nameof(ISolution)}.");
    }
}
