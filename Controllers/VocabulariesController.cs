using Microsoft.AspNetCore.Mvc;
using Web_Learning_Vocabulary.Models;
using Web_Learning_Vocabulary.Services;

namespace Web_Learning_Vocabulary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabulariesController : ControllerBase
{
    private readonly IVocabularyService _vocabularyService;

    public VocabulariesController(IVocabularyService vocabularyService)
    {
        _vocabularyService = vocabularyService;
    }

    [HttpGet]
    public async Task<ActionResult<List<VocabularyItem>>> GetAllVocabularies()
    {
        var vocabularies = await _vocabularyService.GetAllVocabularyAsync();
        return Ok(vocabularies);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VocabularyItem>> GetVocabularyById(int id)
    {
        var vocabulary = await _vocabularyService.GetVocabularyByIdAsync(id);
        if (vocabulary == null)
            return NotFound(new { message = "Từ vựng không tìm thấy" });

        return Ok(vocabulary);
    }

    [HttpPost]
    public async Task<ActionResult<VocabularyItem>> AddVocabulary([FromBody] VocabularyItem vocabulary)
    {
        if (string.IsNullOrWhiteSpace(vocabulary.EnglishWord))
            return BadRequest(new { message = "Từ không được để trống" });

        if (string.IsNullOrWhiteSpace(vocabulary.VietnameseMeaning))
            return BadRequest(new { message = "Nghĩa tiếng Việt không được để trống" });

        var newVocabulary = await _vocabularyService.AddVocabularyAsync(vocabulary);
        return CreatedAtAction(nameof(GetVocabularyById), new { id = newVocabulary.Id }, newVocabulary);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VocabularyItem>> UpdateVocabulary(int id, [FromBody] VocabularyItem vocabulary)
    {
        var updatedVocabulary = await _vocabularyService.UpdateVocabularyAsync(id, vocabulary);
        if (updatedVocabulary == null)
            return NotFound(new { message = "Từ vựng không tìm thấy" });

        return Ok(updatedVocabulary);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVocabulary(int id)
    {
        var success = await _vocabularyService.DeleteVocabularyAsync(id);
        if (!success)
            return NotFound(new { message = "Từ vựng không tìm thấy" });

        return Ok(new { message = "Xóa từ vựng thành công" });
    }
}
