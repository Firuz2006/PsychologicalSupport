using PsychologicalSupport.Domain.Enums;

namespace PsychologicalSupport.Application.DTOs.Booking;

public record SessionDto(
    Guid Id,
    Guid ClientId,
    string? ClientName,
    Guid PsychologistId,
    string? PsychologistName,
    DateTime ScheduledAt,
    SessionStatus Status,
    string? MeetingLink,
    string? Notes
);

public record BookSessionDto(
    Guid PsychologistId,
    DateTime ScheduledAt,
    string? Notes
);

public record TimeSlotDto(
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationMinutes
);

public record AvailabilityDto(
    Guid Id,
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes
);

public record SetAvailabilityDto(
    DayOfWeek DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int SlotDurationMinutes = 60
);
