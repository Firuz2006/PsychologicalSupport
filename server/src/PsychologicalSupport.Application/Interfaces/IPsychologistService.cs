using PsychologicalSupport.Application.DTOs.Psychologist;

namespace PsychologicalSupport.Application.Interfaces;

public interface IPsychologistService
{
    Task<IEnumerable<PsychologistDto>> GetAllAsync(PsychologistFilterDto filter);
    Task<PsychologistDto?> GetByIdAsync(Guid id);
    Task<PsychologistDto?> GetByUserIdAsync(Guid userId);
    Task<PsychologistDto> CreateProfileAsync(Guid userId, CreatePsychologistProfileDto dto);
    Task<PsychologistDto> UpdateProfileAsync(Guid psychologistId, UpdatePsychologistProfileDto dto);
    Task<string?> UploadPhotoAsync(Guid psychologistId, Stream fileStream, string fileName);
    Task<bool> VerifyPsychologistAsync(Guid psychologistId, bool verified);
    Task<IEnumerable<SpecializationDto>> GetAllSpecializationsAsync(string language);
}
