using HakedisCheck.Core.Aggregation;
using HakedisCheck.Core.Models;

namespace HakedisCheck.Core.Matching;

public sealed class MatchedEmployee
{
    public string DisplayName { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string? IdentityNumber { get; set; }
    public LeaveAggregate? Leave { get; set; }
    public MesaiAggregate? Mesai { get; set; }
    public HakedisEntry? Hakedis { get; set; }
}
