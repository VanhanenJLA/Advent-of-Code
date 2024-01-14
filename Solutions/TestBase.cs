using Common;

namespace Advent_of_Code;

public abstract class TestBase
{
    public abstract void Should_give_correct_answer(string input, string expected, Level level);
}