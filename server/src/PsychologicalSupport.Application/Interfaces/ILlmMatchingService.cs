using PsychologicalSupport.Application.DTOs.Matching;
using PsychologicalSupport.Application.DTOs.Psychologist;

namespace PsychologicalSupport.Application.Interfaces;

public interface ILlmMatchingService
{
    Task<List<LlmMatchResult>> GetMatchesAsync(
        QuestionnaireSubmitDto questionnaire,
        List<PsychologistDto> availablePsychologists);
}
