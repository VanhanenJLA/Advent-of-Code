namespace Common;

public static class Constants
{
    public const string InstructionsTemplateName = "instructions_template.html";
    public const string InstructionsFilename = "puzzle_instructions.html";
    public const string CookieFilename = "cookie.txt";
    public const string InputClassTemplateName = "input_template.txt";
    public const string InputTextFileName = "input.txt";
    
    public const string YearToken = "¤YEAR¤";
    public const string DayToken = "¤DAY¤";
    public const string InputToken = "¤INPUT¤";
    
    public const string IncorrectAnswerResponse = "That's not the right answer";
    
    // <article><p>That's the right answer!  You are <span class="day-success">one gold star</span> closer to restoring snow operations. <a href="/2023/day/4#part2">[Continue to Part Two]</a></p></article>
    public const string CorrectAnswerResponse = "That's the right answer";
    public const string TooLow = "your answer is too low";
    public const string TooHigh = "your answer is too high";
}