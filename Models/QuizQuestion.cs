namespace Web_Learning_Vocabulary.Models;

public class QuizQuestion
{
    public int Id { get; set; }
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
    public string QuestionType { get; set; } = "english_to_vietnamese"; // english_to_vietnamese or vietnamese_to_english
    public int VocabularyItemId { get; set; }
}

public class QuizSubmission
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string SelectedAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.Now;
}
