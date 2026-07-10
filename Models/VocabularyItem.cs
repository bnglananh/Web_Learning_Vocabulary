namespace Web_Learning_Vocabulary.Models;

public class VocabularyItem
{
    public int Id { get; set; }
    public string EnglishWord { get; set; } = string.Empty;
    public string VietnameseMeaning { get; set; } = string.Empty;
    public string? Example { get; set; }
    public string? WordType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
