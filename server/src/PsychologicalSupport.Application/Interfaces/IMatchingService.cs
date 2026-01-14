using PsychologicalSupport.Application.DTOs.Matching;

namespace PsychologicalSupport.Application.Interfaces;

public interface IMatchingService
{
    Task<List<PsychologistMatchDto>> ProcessQuestionnaireAsync(QuestionnaireSubmitDto dto, Guid? userId);
}
