using Common;

namespace Advent_of_Code._2022._2;

public class Tests : TestBase
{
    private const string ExampleInput = "A Y\nB X\nC Z";
    
    [Theory]
    [InlineData(ExampleInput, "15", Level.PartOne)]
    [InlineData("C Z", "6", Level.PartOne)]
    [InlineData(null, "13052", Level.PartOne)]
    [InlineData(ExampleInput, "12", Level.PartTwo)]
    [InlineData(null, "13693", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
}

public class Solution : ISolution
{
    public Level level { get; set; }
    public string Solve(string input, Level level)
    {
        this.level = level;
        var games = input.Split('\n');
        return games
            .Select(Play)
            .Sum()
            .ToString();
    }

    private int Play(string game)
    {
        var instructions = game
            .Split(' ')
            .Select(char.Parse)
            .ToList();
        
        var opponent = GetPlay(instructions[0]);
        
        if (level == Level.PartOne)
        {
            var me = GetPlay(instructions[1]);
            var outcome = Get(opponent, me);
            return (int)outcome + (int)me;
        }
        else
        {
            var outcome = GetOutcome(instructions[1]);
            var me = Get(opponent, outcome);
            return (int)outcome + (int)me;
        }
    }

    private static PlayType Get(PlayType opponent, OutcomeType outcome)
    {
        var me = (opponent, outcome) switch
        {
            (PlayType.Rock, OutcomeType.Draw) => PlayType.Rock,
            (PlayType.Paper, OutcomeType.Draw) => PlayType.Paper,
            (PlayType.Scissors, OutcomeType.Draw) => PlayType.Scissors,

            (PlayType.Rock, OutcomeType.Loss) => PlayType.Scissors,
            (PlayType.Paper, OutcomeType.Loss) => PlayType.Rock,
            (PlayType.Scissors, OutcomeType.Loss) => PlayType.Paper,

            (PlayType.Rock, OutcomeType.Win) => PlayType.Paper,
            (PlayType.Paper, OutcomeType.Win) => PlayType.Scissors,
            (PlayType.Scissors, OutcomeType.Win) => PlayType.Rock,
            _ => throw new Exception("Wonky game.")
        };
        return me;
    }

    private static OutcomeType Get(PlayType opponent, PlayType me)
    {
        var outcome = (opponent, me) switch
        {
            (PlayType.Rock, PlayType.Rock) => OutcomeType.Draw,
            (PlayType.Paper, PlayType.Paper) => OutcomeType.Draw,
            (PlayType.Scissors, PlayType.Scissors) => OutcomeType.Draw,

            (PlayType.Rock, PlayType.Scissors) => OutcomeType.Loss,
            (PlayType.Paper, PlayType.Rock) => OutcomeType.Loss,
            (PlayType.Scissors, PlayType.Paper) => OutcomeType.Loss,

            (PlayType.Rock, PlayType.Paper) => OutcomeType.Win,
            (PlayType.Paper, PlayType.Scissors) => OutcomeType.Win,
            (PlayType.Scissors, PlayType.Rock) => OutcomeType.Win,
            _ => throw new Exception("Wonky game.")
        };
        return outcome;
    }

    PlayType GetPlay(char c) =>
        c switch
        {
            'A' or 'X' => PlayType.Rock,
            'B' or 'Y' => PlayType.Paper,
            'C' or 'Z' => PlayType.Scissors,
            _ => throw new ArgumentException($"Unmapped character: {c}")
        };
    
    OutcomeType GetOutcome(char c) =>
        c switch
        {
            'X' => OutcomeType.Loss,
            'Y' => OutcomeType.Draw,
            'Z' => OutcomeType.Win,
            _ => throw new ArgumentException($"Unmapped character: {c}")
        };
}

enum PlayType
{
    Rock = 1,
    Paper = 2,
    Scissors = 3,
}

enum OutcomeType
{
    Loss = 0,
    Draw = 3,
    Win = 6,
}