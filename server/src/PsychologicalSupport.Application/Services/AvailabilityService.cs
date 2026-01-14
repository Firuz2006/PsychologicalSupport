using Microsoft.EntityFrameworkCore;
using PsychologicalSupport.Application.DTOs.Booking;
using PsychologicalSupport.Application.Interfaces;
using PsychologicalSupport.Domain.Entities;

namespace PsychologicalSupport.Application.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly IRepository<Availability> _availabilityRepo;

    public AvailabilityService(IRepository<Availability> availabilityRepo)
    {
        _availabilityRepo = availabilityRepo;
    }

    public async Task<IEnumerable<AvailabilityDto>> GetPsychologistAvailabilityAsync(Guid psychologistId)
    {
        var availabilities = await _availabilityRepo.Query()
            .Where(a => a.PsychologistId == psychologistId)
            .OrderBy(a => a.DayOfWeek)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return availabilities.Select(a => new AvailabilityDto(
            a.Id,
            a.DayOfWeek,
            a.StartTime,
            a.EndTime,
            a.SlotDurationMinutes
        ));
    }

    public async Task<AvailabilityDto> SetAvailabilityAsync(Guid psychologistId, SetAvailabilityDto dto)
    {
        // Check for existing availability on same day
        var existing = await _availabilityRepo.Query()
            .FirstOrDefaultAsync(a => a.PsychologistId == psychologistId && a.DayOfWeek == dto.DayOfWeek);

        if (existing is not null)
        {
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.SlotDurationMinutes = dto.SlotDurationMinutes;
            await _availabilityRepo.UpdateAsync(existing);

            return new AvailabilityDto(
                existing.Id,
                existing.DayOfWeek,
                existing.StartTime,
                existing.EndTime,
                existing.SlotDurationMinutes
            );
        }

        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            PsychologistId = psychologistId,
            DayOfWeek = dto.DayOfWeek,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            SlotDurationMinutes = dto.SlotDurationMinutes
        };

        await _availabilityRepo.AddAsync(availability);

        return new AvailabilityDto(
            availability.Id,
            availability.DayOfWeek,
            availability.StartTime,
            availability.EndTime,
            availability.SlotDurationMinutes
        );
    }

    public async Task<bool> RemoveAvailabilityAsync(Guid availabilityId, Guid psychologistId)
    {
        var availability = await _availabilityRepo.Query()
            .FirstOrDefaultAsync(a => a.Id == availabilityId && a.PsychologistId == psychologistId);

        if (availability is null) return false;

        await _availabilityRepo.DeleteAsync(availability);
        return true;
    }
}
