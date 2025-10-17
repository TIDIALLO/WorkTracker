// WorkTrack.Client/Services/IAttendanceService.cs
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services;

public interface IAttendanceService
{
    Task<bool> SubmitAsync(Guid seanceId, IEnumerable<AttendanceMarkDto> marks, CancellationToken ct = default);
}
