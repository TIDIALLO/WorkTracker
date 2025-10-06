// WorkTrack.Client/Services/AttendanceService.cs
using System.Net.Http.Json;
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services;

public class AttendanceService(HttpClient http) : IAttendanceService
{
    public async Task<bool> SubmitAsync(Guid seanceId, IEnumerable<AttendanceMarkDto> marks, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync($"api/seances/{seanceId}/attendance", marks, ct);
        return resp.IsSuccessStatusCode;
    }
}
