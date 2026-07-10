namespace Web_Learning_Vocabulary.Models;

public class PassageExercise
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OriginalPassage { get; set; } = string.Empty;
    public string PassageWithBlanks { get; set; } = string.Empty;
    public List<BlankWord> BlankWords { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class BlankWord
{
    public int Id { get; set; }
    public string EnglishWord { get; set; } = string.Empty;
    public string VietnameseMeaning { get; set; } = string.Empty;
    public int Position { get; set; } // Vị trí trong đoạn văn
}

public class PassageSubmission
{
    public int Id { get; set; }
    public int ExerciseId { get; set; }
    public List<BlankAnswer> Answers { get; set; } = new();
    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.Now;
}

public class BlankAnswer
{
    public int BlankId { get; set; }
    public string UserAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
