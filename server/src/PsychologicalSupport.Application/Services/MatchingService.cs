using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PsychologicalSupport.Application.DTOs.Matching;
using PsychologicalSupport.Application.DTOs.Psychologist;
using PsychologicalSupport.Application.Interfaces;
using PsychologicalSupport.Domain.Entities;

namespace PsychologicalSupport.Application.Services;

public class MatchingService : IMatchingService
{
    private readonly IRepository<QuestionnaireResponse> _questionnaireRepo;
    private readonly IPsychologistService _psychologistService;
    private readonly ILlmMatchingService _llmService;

    public MatchingService(
        IRepository<QuestionnaireResponse> questionnaireRepo,
        IPsychologistService psychologistService,
        ILlmMatchingService llmService)
    {
        _questionnaireRepo = questionnaireRepo;
        _psychologistService = psychologistService;
        _llmService = llmService;
    }

    public async Task<List<PsychologistMatchDto>> ProcessQuestionnaireAsync(QuestionnaireSubmitDto dto, Guid? userId)
    {
        // 1. Save questionnaire
        var questionnaire = new QuestionnaireResponse
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            GuestSessionId = dto.GuestSessionId,
            Gender = dto.Gender,
            Age = dto.Age,
            PreferredLanguage = dto.PreferredLanguage,
            MainIssue = dto.MainIssue,
            UrgencyLevel = dto.UrgencyLevel,
            FormatPreference = dto.FormatPreference,
            AdditionalInfo = dto.AdditionalInfo,
            CreatedAt = DateTime.UtcNow
        };

        await _questionnaireRepo.AddAsync(questionnaire);

        // 2. Get verified psychologists
        var filter = new PsychologistFilterDto(
            Language: dto.PreferredLanguage,
            WorkFormat: dto.FormatPreference != "any" ? dto.FormatPreference : null,
            SpecializationIds: null,
            MaxPrice: null,
            OnlyVerified: true,
            Page: 1,
            PageSize: 50 // Get more for better matching
        );

        var psychologists = (await _psychologistService.GetAllAsync(filter)).ToList();

        if (!psychologists.Any())
        {
            // Fallback: get all verified psychologists regardless of filters
            filter = filter with { Language = null, WorkFormat = null };
            psychologists = (await _psychologistService.GetAllAsync(filter)).ToList();
        }

        if (!psychologists.Any())
        {
            return [];
        }

        // 3. Call LLM for matching
        var llmMatches = await _llmService.GetMatchesAsync(dto, psychologists);

        // 4. Save recommendation
        questionnaire.LlmRecommendation = JsonSerializer.Serialize(llmMatches);
        await _questionnaireRepo.UpdateAsync(questionnaire);

        // 5. Build result with psychologist details
        var result = new List<PsychologistMatchDto>();

        foreach (var match in llmMatches.Take(3))
        {
            var psychologist = psychologists.FirstOrDefault(p => p.Id == match.PsychologistId);
            if (psychologist is null) continue;

            result.Add(new PsychologistMatchDto(
                psychologist.Id,
                $"{psychologist.FirstName} {psychologist.LastName}".Trim(),
                psychologist.PhotoPath,
                psychologist.ExperienceYears,
                psychologist.PricePerSession,
                psychologist.Specializations.Select(s => s.Name).ToList(),
                match.Reason,
                match.Score
            ));
        }

        return result;
    }
}
