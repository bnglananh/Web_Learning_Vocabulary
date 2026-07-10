using Web_Learning_Vocabulary.Models;

namespace Web_Learning_Vocabulary.Services;

public interface IVocabularyGeneratorService
{
    Task<VocabularyGenerateResponse> GenerateLearningMaterialAsync(VocabularyGenerateRequest request);
}
