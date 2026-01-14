using Microsoft.EntityFrameworkCore;
using PsychologicalSupport.Application.DTOs.Booking;
using PsychologicalSupport.Application.Interfaces;
using PsychologicalSupport.Domain.Entities;
using PsychologicalSupport.Domain.Enums;

namespace PsychologicalSupport.Application.Services;

public class BookingService : IBookingService
{
    private readonly IRepository<Session> _sessionRepo;
    private readonly IRepository<Availability> _availabilityRepo;
    private readonly IRepository<Psychologist> _psychologistRepo;

    public BookingService(
        IRepository<Session> sessionRepo,
        IRepository<Availability> availabilityRepo,
        IRepository<Psychologist> psychologistRepo)
    {
        _sessionRepo = sessionRepo;
        _availabilityRepo = availabilityRepo;
        _psychologistRepo = psychologistRepo;
    }

    public async Task<IEnumerable<TimeSlotDto>> GetAvailableSlotsAsync(Guid psychologistId, DateOnly date)
    {
        var availability = await _availabilityRepo.Query()
            .FirstOrDefaultAsync(a => a.PsychologistId == psychologistId && a.DayOfWeek == date.DayOfWeek);

        if (availability is null)
            return [];

        // Get already booked slots for this date
        var dateTime = date.ToDateTime(TimeOnly.MinValue);
        var nextDay = dateTime.AddDays(1);

        var bookedSlots = await _sessionRepo.Query()
            .Where(s => s.PsychologistId == psychologistId
                     && s.ScheduledAt >= dateTime
                     && s.ScheduledAt < nextDay
                     && s.Status != SessionStatus.Cancelled)
            .Select(s => s.ScheduledAt.TimeOfDay)
            .ToListAsync();

        var slots = new List<TimeSlotDto>();
        var current = availability.StartTime;

        while (current.AddMinutes(availability.SlotDurationMinutes) <= availability.EndTime)
        {
            if (!bookedSlots.Contains(current.ToTimeSpan()))
            {
                slots.Add(new TimeSlotDto(
                    current,
                    current.AddMinutes(availability.SlotDurationMinutes),
                    availability.SlotDurationMinutes
                ));
            }
            current = current.AddMinutes(availability.SlotDurationMinutes);
        }

        return slots;
    }

    public async Task<SessionDto> BookSessionAsync(Guid clientId, BookSessionDto dto)
    {
        var psychologist = await _psychologistRepo.Query()
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == dto.PsychologistId)
            ?? throw new InvalidOperationException("Psychologist not found");

        // Verify slot is available
        var date = DateOnly.FromDateTime(dto.ScheduledAt);
        var availableSlots = await GetAvailableSlotsAsync(dto.PsychologistId, date);
        var requestedTime = TimeOnly.FromDateTime(dto.ScheduledAt);

        if (!availableSlots.Any(s => s.StartTime == requestedTime))
            throw new InvalidOperationException("Slot is not available");

        var session = new Session
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            PsychologistId = dto.PsychologistId,
            ScheduledAt = dto.ScheduledAt,
            Status = SessionStatus.Pending,
            MeetingLink = psychologist.MeetingLink,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        await _sessionRepo.AddAsync(session);

        // TODO: Send email confirmation

        return await MapToDto(session);
    }

    public async Task<IEnumerable<SessionDto>> GetClientSessionsAsync(Guid clientId)
    {
        var sessions = await _sessionRepo.Query()
            .Include(s => s.Psychologist).ThenInclude(p => p.User)
            .Include(s => s.Client)
            .Where(s => s.ClientId == clientId)
            .OrderByDescending(s => s.ScheduledAt)
            .ToListAsync();

        return sessions.Select(s => MapToDtoSync(s));
    }

    public async Task<IEnumerable<SessionDto>> GetPsychologistSessionsAsync(Guid psychologistId)
    {
        var sessions = await _sessionRepo.Query()
            .Include(s => s.Psychologist).ThenInclude(p => p.User)
            .Include(s => s.Client)
            .Where(s => s.PsychologistId == psychologistId)
            .OrderByDescending(s => s.ScheduledAt)
            .ToListAsync();

        return sessions.Select(s => MapToDtoSync(s));
    }

    public async Task<SessionDto?> GetSessionByIdAsync(Guid sessionId)
    {
        var session = await _sessionRepo.Query()
            .Include(s => s.Psychologist).ThenInclude(p => p.User)
            .Include(s => s.Client)
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        return session is null ? null : MapToDtoSync(session);
    }

    public async Task<bool> CancelSessionAsync(Guid sessionId, Guid requesterId)
    {
        var session = await _sessionRepo.Query()
            .FirstOrDefaultAsync(s => s.Id == sessionId);

        if (session is null) return false;
        if (session.ClientId != requesterId && session.PsychologistId != requesterId) return false;

        session.Status = SessionStatus.Cancelled;
        await _sessionRepo.UpdateAsync(session);
        return true;
    }

    public async Task<bool> ConfirmSessionAsync(Guid sessionId, Guid psychologistId)
    {
        var session = await _sessionRepo.Query()
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.PsychologistId == psychologistId);

        if (session is null) return false;

        session.Status = SessionStatus.Confirmed;
        await _sessionRepo.UpdateAsync(session);
        return true;
    }

    private async Task<SessionDto> MapToDto(Session s)
    {
        var session = await _sessionRepo.Query()
            .Include(x => x.Psychologist).ThenInclude(p => p.User)
            .Include(x => x.Client)
            .FirstAsync(x => x.Id == s.Id);

        return MapToDtoSync(session);
    }

    private static SessionDto MapToDtoSync(Session s) => new(
        s.Id,
        s.ClientId,
        $"{s.Client?.FirstName} {s.Client?.LastName}".Trim(),
        s.PsychologistId,
        $"{s.Psychologist?.User?.FirstName} {s.Psychologist?.User?.LastName}".Trim(),
        s.ScheduledAt,
        s.Status,
        s.MeetingLink,
        s.Notes
    );
}
