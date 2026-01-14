using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PsychologicalSupport.Application.DTOs.Matching;
using PsychologicalSupport.Application.Interfaces;

namespace PsychologicalSupport.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MatchingController : ControllerBase
{
    private readonly IMatchingService _matchingService;

    public MatchingController(IMatchingService matchingService)
    {
        _matchingService = matchingService;
    }

    [HttpPost("questionnaire")]
    public async Task<IActionResult> SubmitQuestionnaire([FromBody] QuestionnaireSubmitDto dto)
    {
        Guid? userId = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim is not null)
                userId = Guid.Parse(claim.Value);
        }

        var matches = await _matchingService.ProcessQuestionnaireAsync(dto, userId);
        return Ok(matches);
    }
}
