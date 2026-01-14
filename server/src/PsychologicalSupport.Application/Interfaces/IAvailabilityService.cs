using PsychologicalSupport.Application.DTOs.Booking;

namespace PsychologicalSupport.Application.Interfaces;

public interface IAvailabilityService
{
    Task<IEnumerable<AvailabilityDto>> GetPsychologistAvailabilityAsync(Guid psychologistId);
    Task<AvailabilityDto> SetAvailabilityAsync(Guid psychologistId, SetAvailabilityDto dto);
    Task<bool> RemoveAvailabilityAsync(Guid availabilityId, Guid psychologistId);
}
