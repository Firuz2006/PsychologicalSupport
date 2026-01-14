using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PsychologicalSupport.Application.DTOs.Matching;
using PsychologicalSupport.Application.DTOs.Psychologist;
using PsychologicalSupport.Application.Interfaces;

namespace PsychologicalSupport.Infrastructure.ExternalServices;

public class LlmMatchingService : ILlmMatchingService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpoint;
    private readonly string _model;

    public LlmMatchingService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _apiKey = configuration["Llm:ApiKey"] ?? "";
        _endpoint = configuration["Llm:Endpoint"] ?? "https://api.openai.com/v1/chat/completions";
        _model = configuration["Llm:Model"] ?? "gpt-4o-mini";
    }

    public async Task<List<LlmMatchResult>> GetMatchesAsync(
        QuestionnaireSubmitDto questionnaire,
        List<PsychologistDto> availablePsychologists)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            // Fallback to simple matching if no API key
            return SimpleFallbackMatching(questionnaire, availablePsychologists);
        }

        var prompt = BuildMatchingPrompt(questionnaire, availablePsychologists);

        var request = new
        {
            model = _model,
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = """
                        You are a psychologist matching algorithm. Your task is to analyze a client's needs
                        and match them with the most suitable psychologists from the available list.

                        Consider these factors:
                        1. Specialization match with the client's main issue
                        2. Language preference
                        3. Format preference (online/offline)
                        4. Urgency level (higher urgency may need more experienced psychologists)
                        5. Price considerations

                        Return a JSON array with exactly 3 matches (or fewer if not enough suitable psychologists).
                        Each match should have: psychologistId (GUID), score (1-100), reason (brief explanation).

                        IMPORTANT: Return ONLY valid JSON, no other text. Format:
                        [{"psychologistId":"guid","score":85,"reason":"explanation"},...]
                        """
                },
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 1000
        };

        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

            var response = await _httpClient.PostAsJsonAsync(_endpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                return SimpleFallbackMatching(questionnaire, availablePsychologists);
            }

            var result = await response.Content.ReadFromJsonAsync<LlmResponse>();
            var content = result?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrEmpty(content))
            {
                return SimpleFallbackMatching(questionnaire, availablePsychologists);
            }

            // Parse JSON response
            var matches = JsonSerializer.Deserialize<List<LlmMatchResultRaw>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return matches?.Select(m => new LlmMatchResult(
                Guid.Parse(m.PsychologistId),
                m.Score,
                m.Reason
            )).ToList() ?? SimpleFallbackMatching(questionnaire, availablePsychologists);
        }
        catch
        {
            return SimpleFallbackMatching(questionnaire, availablePsychologists);
        }
    }

    private static string BuildMatchingPrompt(QuestionnaireSubmitDto q, List<PsychologistDto> psychologists)
    {
        var psychList = string.Join("\n", psychologists.Select(p =>
            $"- ID: {p.Id}, Name: {p.FirstName} {p.LastName}, Experience: {p.ExperienceYears} years, " +
            $"Specializations: {string.Join(", ", p.Specializations.Select(s => s.Key))}, " +
            $"Languages: {string.Join(", ", p.Languages)}, Formats: {string.Join(", ", p.WorkFormats)}, " +
            $"Price: {p.PricePerSession}"
        ));

        return $"""
            CLIENT INFORMATION:
            - Gender: {q.Gender}
            - Age: {q.Age}
            - Preferred Language: {q.PreferredLanguage}
            - Main Issue: {q.MainIssue}
            - Urgency Level: {q.UrgencyLevel}
            - Format Preference: {q.FormatPreference}
            - Additional Info: {q.AdditionalInfo ?? "None"}

            AVAILABLE PSYCHOLOGISTS:
            {psychList}

            Please select the top 3 most suitable psychologists for this client and explain why.
            """;
    }

    private static List<LlmMatchResult> SimpleFallbackMatching(
        QuestionnaireSubmitDto questionnaire,
        List<PsychologistDto> psychologists)
    {
        // Simple rule-based matching when LLM is unavailable
        var scored = psychologists.Select(p =>
        {
            var score = 50; // base score

            // Language match
            if (p.Languages.Contains(questionnaire.PreferredLanguage, StringComparer.OrdinalIgnoreCase))
                score += 20;

            // Format match
            if (p.WorkFormats.Contains(questionnaire.FormatPreference, StringComparer.OrdinalIgnoreCase)
                || p.WorkFormats.Contains("both", StringComparer.OrdinalIgnoreCase))
                score += 15;

            // Specialization match (simple keyword matching)
            var issue = questionnaire.MainIssue.ToLower();
            if (p.Specializations.Any(s => issue.Contains(s.Key) || s.Key.Contains(issue)))
                score += 25;

            // Experience bonus for high urgency
            if (questionnaire.UrgencyLevel == "high" && p.ExperienceYears >= 5)
                score += 10;

            return new { Psychologist = p, Score = Math.Min(score, 100) };
        })
        .OrderByDescending(x => x.Score)
        .Take(3)
        .ToList();

        return scored.Select(x => new LlmMatchResult(
            x.Psychologist.Id,
            x.Score,
            $"Matched based on language, format, and specialization compatibility"
        )).ToList();
    }

    private record LlmResponse
    {
        public List<LlmChoice>? Choices { get; init; }
    }

    private record LlmChoice
    {
        public LlmMessage? Message { get; init; }
    }

    private record LlmMessage
    {
        public string? Content { get; init; }
    }

    private record LlmMatchResultRaw
    {
        public string PsychologistId { get; init; } = "";
        public int Score { get; init; }
        public string Reason { get; init; } = "";
    }
}
