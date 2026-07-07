using Common;

namespace Advent_of_Code._2023._7;

public class Tests : TestBase
{
    private const string Example = "32T3K 765\nT55J5 684\nKK677 28\nKTJJT 220\nQQQJA 483";

    [Theory]
    [InlineData(null, "249638405", Level.PartOne)]
    [InlineData(null, "TBD", Level.PartTwo)]
    [InlineData(Example, "6440", Level.PartOne)]
    public override void Should_solve_correct_answer(string? input, string expected, Level level)
    {
        DefaultTest(input, expected, level);
    }
}

public class Solution : ISolution
{
    public string Solve(string input, Level level)
    {
        return Parse(input)
            .Order(new HandComparer())
            .Select((hand, index) => hand.Bid * (index + 1))
            .Sum()
            .ToString();
    }

    private static IEnumerable<Hand> Parse(string input)
    {
        return input.Split('\n')
            .Select(ParseHand);
    }

    private static Hand ParseHand(string row)
    {
        var parts = row.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var cards = parts.First();
        var bid = int.Parse(parts.Last());
        IReadOnlyList<CardType> cardTypes = cards.Select(ParseCard).ToList();
        var handType = FindHandType(cardTypes);
        return new Hand(cardTypes, bid, handType);
    }

    private static HandType FindHandType(IEnumerable<CardType> cards)
    {
        var counts = cards
            .GroupBy(card => card)
            .Select(group => group.Count())
            .OrderDescending()
            .ToArray();

        return counts switch
        {
            [5] => HandType.FiveOfAKind,
            [4, 1] => HandType.FourOfAKind,
            [3, 2] => HandType.FullHouse,
            [3, 1, 1] => HandType.ThreeOfAKind,
            [2, 2, 1] => HandType.TwoPair,
            [2, 1, 1, 1] => HandType.OnePair,
            [1, 1, 1, 1, 1] => HandType.HighCard,
            _ => throw new ArgumentOutOfRangeException(nameof(cards), cards, null)
        };
    }
    
    public enum CardType
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    private static CardType ParseCard(char card) => card switch
    {
        'A' => CardType.Ace,
        'K' => CardType.King,
        'Q' => CardType.Queen,
        'J' => CardType.Jack,
        'T' => CardType.Ten,
        '9' => CardType.Nine,
        '8' => CardType.Eight,
        '7' => CardType.Seven,
        '6' => CardType.Six,
        '5' => CardType.Five,
        '4' => CardType.Four,
        '3' => CardType.Three,
        '2' => CardType.Two,
        _ => throw new ArgumentOutOfRangeException(nameof(card), card, null)
    };

    public enum HandType
    {
        HighCard,
        OnePair,
        TwoPair,
        ThreeOfAKind,
        FullHouse,
        FourOfAKind,
        FiveOfAKind
    }

    public record Hand(
        IReadOnlyList<CardType> Cards,
        int Bid,
        HandType Type
    );

    private class HandComparer : IComparer<Hand>
    {
        public int Compare(Hand? x, Hand? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;
            
            var typeComparison = x.Type.CompareTo(y.Type);
            if (typeComparison != 0)
                return typeComparison;

            for (var i = 0; i < x.Cards.Count; i++)
            {
                var cardComparison = x.Cards[i].CompareTo(y.Cards[i]);
                if (cardComparison != 0)
                    return cardComparison;
            }

            return 0;
        }
    }
}
