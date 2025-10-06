// WorkTrack.Client/Services/SeancesService.cs
using System.Net.Http.Json;
using System.Web;
using WorkTrack.Client.Models;

namespace WorkTrack.Client.Services;

public class SeancesService(HttpClient http) : ISeancesService
{
    public async Task<List<SeanceDto>> GetTodayAsync(CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<SeanceDto>>("api/seances/today", ct) ?? new();

    public async Task<List<RosterStudentDto>> GetRosterAsync(Guid seanceId, CancellationToken ct = default)
        => await http.GetFromJsonAsync<List<RosterStudentDto>>($"api/seances/{seanceId}/roster", ct) ?? new();

    public async Task<List<SeanceDto>> GetAsync(DateTimeOffset? from = null, DateTimeOffset? to = null,
        Guid? enseignantId = null, Guid? promotionId = null, CancellationToken ct = default)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        if (from.HasValue) query["from"] = from.Value.ToString("O");
        if (to.HasValue) query["to"] = to.Value.ToString("O");
        if (enseignantId.HasValue) query["enseignantId"] = enseignantId.Value.ToString();
        if (promotionId.HasValue) query["promotionId"] = promotionId.Value.ToString();

        var url = "api/seances";
        var qs = query.ToString();
        if (!string.IsNullOrWhiteSpace(qs)) url += "?" + qs;

        return await http.GetFromJsonAsync<List<SeanceDto>>(url, ct) ?? new();
    }

    public async Task<List<AttendanceRowDto>> GetAttendanceListAsync(Guid seanceId, CancellationToken ct = default)
    => await http.GetFromJsonAsync<List<AttendanceRowDto>>($"api/seances/{seanceId}/attendance/list", ct) ?? new();

}
