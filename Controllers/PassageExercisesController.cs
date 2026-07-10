using Microsoft.AspNetCore.Mvc;
using Web_Learning_Vocabulary.Models;
using Web_Learning_Vocabulary.Services;

namespace Web_Learning_Vocabulary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PassageExercisesController : ControllerBase
{
    private readonly IPassageExerciseService _passageExerciseService;
    private readonly IVocabularyGeneratorService _vocabularyGeneratorService;
    private readonly IVocabularyService _vocabularyService;

    public PassageExercisesController(
        IPassageExerciseService passageExerciseService,
        IVocabularyGeneratorService vocabularyGeneratorService,
        IVocabularyService vocabularyService)
    {
        _passageExerciseService = passageExerciseService;
        _vocabularyGeneratorService = vocabularyGeneratorService;
        _vocabularyService = vocabularyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PassageExercise>>> GetAllExercises()
    {
        var exercises = await _passageExerciseService.GetAllExercisesAsync();
        return Ok(exercises);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PassageExercise>> GetExerciseById(int id)
    {
        var exercise = await _passageExerciseService.GetExerciseByIdAsync(id);
        if (exercise == null)
            return NotFound(new { message = "Bài tập không tìm thấy" });

        return Ok(exercise);
    }

    [HttpPost]
    public async Task<ActionResult<PassageExercise>> CreateExercise([FromBody] PassageExercise exercise)
    {
        if (string.IsNullOrWhiteSpace(exercise.Title))
            return BadRequest(new { message = "Tiêu đề bài tập không được để trống" });

        if (string.IsNullOrWhiteSpace(exercise.OriginalPassage))
            return BadRequest(new { message = "Đoạn văn không được để trống" });

        var newExercise = await _passageExerciseService.CreateExerciseAsync(exercise);
        return CreatedAtAction(nameof(GetExerciseById), new { id = newExercise.Id }, newExercise);
    }

    [HttpPost("generate")]
    public async Task<ActionResult<PassageExercise>> GenerateExercise()
    {
        var vocabularies = await _vocabularyService.GetAllVocabularyAsync();

        if (vocabularies.Count == 0)
            return BadRequest(new { message = "Cần ít nhất 1 từ vựng để tạo bài tập" });

        var random = new Random();
        var selectedVocabs = vocabularies.OrderBy(x => random.Next()).Take(10).ToList();

        var request = new VocabularyGenerateRequest { VocabularyList = selectedVocabs };

        try
        {
            var response = await _vocabularyGeneratorService.GenerateLearningMaterialAsync(request);

            var displayText = response.DisplayText;
            foreach (var b in response.Blanks)
            {
                displayText = displayText.Replace(b.Id, "_____");
            }

            var exercise = new PassageExercise
            {
                Title = response.Title,
                OriginalPassage = response.FullText,
                PassageWithBlanks = displayText,
                BlankWords = response.Blanks.Select((b, index) => new BlankWord
                {
                    Id = index + 1,
                    EnglishWord = b.CorrectAnswer,
                    VietnameseMeaning = b.HintVi,
                    Position = index + 1
                }).ToList()
            };

            var created = await _passageExerciseService.CreateExerciseAsync(exercise);
            return Ok(created);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("OpenAI API key"))
        {
            return StatusCode(500, new { message = "Lỗi: Chưa cấu hình API key của OpenAI. Vui lòng liên hệ quản trị viên.", error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(500, new { message = "Lỗi kết nối đến OpenAI API. Vui lòng kiểm tra kết nối internet hoặc thử lại sau.", error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { message = "Lỗi: " + ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi gọi AI API để tạo bài tập. Chi tiết: " + ex.Message });
        }
    }

    [HttpPost("{id}/submit")]
    public async Task<ActionResult<PassageSubmission>> SubmitAnswers(int id, [FromBody] List<BlankAnswer> answers)
    {
        try
        {
            var submission = await _passageExerciseService.SubmitAnswersAsync(id, answers);
            return Ok(submission);
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Bài tập không tìm thấy" });
        }
    }
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteExercise(int id)
    {
        var result = await _passageExerciseService.DeleteExerciseAsync(id);
        if (!result)
            return NotFound(new { message = "Bài tập không tìm thấy" });

        return Ok(new { message = "Xóa bài tập thành công" });
    }
}
