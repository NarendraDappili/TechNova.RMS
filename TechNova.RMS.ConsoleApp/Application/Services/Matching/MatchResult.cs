using System.Collections.Generic;

namespace TechNova.RMS.ConsoleApp.Application.Services.Matching;

public sealed record MatchResult
(
    System.Guid EmployeeId,
    string EmployeeName,
    int Score,
    IEnumerable<string> MatchedSkills,
    IEnumerable<string> MissingSkills
);
