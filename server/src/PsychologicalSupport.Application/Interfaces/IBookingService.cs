using PsychologicalSupport.Application.DTOs.Booking;

namespace PsychologicalSupport.Application.Interfaces;

public interface IBookingService
{
    Task<IEnumerable<TimeSlotDto>> GetAvailableSlotsAsync(Guid psychologistId, DateOnly date);
    Task<SessionDto> BookSessionAsync(Guid clientId, BookSessionDto dto);
    Task<IEnumerable<SessionDto>> GetClientSessionsAsync(Guid clientId);
    Task<IEnumerable<SessionDto>> GetPsychologistSessionsAsync(Guid psychologistId);
    Task<SessionDto?> GetSessionByIdAsync(Guid sessionId);
    Task<bool> CancelSessionAsync(Guid sessionId, Guid requesterId);
    Task<bool> ConfirmSessionAsync(Guid sessionId, Guid psychologistId);
}
