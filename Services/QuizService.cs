using Web_Learning_Vocabulary.Models;

namespace Web_Learning_Vocabulary.Services;

public interface IQuizService
{
    Task<List<QuizQuestion>> GenerateQuizQuestionsAsync(List<VocabularyItem> vocabularies);
    Task<QuizQuestion> CreateQuestionAsync(QuizQuestion question);
    Task<bool> ValidateAnswerAsync(int questionId, string answer);
}

public class QuizService : IQuizService
{
    private static List<QuizQuestion> _questions = new();
    private static int _nextQuestionId = 1;
    private static Random _random = new Random();

    public Task<List<QuizQuestion>> GenerateQuizQuestionsAsync(List<VocabularyItem> vocabularies)
    {
        var questions = new List<QuizQuestion>();

        // Xáo trộn danh sách từ vựng để đảm bảo tính ngẫu nhiên
        var shuffledVocabs = vocabularies.OrderBy(_ => _random.Next()).ToList();

        // Chọn ngẫu nhiên loại câu hỏi đầu tiên, sau đó luân phiên
        bool isEnglishToVietnamese = _random.Next(2) == 0;

        foreach (var vocab in shuffledVocabs)
        {
            if (isEnglishToVietnamese)
            {
                // Câu hỏi loại: Tiếng Anh → Tiếng Việt
                var englishToVietnamese = new QuizQuestion
                {
                    Id = _nextQuestionId++,
                    Question = $"'{vocab.EnglishWord}' có nghĩa là gì?",
                    Options = GenerateIncorrectOptions(vocabularies, vocab.VietnameseMeaning).ToList(),
                    CorrectAnswer = vocab.VietnameseMeaning,
                    QuestionType = "english_to_vietnamese",
                    VocabularyItemId = vocab.Id
                };
                questions.Add(englishToVietnamese);
            }
            else
            {
                // Câu hỏi loại: Tiếng Việt → Tiếng Anh
                var vietnameseToEnglish = new QuizQuestion
                {
                    Id = _nextQuestionId++,
                    Question = $"'{vocab.VietnameseMeaning}' trong tiếng Anh là gì?",
                    Options = GenerateIncorrectEnglishOptions(vocabularies, vocab.EnglishWord).ToList(),
                    CorrectAnswer = vocab.EnglishWord,
                    QuestionType = "vietnamese_to_english",
                    VocabularyItemId = vocab.Id
                };
                questions.Add(vietnameseToEnglish);
            }

            // Đổi loại câu hỏi cho lần lặp tiếp theo để đảm bảo không có 2 câu cùng loại liên tiếp
            isEnglishToVietnamese = !isEnglishToVietnamese;
        }

        _questions = questions;
        return Task.FromResult(questions);
    }

    public Task<QuizQuestion> CreateQuestionAsync(QuizQuestion question)
    {
        question.Id = _nextQuestionId++;
        _questions.Add(question);
        return Task.FromResult(question);
    }

    public Task<bool> ValidateAnswerAsync(int questionId, string answer)
    {
        var question = _questions.FirstOrDefault(q => q.Id == questionId);
        if (question == null)
            return Task.FromResult(false);

        return Task.FromResult(question.CorrectAnswer.Equals(answer, StringComparison.OrdinalIgnoreCase));
    }

    private List<string> GenerateIncorrectOptions(List<VocabularyItem> vocabularies, string correctAnswer)
    {
        var options = new List<string> { correctAnswer };
        var otherMeanings = vocabularies
            .Where(v => v.VietnameseMeaning != correctAnswer)
            .Select(v => v.VietnameseMeaning)
            .OrderBy(_ => _random.Next())
            .Take(3)
            .ToList();

        options.AddRange(otherMeanings);
        return options.OrderBy(_ => _random.Next()).ToList();
    }

    private List<string> GenerateIncorrectEnglishOptions(List<VocabularyItem> vocabularies, string correctAnswer)
    {
        var options = new List<string> { correctAnswer };
        var otherWords = vocabularies
            .Where(v => v.EnglishWord != correctAnswer)
            .Select(v => v.EnglishWord)
            .OrderBy(_ => _random.Next())
            .Take(3)
            .ToList();

        options.AddRange(otherWords);
        return options.OrderBy(_ => _random.Next()).ToList();
    }
}
