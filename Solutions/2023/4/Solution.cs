using Common;

namespace Advent_of_Code._2023._4;

public class Tests : TestBase
{
    const string Card1 = "Card 1: 41 48 83 86 17 | 83 86  6 31 17  9 48 53";
    const string Card2 = "Card 2: 13 32 20 16 61 | 61 30 68 82 17 32 24 19";
    const string Card3 = "Card 3:  1 21 53 59 44 | 69 82 63 72 16 21 14  1";
    const string Card4 = "Card 4: 41 92 73 84 69 | 59 84 76 51 58  5 54 83";
    const string Card5 = "Card 5: 87 83 26 28 32 | 88 30 70 12 93 22 82 36";
    const string Card6 = "Card 6: 31 18 13 56 72 | 74 77 10 23 35 67 36 11";
    const string Example = $"{Card1}\n{Card2}\n{Card3}\n{Card4}\n{Card5}\n{Card6}";

    [Theory]
    [InlineData(Example, "13", Level.PartOne)]
    [InlineData(null, "23750", Level.PartOne)]
    [InlineData(Example, "30", Level.PartTwo)]
    [InlineData(null, "13261850", Level.PartTwo)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }

    [Theory]
    [InlineData(Card1, 8)]
    [InlineData(Card2, 2)]
    [InlineData(Card3, 2)]
    [InlineData(Card4, 1)]
    [InlineData(Card5, 0)]
    [InlineData(Card6, 0)]
    public void Should_count_points(string cardString, int expected)
    {
        var card = Solution.Parse(cardString);
        var wins = Solution.WinningNumbers(card);
        var points = Solution.CountPoints(wins);
        Assert.Equal(expected, points);
    }
}

public record Card(int Id, HashSet<int> Winning, HashSet<int> Present);

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        
        var cards = input
            .Split(Environment.NewLine)
            .Select(Parse);
        
        if (level == Level.PartOne)
            return cards
                .Select(WinningNumbers)
                .Select(CountPoints)
                .Sum()
                .ToString();

        var count = 0;
        
        cards
            .ToList()
            .ForEach(WinningCards);

        count += cards.Count();
        
        return count.ToString();
        
        void WinningCards(Card card)
        {
            var (id, winning, present) = card;
            var wins = winning.Intersect(present);
            var winCount = wins.Count();
            count += winCount;

            if (winCount == 0) return;

            var ids = Enumerable.Range(id, winCount);
            foreach (var i in ids)
            {
                var c = cards.ElementAt(i);
                WinningCards(c);
            }
        }
    }
    
    public static Card Parse(string card)
    {
        var parts = card.Split(": ");
        var id = int.Parse(parts[0].Replace("Card ", ""));

        var numberParts = parts[1].Split(" | ");
        var winning = Parse(numberParts[0]);
        var present = Parse(numberParts[1]);
            
        static HashSet<int> Parse(string numbers) => numbers
            .Split(" ", StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToHashSet();

        return new Card(id, winning, present);
    }

    public static IEnumerable<int> WinningNumbers(Card card)
    {
        var (_, winning, present) = card;
        return winning.Intersect(present);
    }

    public static int CountPoints(IEnumerable<int> wins)
    {
        if (!wins.Any())
            return 0;
        return (int) Math.Pow(2, wins.Count() - 1);
    }
    
}