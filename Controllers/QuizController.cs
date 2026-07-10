using Microsoft.AspNetCore.Mvc;
using Web_Learning_Vocabulary.Models;
using Web_Learning_Vocabulary.Services;

namespace Web_Learning_Vocabulary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;
    private readonly IVocabularyService _vocabularyService;

    public QuizController(IQuizService quizService, IVocabularyService vocabularyService)
    {
        _quizService = quizService;
        _vocabularyService = vocabularyService;
    }

    [HttpPost("generate")]
    public async Task<ActionResult<List<QuizQuestion>>> GenerateQuiz()
    {
        var vocabularies = await _vocabularyService.GetAllVocabularyAsync();

        if (!vocabularies.Any())
            return BadRequest(new { message = "Không có từ vựng để tạo bài quiz" });

        var questions = await _quizService.GenerateQuizQuestionsAsync(vocabularies);
        return Ok(questions);
    }

    [HttpPost("submit")]
    public async Task<ActionResult<object>> SubmitAnswer([FromBody] QuizSubmissionRequest request)
    {
        var isCorrect = await _quizService.ValidateAnswerAsync(request.QuestionId, request.Answer);
        return Ok(new
        {
            isCorrect,
            message = isCorrect ? "Chính xác!" : "Sai rồi, hãy thử lại!"
        });
    }
}

public class QuizSubmissionRequest
{
    public int QuestionId { get; set; }
    public string Answer { get; set; } = string.Empty;
}
