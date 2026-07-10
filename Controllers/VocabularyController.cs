using Microsoft.AspNetCore.Mvc;
using Web_Learning_Vocabulary.Models;
using Web_Learning_Vocabulary.Services;

namespace Web_Learning_Vocabulary.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyGeneratorService _generatorService;
    private readonly ILogger<VocabularyController> _logger;

    public VocabularyController(IVocabularyGeneratorService generatorService, ILogger<VocabularyController> logger)
    {
        _generatorService = generatorService;
        _logger = logger;
    }

    /// <summary>
    /// Generate vocabulary learning materials from a list of vocabulary items
    /// </summary>
    /// <param name="request">Vocabulary list to generate materials for</param>
    /// <returns>Learning material with fill-in-the-blank exercise</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(VocabularyGenerateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VocabularyGenerateResponse>> GenerateMaterials([FromBody] VocabularyGenerateRequest request)
    {
        try
        {
            if (request?.VocabularyList == null || request.VocabularyList.Count == 0)
            {
                return BadRequest(new { error = "Vocabulary list cannot be empty" });
            }

            _logger.LogInformation("Generating vocabulary learning materials for {Count} words", request.VocabularyList.Count);

            var result = await _generatorService.GenerateLearningMaterialAsync(request);

            _logger.LogInformation("Successfully generated learning materials");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating learning materials");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while generating learning materials" });
        }
    }
}
