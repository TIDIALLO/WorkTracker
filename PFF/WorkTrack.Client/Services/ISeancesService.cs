// WorkTrack.Client/Services/ISeancesService.cs
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services;

public interface ISeancesService
{
    Task<List<SeanceDto>> GetTodayAsync(CancellationToken ct = default);
    Task<List<RosterStudentDto>> GetRosterAsync(Guid seanceId, CancellationToken ct = default);
    Task<List<SeanceDto>> GetAsync(DateTimeOffset? from = null, DateTimeOffset? to = null,
        Guid? enseignantId = null, Guid? promotionId = null, CancellationToken ct = default);
    Task<List<AttendanceRowDto>> GetAttendanceListAsync(Guid seanceId, CancellationToken ct = default);

}
