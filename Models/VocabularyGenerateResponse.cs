using System.Text.Json.Serialization;

namespace Web_Learning_Vocabulary.Models;

public class VocabularyGenerateResponse
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("full_text")]
    public string FullText { get; set; } = string.Empty;

    [JsonPropertyName("display_text")]
    public string DisplayText { get; set; } = string.Empty;

    [JsonPropertyName("blanks")]
    public List<BlankItem> Blanks { get; set; } = new();
}
