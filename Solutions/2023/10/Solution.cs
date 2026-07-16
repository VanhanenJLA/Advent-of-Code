using Common;

namespace Advent_of_Code._2023._10;

public class Tests : TestBase
{
    private const string Example = "..F7.\n.FJ|.\nSJ.L7\n|F--J\nLJ...";

    [Theory]
    [InlineData(Example, "8", Level.PartOne)]
    [InlineData(null, "6828", Level.PartOne)]
    [InlineData(null, "TBD", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
}

public class Solution : ISolution
{
    char[][] maze;

    public string Solve(string input, Level level)
    {
        var rows = input
            .Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        maze = rows.Select(r => r.ToArray()).ToArray();
        var start = FindStart();

        var previous = start;
        var current = FindConnectedNeighbor(start);

        var steps = 1;
        var area = 0L;

        while (current != start)
        {
            area += (long)previous.x * current.y - (long)current.x * previous.y;
            var next = Next(current, previous);
            previous = current;
            current = next;
            steps++;
        }
        
        area += (long)previous.x * start.y - (long)start.x * previous.y;
        area = Math.Abs(area);
        var interior = (area - steps + 2) / 2;
        var farthestWithinCycle = steps / 2;

        if (level == Level.PartTwo)
            return interior.ToString();

        return farthestWithinCycle.ToString();
    }

    private (int x, int y) FindConnectedNeighbor((int x, int y) start)
    {
        var directions = new[]
        {
            (dx: 0, dy: -1),
            (dx: 1, dy: 0),
            (dx: 0, dy: 1),
            (dx: -1, dy: 0),
        };

        foreach (var direction in directions)
        {
            var neighbor = (x: start.x + direction.dx, y: start.y + direction.dy);
            if (!IsInside(neighbor))
                continue;

            var tile = maze[neighbor.y][neighbor.x];
            if (tile is '.' or 'S')
                continue;

            var exits = ExitsFor(tile);
            var backToStart = (dx: start.x - neighbor.x, dy: start.y - neighbor.y);
            if (backToStart == exits.Item1 || backToStart == exits.Item2)
                return neighbor;
        }

        throw new InvalidOperationException("No connected neighbor found for start tile");
    }

    private bool IsInside((int x, int y) position)
    {
        return position.y >= 0
               && position.y < maze.Length
               && position.x >= 0
               && position.x < maze[position.y].Length;
    }

    private (int x, int y) FindStart()
    {
        for (var y = 0; y < maze.Length; y++)
        for (var x = 0; x < maze[y].Length; x++)
            if (maze[y][x] == 'S')
                return (x, y);

        throw new InvalidOperationException("No start tile found");
    }

    private (int x, int y) Next((int x, int y) current, (int x, int y) previous)
    {
        var c = maze[current.y][current.x];
        var exits = ExitsFor(c);

        var cameFrom = (dx: previous.x - current.x, dy: previous.y - current.y);

        var deltaNext =
            cameFrom == exits.Item1
                ? exits.Item2
                : cameFrom == exits.Item2
                    ? exits.Item1
                    : throw new InvalidOperationException($"Tile {c} is not connected to previous position");

        return (current.x + deltaNext.dx, current.y + deltaNext.dy);
    }

    private static ((int dx, int dy), (int dx, int dy)) ExitsFor(char c)
    {
        var exits = c switch
        {
            '|' => ((dx: 0, dy: -1), (dx: 0, dy: 1)),
            '-' => ((dx: -1, dy: 0), (dx: 1, dy: 0)),
            'L' => ((dx: 0, dy: -1), (dx: 1, dy: 0)),
            'J' => ((dx: 0, dy: -1), (dx: -1, dy: 0)),
            '7' => ((dx: -1, dy: 0), (dx: 0, dy: 1)),
            'F' => ((dx: 1, dy: 0), (dx: 0, dy: 1)),
            'S' => throw new InvalidOperationException("Start tile exits must be resolved before calling Next"),
            '.' => throw new InvalidOperationException("Ground tile has no exits"),

            _ => throw new InvalidOperationException($"Unexpected tile {c}")
        };
        return exits;
    }
}