using Microsoft.EntityFrameworkCore;
using PsychologicalSupport.Application.DTOs.Psychologist;
using PsychologicalSupport.Application.Interfaces;
using PsychologicalSupport.Domain.Entities;

namespace PsychologicalSupport.Application.Services;

public class PsychologistService : IPsychologistService
{
    private readonly IRepository<Psychologist> _psychologistRepo;
    private readonly IRepository<Specialization> _specializationRepo;
    private readonly IRepository<PsychologistSpecialization> _psychSpecRepo;

    public PsychologistService(
        IRepository<Psychologist> psychologistRepo,
        IRepository<Specialization> specializationRepo,
        IRepository<PsychologistSpecialization> psychSpecRepo)
    {
        _psychologistRepo = psychologistRepo;
        _specializationRepo = specializationRepo;
        _psychSpecRepo = psychSpecRepo;
    }

    public async Task<IEnumerable<PsychologistDto>> GetAllAsync(PsychologistFilterDto filter)
    {
        var query = _psychologistRepo.Query()
            .Include(p => p.User)
            .Include(p => p.Specializations)
                .ThenInclude(ps => ps.Specialization)
            .AsQueryable();

        if (filter.OnlyVerified)
            query = query.Where(p => p.IsVerified);

        if (!string.IsNullOrEmpty(filter.Language))
            query = query.Where(p => p.Languages.Contains(filter.Language));

        if (!string.IsNullOrEmpty(filter.WorkFormat))
            query = query.Where(p => p.WorkFormats.Contains(filter.WorkFormat));

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.PricePerSession <= filter.MaxPrice.Value);

        if (filter.SpecializationIds?.Any() == true)
            query = query.Where(p => p.Specializations.Any(s => filter.SpecializationIds.Contains(s.SpecializationId)));

        var psychologists = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return psychologists.Select(MapToDto);
    }

    public async Task<PsychologistDto?> GetByIdAsync(Guid id)
    {
        var psychologist = await _psychologistRepo.Query()
            .Include(p => p.User)
            .Include(p => p.Specializations)
                .ThenInclude(ps => ps.Specialization)
            .FirstOrDefaultAsync(p => p.Id == id);

        return psychologist is null ? null : MapToDto(psychologist);
    }

    public async Task<PsychologistDto?> GetByUserIdAsync(Guid userId)
    {
        var psychologist = await _psychologistRepo.Query()
            .Include(p => p.User)
            .Include(p => p.Specializations)
                .ThenInclude(ps => ps.Specialization)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        return psychologist is null ? null : MapToDto(psychologist);
    }

    public async Task<PsychologistDto> CreateProfileAsync(Guid userId, CreatePsychologistProfileDto dto)
    {
        var psychologist = new Psychologist
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Education = dto.Education,
            ApproachDescription = dto.ApproachDescription,
            Languages = string.Join(",", dto.Languages),
            WorkFormats = string.Join(",", dto.WorkFormats),
            PricePerSession = dto.PricePerSession,
            MeetingLink = dto.MeetingLink,
            CreatedAt = DateTime.UtcNow
        };

        await _psychologistRepo.AddAsync(psychologist);

        // Add specializations
        foreach (var specId in dto.SpecializationIds)
        {
            await _psychSpecRepo.AddAsync(new PsychologistSpecialization
            {
                PsychologistId = psychologist.Id,
                SpecializationId = specId
            });
        }

        return (await GetByIdAsync(psychologist.Id))!;
    }

    public async Task<PsychologistDto> UpdateProfileAsync(Guid psychologistId, UpdatePsychologistProfileDto dto)
    {
        var psychologist = await _psychologistRepo.Query()
            .Include(p => p.Specializations)
            .FirstOrDefaultAsync(p => p.Id == psychologistId)
            ?? throw new InvalidOperationException("Psychologist not found");

        psychologist.Education = dto.Education;
        psychologist.ApproachDescription = dto.ApproachDescription;
        psychologist.Languages = string.Join(",", dto.Languages);
        psychologist.WorkFormats = string.Join(",", dto.WorkFormats);
        psychologist.PricePerSession = dto.PricePerSession;
        psychologist.MeetingLink = dto.MeetingLink;

        await _psychologistRepo.UpdateAsync(psychologist);

        // Update specializations
        var existingSpecIds = psychologist.Specializations.Select(s => s.SpecializationId).ToList();
        var toRemove = existingSpecIds.Except(dto.SpecializationIds);
        var toAdd = dto.SpecializationIds.Except(existingSpecIds);

        foreach (var specId in toRemove)
        {
            var spec = psychologist.Specializations.First(s => s.SpecializationId == specId);
            await _psychSpecRepo.DeleteAsync(spec);
        }

        foreach (var specId in toAdd)
        {
            await _psychSpecRepo.AddAsync(new PsychologistSpecialization
            {
                PsychologistId = psychologistId,
                SpecializationId = specId
            });
        }

        return (await GetByIdAsync(psychologistId))!;
    }

    public async Task<string?> UploadPhotoAsync(Guid psychologistId, Stream fileStream, string fileName)
    {
        var psychologist = await _psychologistRepo.GetByIdAsync(psychologistId);
        if (psychologist is null) return null;

        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "photos");
        Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{psychologistId}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        await using var fs = new FileStream(filePath, FileMode.Create);
        await fileStream.CopyToAsync(fs);

        psychologist.PhotoPath = $"/uploads/photos/{uniqueFileName}";
        await _psychologistRepo.UpdateAsync(psychologist);

        return psychologist.PhotoPath;
    }

    public async Task<bool> VerifyPsychologistAsync(Guid psychologistId, bool verified)
    {
        var psychologist = await _psychologistRepo.GetByIdAsync(psychologistId);
        if (psychologist is null) return false;

        psychologist.IsVerified = verified;
        await _psychologistRepo.UpdateAsync(psychologist);
        return true;
    }

    public async Task<IEnumerable<SpecializationDto>> GetAllSpecializationsAsync(string language)
    {
        var specs = await _specializationRepo.GetAllAsync();
        return specs.Select(s => new SpecializationDto(
            s.Id,
            s.Key,
            language == "tj" ? s.NameTj : s.NameRu
        ));
    }

    private static PsychologistDto MapToDto(Psychologist p) => new(
        p.Id,
        p.UserId,
        p.User?.FirstName,
        p.User?.LastName,
        p.PhotoPath,
        p.ExperienceYears,
        p.Education,
        p.ApproachDescription,
        p.Languages.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        p.WorkFormats.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
        p.PricePerSession,
        p.MeetingLink,
        p.IsVerified,
        p.Specializations.Select(s => new SpecializationDto(
            s.Specialization.Id,
            s.Specialization.Key,
            s.Specialization.NameRu
        )).ToList()
    );
}
