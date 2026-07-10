using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Web_Learning_Vocabulary.Models;

namespace Web_Learning_Vocabulary.Services;

public class VocabularyGeneratorService : IVocabularyGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<VocabularyGeneratorService> _logger;

    public VocabularyGeneratorService(HttpClient httpClient, IConfiguration configuration, ILogger<VocabularyGeneratorService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<VocabularyGenerateResponse> GenerateLearningMaterialAsync(VocabularyGenerateRequest request)
    {
        if (request.VocabularyList == null || request.VocabularyList.Count == 0)
        {
            throw new ArgumentException("Vocabulary list cannot be empty");
        }

        var geminiApiKey = _configuration["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(geminiApiKey) || geminiApiKey == "YOUR_GEMINI_API_KEY_HERE")
        {
            throw new InvalidOperationException("Chưa cấu hình Gemini API Key. Vui lòng lấy key từ Google AI Studio và điền vào appsettings.json.");
        }

        var vocabularyText = string.Join(", ", request.VocabularyList.Select(v => $"{v.EnglishWord} ({v.VietnameseMeaning})"));

        int wordCount = request.VocabularyList.Count;
        int minWords = Math.Min(wordCount, 3);
        int maxWords = Math.Min(wordCount, 6);
        string selectionInstruction = wordCount <= 3
            ? $"Use all {wordCount} provided vocabulary words naturally in a single context."
            : $"Select {minWords} to {maxWords} words from the provided vocabulary list that fit together naturally in a single context. You DO NOT need to use all the words.";

        var prompt = $@"You are an expert English teacher creating vocabulary learning materials.

CRITICAL REQUIREMENTS:
1. Generate ONLY a valid JSON object. No introductory text, explanations, or markdown.
2. English level must be CEFR B1-B2.
3. {selectionInstruction}
4. Create a logical, coherent paragraph of 60-90 words using ONLY your selected vocabulary words.
5. Replace your selected words with [BLANK_1], [BLANK_2], etc. in display_text, matching their order of appearance.
6. Provide accurate contextual Vietnamese meanings for hints.

Vocabulary to choose from:
{vocabularyText}

Generate ONLY this JSON structure (no other text):
{{
  ""title"": ""A short engaging title"",
  ""full_text"": ""The complete paragraph with all vocabulary words"",
  ""display_text"": ""Paragraph with [BLANK_1], [BLANK_2], etc."",
  ""blanks"": [
    {{
      ""id"": ""[BLANK_1]"",
      ""correct_answer"": ""word"",
      ""word_type"": ""noun/verb/adjective/adverb"",
      ""hint_vi"": ""Vietnamese meaning in context""
    }}
  ]
}}";

        var requestBody = new
        {
            system_instruction = new { parts = new[] { new { text = "You are an expert English teacher. Respond ONLY with valid JSON, no additional text." } } },
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            },
            generationConfig = new
            {
                temperature = 0.7,
                maxOutputTokens = 8192,
                responseMimeType = "application/json"
            }
        };

        try
        {
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={geminiApiKey}";
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            var jsonContent = JsonSerializer.Serialize(requestBody);
            httpRequest.Content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Gemini API failed with status {StatusCode}: {Error}", response.StatusCode, errorContent);

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest && errorContent.Contains("API_KEY_INVALID"))
                {
                    throw new InvalidOperationException("API Key của Gemini không hợp lệ. Vui lòng kiểm tra lại cấu hình trong appsettings.json.");
                }
                else
                {
                    throw new InvalidOperationException($"Lỗi từ Gemini API ({(int)response.StatusCode}). Vui lòng kiểm tra lại.");
                }
            }

            var cleanedContent = "";
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);

            var candidates = jsonDocument.RootElement.GetProperty("candidates");
            if (candidates.GetArrayLength() == 0)
            {
                throw new InvalidOperationException("Empty response from Gemini");
            }

            var firstCandidate = candidates[0];

            if (firstCandidate.TryGetProperty("finishReason", out var finishReasonProp))
            {
                var finishReason = finishReasonProp.GetString();
                if (finishReason == "SAFETY" || finishReason == "RECITATION" || finishReason == "OTHER")
                {
                    throw new InvalidOperationException($"Nội dung bị chặn bởi bộ lọc an toàn của AI (Lý do: {finishReason}). Vui lòng chọn các từ vựng khác.");
                }
                if (finishReason == "MAX_TOKENS")
                {
                    throw new InvalidOperationException("Văn bản tạo ra quá dài và bị cắt ngang (MAX_TOKENS).");
                }
            }

            var messageContent = firstCandidate
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrEmpty(messageContent))
            {
                throw new InvalidOperationException("Empty response from Gemini");
            }

            // Clean up potential markdown code blocks
            cleanedContent = messageContent
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result = JsonSerializer.Deserialize<VocabularyGenerateResponse>(cleanedContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
            {
                throw new InvalidOperationException("Failed to parse response from Gemini");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error calling Gemini API");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing Gemini response");
            var preview = "";
            try { preview = ex.Message; } catch { }
            throw new InvalidOperationException($"Lỗi JSON: {ex.Message}. Vui lòng thử lại. Nếu vẫn lỗi, thử F12 hoặc check log để xem RAW data.");
        }
    }
}
