using System.Text.Json.Serialization;

namespace Web_Learning_Vocabulary.Models;

public class BlankItem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("correct_answer")]
    public string CorrectAnswer { get; set; } = string.Empty;

    [JsonPropertyName("word_type")]
    public string WordType { get; set; } = string.Empty;

    [JsonPropertyName("hint_vi")]
    public string HintVi { get; set; } = string.Empty;
}
