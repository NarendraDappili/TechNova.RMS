using System.Collections.Generic;

namespace TechNova.RMS.ConsoleApp.Application.Services.Matching;

public sealed record MatchResult
(
    int EmployeeId,
    string EmployeeName,
    int Score,
    IEnumerable<string> MatchedSkills,
    IEnumerable<string> MissingSkills
);
